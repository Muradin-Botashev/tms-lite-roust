using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Calculation)]
    public class CalcShippingTemperature : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly IShippingCalculationService _calculationService;

        public CalcShippingTemperature(ICommonDataService dataService, IShippingCalculationService calculationService)
        {
            _dataService = dataService;
            _calculationService = calculationService;
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

            foreach (var entity in changes.Select(x => x.Entity).Where(x => x.ShippingId != null))
            {
                Shipping shipping = null;
                shippingsDict.TryGetValue(entity.ShippingId.Value, out shipping);

                List<Order> orders = null;
                ordersDict.TryGetValue(shipping.Id, out orders);

                _calculationService.RecalculateTemperature(shipping, orders);
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.TemperatureMin,
                x => x.TemperatureMax);
        }
    }
}
