using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// Выделить в отдельную перевозку
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(23), DescriptionKey("unionOrdersInOtherShippingDescription")]
    public class UnionOrdersInOtherShipping : IGroupAppAction<Order>
    {
        private readonly IShippingActionService _shippingActionService;
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly IDeliveryCostCalcService _deliveryCostCalcService;
        private readonly IShippingCalculationService _shippingCalculationService;

        public UnionOrdersInOtherShipping(
            ICommonDataService dataService, 
            IShippingActionService shippingActionService, 
            IHistoryService historyService, 
            IDeliveryCostCalcService deliveryCostCalcService, 
            IShippingCalculationService shippingCalculationService)
        {
            _shippingActionService = shippingActionService;
            _dataService = dataService;
            _historyService = historyService;
            _deliveryCostCalcService = deliveryCostCalcService;
            _shippingCalculationService = shippingCalculationService;

            Color = AppColor.Orange;
        }

        public AppColor Color { get; set; }

        public bool IsSingleAllowed => true;

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> orders)
        {
            var shippingToRemoveIds = orders.Select(i => i.ShippingId).Distinct().ToList();
            var shippingToRemoveNumbers = orders.Select(i => i.ShippingNumber).Distinct().ToList();

            var ordersToRemove = orders
                .GroupBy(i => i.ShippingId, i => i)
                .ToDictionary(
                    i => i.Key, 
                    i => i.Select(j => new
                    {
                        j.ShippingId,
                        j.ShippingNumber,
                        j.Id,
                        j.OrderNumber
                    }).ToList()
                );

            orders.ToList().ForEach(i => i.ShippingId = null);

            var shipping = _shippingActionService.UnionOrders(orders, forRegrounpping: true);

            shipping.Status = ShippingState.ShippingConfirmed;
            orders.ToList().ForEach(i => i.OrderShippingStatus = ShippingState.ShippingConfirmed);

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
                        CarrierId = shipping.CarrierId.Value,
                        SentAt = DateTime.Now
                    };
                    requestsDbSet.Add(requestEntry);
                }
                requestEntry.RejectedAt = DateTime.Now;
            }

            shipping.DeliveryType = orders.First().DeliveryType;
            shipping.TarifficationType = orders.First().TarifficationType;

            _historyService.Save(shipping.Id, "shippingConfirmedRegroupping", shipping.ShippingNumber);

            shippingToRemoveIds.ForEach(shippingToRemove =>
            {
                var removedOrders = ordersToRemove[shippingToRemove];
                var orderNumbers = removedOrders.Select(i => i.OrderNumber);
                var removedNumber = removedOrders.First().ShippingNumber;
                _historyService.Save(shippingToRemove.Value, "shippingToRemoveRegroupping", string.Join(", ", orderNumbers), removedNumber, shipping.ShippingNumber);
            });

            ordersToRemove.SelectMany(i => i.Value).ToList().ForEach(orderToRemove => 
            {
                _historyService.Save(orderToRemove.Id, "orderToRemoveRegroupping", orderToRemove.ShippingNumber, shipping.ShippingNumber);
            });

            var shippingsOrders = _dataService.GetDbSet<Order>()
                .Where(i => shippingToRemoveIds.Contains(i.ShippingId))
                .ToList();

            shippingToRemoveIds.ForEach(shippingToRemoveId =>
            {
                var shippingToRemove = _dataService.GetById<Shipping>(shippingToRemoveId.Value);
                var shippingToRemoveOrders = shippingsOrders
                    .Where(i => i.ShippingId == shippingToRemoveId)
                    .Where(i => !ordersToRemove.Keys.Contains(i.Id));

                _deliveryCostCalcService.UpdateDeliveryCost(shippingToRemove, shippingToRemoveOrders);
                _shippingCalculationService.RecalculateDeliveryCosts(shippingToRemove, shippingToRemoveOrders);
            });

            var result = new OperationDetailedResult()
            {
                IsError = false,
                Message = "shippingSetCreated".Translate(user.Language, shipping.ShippingNumber),
            };

            result.Entries.Add(new OperationDetailedResultItem
            { 
                Title = "regrouppingOrdersCount".Translate(user.Language, orders.Count()),
                MessageColumns = 3,
                Messages = new List<string>(orders.Select(i => i.OrderNumber))
            });


            result.Entries.Add(new OperationDetailedResultItem
            {
                Title = "regrouppingShippingCount".Translate(user.Language, shippingToRemoveNumbers.Count()),
                MessageColumns = 3,
                Messages = new List<string>(shippingToRemoveNumbers)
            });

            return result;
        }

        public bool IsAvailable(IEnumerable<Order> orders)
        {
            var tarifficationType = orders.First().TarifficationType;
            var deliveryType = orders.First().DeliveryType;
            var carrierId = orders.First().CarrierId;

            return orders.All(order => order.Status == OrderState.InShipping)
                && orders.All(i => i.TarifficationType == tarifficationType)
                && orders.All(i => i.DeliveryType == deliveryType)
                && orders.All(i => i.OrderShippingStatus == ShippingState.ShippingConfirmed)
                && orders.All(i => i.CarrierId == carrierId);
        }
    }
}
