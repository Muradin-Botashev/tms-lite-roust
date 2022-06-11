using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Calculation)]
    public class UpdateOrderDeliveryCost : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly IDeliveryCostCalcService _calcService;
        private readonly IShippingCalculationService _shippingCalculationService;

        public UpdateOrderDeliveryCost(
            ICommonDataService dataService, 
            IHistoryService historyService, 
            IDeliveryCostCalcService calcService,
            IShippingCalculationService shippingCalculationService)
        {
            _dataService = dataService;
            _historyService = historyService;
            _calcService = calcService;
            _shippingCalculationService = shippingCalculationService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var orderIds = changes.Select(x => x.Entity.Id).ToList();
            var shippingIds = changes.Select(x => x.Entity.ShippingId).Where(x => x != null).ToList();

            var shippingsDict = _dataService.GetDbSet<Shipping>()
                                         .Where(x => shippingIds.Contains(x.Id))
                                         .ToDictionary(x => x.Id);

            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null
                                                && shippingIds.Contains(x.ShippingId.Value)
                                                && !orderIds.Contains(x.Id))
                                         .GroupBy(x => x.ShippingId)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var entity in changes.Select(x => x.Entity).Where(x => x.ShippingId != null))
            {
                List<Order> shippingOrders;
                if (!ordersDict.TryGetValue(entity.ShippingId, out shippingOrders))
                {
                    shippingOrders = new List<Order>();
                    ordersDict[entity.ShippingId] = shippingOrders;
                }
                shippingOrders.Add(entity);
            }

            foreach (var entity in changes.Select(x => x.Entity))
            {
                if (entity.ShippingId != null && shippingsDict.TryGetValue(entity.ShippingId.Value, out Shipping shipping))
                {
                    List<Order> orders = null;
                    ordersDict.TryGetValue(shipping.Id, out orders);

                    _calcService.UpdateDeliveryCost(shipping, orders);
                    _shippingCalculationService.RecalculateDeliveryCosts(shipping, orders);
                }
                else if (entity.DeliveryCost != null)
                {
                    _historyService.Save(entity.Id, "fieldChanged",
                                         nameof(entity.DeliveryCost).ToLowerFirstLetter(),
                                         entity.DeliveryCost, null);
                    entity.DeliveryCost = null;
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.ShippingId,
                x => x.DeliveryType,
                x => x.PalletsCount,
                x => x.ClientName,
                x => x.ShippingDate,
                x => x.DeliveryDate,
                x => x.ShippingCity,
                x => x.DeliveryCity,
                x => x.ShippingRegion,
                x => x.DeliveryRegion);
        }
    }
}
