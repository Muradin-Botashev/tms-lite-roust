using Application.BusinessModels.Shared.Actions;
using Application.Shared.Notifications;
using Application.Shared.Shippings;
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
    /// Убрать из перевозки
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(9)]
    public class RemoveFromShipping : BaseOrderAction, IGroupAppAction<Order>
    {
        private readonly IHistoryService _historyService;
        private readonly IShippingCalculationService _shippingCalculationService;
        private readonly IDeliveryCostCalcService _deliveryCostCalcService;
        private readonly INotificationService _notificationService;

        public RemoveFromShipping(ICommonDataService dataService, 
                                  IHistoryService historyService, 
                                  IShippingCalculationService shippingCalculationService,
                                  IDeliveryCostCalcService deliveryCostCalcService,
                                  INotificationService notificationService)
            : base(dataService)
        {
            _historyService = historyService;
            _shippingCalculationService = shippingCalculationService;
            _deliveryCostCalcService = deliveryCostCalcService;
            _notificationService = notificationService;
            Color = AppColor.Blue;
        }

        public AppColor Color { get; set; }

        public bool IsSingleAllowed => true;

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> orders)
        {
            var messages = new List<string>();

            foreach (var ordersGroup in orders.GroupBy(x => x.ShippingId))
            {
                var shipping = _dataService.GetById<Shipping>(ordersGroup.Key.Value);

                foreach (var order in ordersGroup)
                {
                    var company = GetCompany(order);

                    order.Status = company?.OrderRequiresConfirmation == true ? OrderState.Confirmed : OrderState.Created;
                    order.ShippingStatus = VehicleState.VehicleEmpty;
                    order.DeliveryStatus = VehicleState.VehicleEmpty;
                    order.ShippingId = null;
                    order.ShippingNumber = null;
                    order.OrderShippingStatus = null;

                    _historyService.Save(order.Id, "orderRemovedFromShipping", order.OrderNumber, shipping.ShippingNumber);
                    _historyService.Save(shipping.Id, "orderRemovedFromShipping", order.OrderNumber, shipping.ShippingNumber);
                }

                _shippingCalculationService.ClearShippingOrdersCosts(ordersGroup);

                var removedOrderIds = ordersGroup.Select(x => x.Id).ToList();
                var actualOrders = _dataService.GetDbSet<Order>().Where(x => x.ShippingId == shipping.Id && !removedOrderIds.Contains(x.Id)).ToList();

                if (!actualOrders.Any())
                {
                    if (shipping.TarifficationType != TarifficationType.Milkrun 
                        && shipping.TarifficationType != TarifficationType.Pooling
                        && (shipping.Status == ShippingState.ShippingRequestSent || shipping.Status == ShippingState.ShippingConfirmed))
                    {
                        var data = new CancelNotificationDto
                        {
                            DeliveryPoints = actualOrders.Concat(ordersGroup)
                                                         .Select(x => new DestinationPointDto
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

                    shipping.Status = ShippingState.ShippingCanceled;

                    _historyService.Save(shipping.Id, "shippingSetCancelled", shipping.ShippingNumber);
                }
                else
                {
                    _deliveryCostCalcService.UpdateDeliveryCost(shipping, actualOrders, true);
                    _shippingCalculationService.RecalculateDeliveryCosts(shipping, actualOrders);

                    _shippingCalculationService.RecalculateShipping(shipping, actualOrders);

                    _shippingCalculationService.RecalculateShippingOrdersCosts(shipping, actualOrders);

                    if ((shipping.Status == ShippingState.ShippingRequestSent || shipping.Status == ShippingState.ShippingConfirmed)
                        && shipping.TarifficationType != TarifficationType.Milkrun 
                        && shipping.TarifficationType != TarifficationType.Pooling)
                    {
                        var notificationData = new NotificationOrdersListDto
                        {
                            Orders = ordersGroup.Select(x => new NotificationOrderDto
                            {
                                OrderNumber = x.OrderNumber,
                                DeliveryAddress = x.DeliveryAddress
                            }).ToList()
                        };
                        _notificationService.SendRemoveOrdersFromShippingNotification(shipping.Id, notificationData);
                    }
                }

                var orderNumbers = string.Join(", ", ordersGroup.Select(x => x.OrderNumber));
                messages.Add("orderRemovedFromShipping".Translate(user.Language, orderNumbers, shipping.ShippingNumber));
            }

            return new AppResult
            {
                IsError = false,
                Message = string.Join(' ', messages)
            };
        }

        public bool IsAvailable(IEnumerable<Order> orders)
        {
            return orders.All(x => x.Status == OrderState.InShipping && x.OrderShippingStatus != ShippingState.ShippingSlotBooked);
        }
    }
}