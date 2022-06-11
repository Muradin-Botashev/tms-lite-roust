using Application.BusinessModels.Shared.Actions;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// Отменить бронь в Пулинге
    /// </summary>
    [ActionGroup(nameof(Shipping)), OrderNumber(17)]
    public class CancelPoolingReservation : IGroupAppAction<Order>
    {
        public AppColor Color { get; set; }

        public bool IsSingleAllowed => true;

        private readonly ICommonDataService _dataService;

        private readonly IOrderPoolingService _poolingService;

        private readonly IHistoryService _historyService;

        public CancelPoolingReservation(
            ICommonDataService dataService,
            IHistoryService historyService,
            IOrderPoolingService poolingService)
        {
            _poolingService = poolingService;
            _dataService = dataService;
            _historyService = historyService;

            Color = AppColor.Red;
        }

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> target)
        {
            var shipping = _dataService.GetById<Shipping>(target.First().ShippingId.Value);

            if (!_poolingService.CheckConsolidationDate(shipping))
            {
                return new AppResult
                {
                    IsError = true,
                    Message = "cancelBookedSlotOverdue".Translate(user.Language, shipping.ShippingNumber)
                };
            }

            var result = _poolingService.CancelSlot(shipping);

            var error = _poolingService.ValidateCancelSlot(result, shipping, user);

            if (error != null)
            {
                return error;
            }

            var bookNumber = _dataService.GetDbSet<Order>()
                .Where(i => i.ShippingId == shipping.Id)
                .Select(i => i.BookingNumber)
                .FirstOrDefault();

            _historyService.Save(shipping.Id, "bookedSlotCancelledHistory", bookNumber);

            return new AppResult
            {
                IsError = false,
                Message = "bookedSlotCancelled".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(IEnumerable<Order> target)
        {
            bool sameShipping = target.All(x => x.ShippingId.HasValue)
                && target.Select(x => x.ShippingId).Distinct().Count() == 1
                && target.All(x => x.OrderShippingStatus == ShippingState.ShippingSlotBooked);

            return sameShipping;
        }
    }
}
