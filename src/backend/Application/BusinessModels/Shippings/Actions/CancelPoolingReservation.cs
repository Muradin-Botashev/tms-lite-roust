using Application.BusinessModels.Shared.Actions;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System.Linq;

namespace Application.BusinessModels.Shippings.Actions
{
    public class CancelPoolingReservation : IAppAction<Shipping>
    {
        private readonly ICommonDataService _dataService;

        private readonly IHistoryService _historyService;

        private readonly IOrderPoolingService _poolingService;

        public AppColor Color { get; set; }

        public CancelPoolingReservation(
            ICommonDataService dataService,
            IHistoryService historyService,
            IOrderPoolingService poolingService)
        {
            _dataService = dataService;
            _historyService = historyService;
            _poolingService = poolingService;

            Color = AppColor.Red;
        }

        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
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

        public bool IsAvailable(Shipping target)
        {
            return target.Status == ShippingState.ShippingSlotBooked;
        }
    }
}
