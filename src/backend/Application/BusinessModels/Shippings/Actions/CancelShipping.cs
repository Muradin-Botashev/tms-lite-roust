using Application.BusinessModels.Shared.Actions;
using Application.Shared.Notifications;
using Application.Shared.Shippings;
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
    /// Отменить перевозку
    /// </summary>
    public class CancelShipping : BaseShippingAction, IAppAction<Shipping>
    {
        private readonly IHistoryService _historyService;
        private readonly IShippingCalculationService _calculationService;
        private readonly INotificationService _notificationService;

        public AppColor Color { get; set; }

        public CancelShipping(
            ICommonDataService dataService, 
            IHistoryService historyService, 
            IShippingCalculationService calculationService,
            INotificationService notificationService)
            : base(dataService)
        {
            _historyService = historyService;
            _calculationService = calculationService;
            _notificationService = notificationService;
            Color = AppColor.Red;
        }

        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
            shipping.Status = ShippingState.ShippingCanceled;
            shipping.IsNewCarrierRequest = false;

            var orders = _dataService.GetDbSet<Order>().Where(x => x.ShippingId.HasValue && x.ShippingId.Value == shipping.Id).ToList();

            if (shipping.TarifficationType != TarifficationType.Milkrun
                && shipping.TarifficationType != TarifficationType.Pooling
                && (shipping.Status == ShippingState.ShippingRequestSent || shipping.Status == ShippingState.ShippingConfirmed))
            {
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
            }

            shipping.BasicDeliveryCostWithoutVAT = null;

            _calculationService.RecalculateTotalCosts(shipping, orders);

            _historyService.Save(shipping.Id, "shippingSetCancelled", shipping.ShippingNumber);

            foreach (var order in orders)
            {
                var company = GetOrderCompany(order);

                order.Status = company?.OrderRequiresConfirmation == true ? OrderState.Confirmed : OrderState.Created;
                order.ShippingId = null;
                order.ShippingNumber = null;
                order.OrderShippingStatus = null;
                order.IsNewCarrierRequest = false;

                _historyService.Save(order.Id, "orderCancellingShipping", order.OrderNumber, shipping.ShippingNumber);
            }

            _calculationService.ClearShippingOrdersCosts(orders);

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetCancelled".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(Shipping shipping)
        {
            return IsAvailable(shipping.Status);
        }

        public bool IsAvailable(ShippingState? shippingStatus)
        {
            return shippingStatus == ShippingState.ShippingCreated 
                || shippingStatus == ShippingState.ShippingRequestSent 
                || shippingStatus == ShippingState.ShippingConfirmed
                || shippingStatus == ShippingState.ShippingSlotCancelled
                || shippingStatus == ShippingState.ShippingChangesAgreeing;
        }
    }
}