using Application.BusinessModels.Shared.Actions;
using Application.Shared.Orders;
using Application.Shared.Pooling.Models;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Pooling.Models;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// В архив
    /// </summary>
    [ActionGroup(nameof(Shipping)), OrderNumber(16)]
    public class SendToPooling : BaseOrderAction, IGroupAppAction<Order>
    {
        private readonly IHistoryService _historyService;

        private readonly IOrderPoolingService _poolingService;

        private readonly IShippingActionService _shippingActionService;

        public SendToPooling(
            ICommonDataService dataService,
            IHistoryService historyService,
            IOrderPoolingService poolingService,
            IShippingActionService shippingActionService)
            : base(dataService)
        {
            _historyService = historyService;
            _poolingService = poolingService;
            _shippingActionService = shippingActionService;

            Color = AppColor.Blue;
        }

        public AppColor Color { get; set; }

        public bool IsSingleAllowed => true;

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> orders)
        {
            var validationResult = _poolingService.ValidateOrders(orders, user);

            if (validationResult != null)
            {
                return validationResult;
            }

            var tarifficationType = orders.FirstOrDefault()?.TarifficationType;

            HttpResult<SlotDto> slot = null;

            if (tarifficationType == TarifficationType.Pooling || tarifficationType == TarifficationType.Milkrun)
            {
                slot = _poolingService.GetSlot(null, orders.First());

                var slotResult = _poolingService.ValidateGetSlot(slot, user);

                if (slotResult != null)
                {
                    return slotResult;
                }
            }

            Shipping shipping = null;

            var allOrders = orders;
            var shippingId = orders.Where(x => x.ShippingId.HasValue).FirstOrDefault()?.ShippingId;

            if (shippingId == null)
            {
                shipping = _shippingActionService.UnionOrders(orders);
            }
            else
            {
                shipping = _dataService.GetById<Shipping>(shippingId.Value);
                allOrders = _dataService.GetDbSet<Order>().Where(i => i.ShippingId == shippingId);
            }

            var bookedSlot = _poolingService.BookSlot(shipping, orders, slot?.Result);

            var bookedSlotResult = _poolingService.ValidateBookedSlot(bookedSlot, user);

            if (bookedSlotResult != null)
            {
                return bookedSlotResult;
            }

            shipping.PoolingReservationId = bookedSlot.Result.Id;
            shipping.SlotId = bookedSlot.Result.SlotId;
            shipping.AvailableUntil = bookedSlot.Result.EditableUntil.ToDateTime();
            shipping.ConsolidationDate = slot?.Result?.ConsolidationDate?.ToDateTime();

            shipping.Status = ShippingState.ShippingSlotBooked;

            allOrders.ToList().ForEach(i => i.OrderShippingStatus = shipping.Status);
            allOrders.ToList().ForEach(i => i.BookingNumber = bookedSlot.Result.Number);

            shipping.IsPooling = bookedSlot.Result.ShippingType == "Pooling";

            allOrders.ToList().ForEach(x => x.IsPooling = shipping.IsPooling);

            _historyService.Save(shipping.Id, "orderSentToPooling", shipping.ShippingNumber);

            allOrders.ToList().ForEach(i => _historyService.Save(i.Id, "orderSentToPooling", shipping.ShippingNumber));

            return new AppResult
            {
                IsError = false,
                Message = "orderSentToPoolingAlert".Translate(user.Language, shipping.ShippingNumber),
                ManuallyClosableMessage = true
            };
        }

        public bool IsAvailable(IEnumerable<Order> orders)
        {
            if (orders == null || !orders.Any()) return false;

            var firstOrder = orders.First();
            return orders.All(i => i.ShippingId == firstOrder.ShippingId)
                && orders.All(i => i.DeliveryType == DeliveryType.Delivery)
                && (firstOrder.OrderShippingStatus == ShippingState.ShippingCreated
                    || firstOrder.OrderShippingStatus == ShippingState.ShippingSlotCancelled)
                && orders.All(i => i.CarrierId.HasValue && i.CarrierId == firstOrder.CarrierId);
        }
    }
}