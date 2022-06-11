using Application.Shared.Notifications;
using Application.Shared.Orders;
using Application.Shared.Pooling.Models;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Pooling.Models;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Shippings
{
    public class SendShippingService : ISendShippingService
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly IOrderPoolingService _poolingService;
        private readonly INotificationService _notificationService;

        public SendShippingService(
            ICommonDataService dataService,
            IHistoryService historyService,
            IOrderPoolingService poolingService,
            INotificationService notificationService)
        {
            _dataService = dataService;
            _historyService = historyService;
            _poolingService = poolingService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Отправить заявку в ТК перевозки
        /// </summary>
        /// <param name="shipping"></param>
        /// <returns></returns>
        public Shipping SendShippingToTk(Shipping shipping, List<Order> orders = null)
        {
            shipping.Status = ShippingState.ShippingRequestSent;
            shipping.IsNewCarrierRequest = true;

            if (shipping.TarifficationType != TarifficationType.Milkrun && shipping.TarifficationType != TarifficationType.Pooling)
            {
                _notificationService.SendRequestToCarrierNotification(shipping.Id);
            }

            if (shipping.CarrierId != null)
            {
                var requestsDbSet = _dataService.GetDbSet<CarrierRequestDatesStat>();
                var requestEntry = requestsDbSet.FirstOrDefault(x => x.ShippingId == shipping.Id && x.CarrierId == shipping.CarrierId);
                if (requestEntry == null)
                {
                    requestEntry = new CarrierRequestDatesStat
                    {
                        Id = Guid.NewGuid(),
                        ShippingId = shipping.Id,
                        CarrierId = shipping.CarrierId.Value
                    };
                    requestsDbSet.Add(requestEntry);
                }
                requestEntry.SentAt = DateTime.Now;
                requestEntry.RejectedAt = null;
                requestEntry.ConfirmedAt = null;
            }

            if (orders == null)
            {
                orders = _dataService.GetDbSet<Order>().Where(o => o.ShippingId == shipping.Id).ToList();
            }

            foreach (var order in orders)
            {
                order.OrderShippingStatus = shipping.Status;
                order.IsNewCarrierRequest = true;
            }

            _historyService.Save(shipping.Id, "shippingSetRequestSent", shipping.ShippingNumber);

            return shipping;
        }


        /// <summary>
        /// Отправить заявку в ТК перевозки
        /// </summary>
        /// <param name="shipping"></param>
        /// <returns></returns>
        public AppResult SendShippingToPooling(CurrentUserDto user, Shipping shipping, List<Order> orders = null)
        {
            if (orders == null)
            {
                orders = _dataService.GetDbSet<Order>().Where(i => i.ShippingId == shipping.Id).ToList();
            }

            var validationResult = _poolingService.ValidateOrders(orders, user);

            if (validationResult != null)
            {
                return validationResult;
            }

            var tarifficationType = orders.FirstOrDefault()?.TarifficationType;

            HttpResult<SlotDto> slot = null;

            if (tarifficationType == TarifficationType.Pooling || tarifficationType == TarifficationType.Milkrun)
            {
                slot = _poolingService.GetSlot(shipping, orders.First());

                var slotResult = _poolingService.ValidateGetSlot(slot, user);

                if (slotResult != null)
                {
                    return slotResult;
                }
            }

            var bookedSlot = _poolingService.BookSlot(shipping, orders, slot?.Result);

            var bookedSlotResult = _poolingService.ValidateBookedSlot(bookedSlot, user);

            if (bookedSlotResult != null)
            {
                return bookedSlotResult;
            }

            shipping.PoolingReservationId = bookedSlot.Result.Id;
            shipping.SlotId = bookedSlot.Result.SlotId;
            shipping.ConsolidationDate = slot?.Result?.ConsolidationDate?.ToDateTime();
            shipping.AvailableUntil = bookedSlot.Result.EditableUntil.ToDateTime();

            shipping.Status = ShippingState.ShippingSlotBooked;

            foreach (var shippingOrder in orders)
            {
                shippingOrder.OrderShippingStatus = shipping.Status;
            }

            orders.ToList().ForEach(i => i.BookingNumber = bookedSlot.Result.Number);

            shipping.IsPooling = bookedSlot.Result.ShippingType == "Pooling";

            orders.ToList().ForEach(x => x.IsPooling = shipping.IsPooling);

            _historyService.Save(shipping.Id, "orderSentToPooling", shipping.ShippingNumber);

            orders.ToList().ForEach(i => _historyService.Save(i.Id, "orderSentToPooling", shipping.ShippingNumber));

            return new AppResult
            {
                IsError = false,
                Message = "orderSentToPoolingAlert".Translate(user.Language, shipping.ShippingNumber),
                ManuallyClosableMessage = true
            };
        }
    }
}
