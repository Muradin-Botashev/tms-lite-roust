using Application.BusinessModels.Shared.Actions;
using Application.Shared.Notifications;
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
    /// <summary>
    /// Отменить заявку
    /// </summary>
    public class CancelRequestShipping : IAppAction<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly INotificationService _notificationService;
        private readonly IHistoryService _historyService;

        public AppColor Color { get; set; }

        public CancelRequestShipping(ICommonDataService dataService, INotificationService notificationService, IHistoryService historyService)
        {
            _dataService = dataService;
            _notificationService = notificationService;
            _historyService = historyService;
            Color = AppColor.Red;
        }
        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
            shipping.Status = ShippingState.ShippingCreated;
            shipping.IsNewCarrierRequest = false;

            var orders = _dataService.GetDbSet<Order>().Where(o => o.ShippingId == shipping.Id).ToList();
            foreach (var order in orders)
            {
                order.OrderShippingStatus = shipping.Status;
                order.IsNewCarrierRequest = false;
            }

            _historyService.Save(shipping.Id, "shippingSetCancelledRequest", shipping.ShippingNumber);

            var data = new CancelNotificationDto
            {
                DeliveryPoints = orders.Select(x => new DestinationPointDto
                                        {
                                            DeliveryAddress = x.DeliveryAddress,
                                            DeliveryCity = x.DeliveryCity,
                                            DeliveryDate = x.DeliveryDate,
                                            DeliveryRegion = x.DeliveryRegion,
                                            DeliveryWarehouseName = x.DeliveryWarehouse?.WarehouseName
                                        })
                                       .ToList()
            };

            _notificationService.SendCancelShippingNotification(shipping.Id, data);

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetCancelledRequest".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(Shipping shipping)
        {
            return IsAvailable(shipping.Status);
        }

        public bool IsAvailable(ShippingState? shippingStatus)
        {
            return shippingStatus == ShippingState.ShippingRequestSent;
        }
    }
}