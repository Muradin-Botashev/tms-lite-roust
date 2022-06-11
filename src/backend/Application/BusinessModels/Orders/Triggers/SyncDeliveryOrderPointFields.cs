using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncDeliveryOrderPointFields : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        

        public SyncDeliveryOrderPointFields(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var changesGroup in changes.Where(x => x.Entity.ShippingId != null)
                                                .GroupBy(x => x.Entity.DeliveryWarehouseId))
            {
                var orderIds = changesGroup.Select(x => x.Entity.Id).ToList();
                var shippingIds = changesGroup.Select(x => x.Entity.ShippingId).ToList();

                var ordersDict = _dataService.GetDbSet<Order>()
                                             .Where(x => x.ShippingId != null
                                                    && shippingIds.Contains(x.ShippingId.Value)
                                                    && !orderIds.Contains(x.Id)
                                                    && x.DeliveryWarehouseId == changesGroup.Key)
                                             .GroupBy(x => x.ShippingId)
                                             .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var entity in changesGroup.Select(x => x.Entity))
                {
                    List<Order> shippingOrders;
                    if (!ordersDict.TryGetValue(entity.ShippingId, out shippingOrders))
                    {
                        shippingOrders = new List<Order>();
                        ordersDict[entity.ShippingId] = shippingOrders;
                    }
                    shippingOrders.Add(entity);
                }

                foreach (var change in changesGroup)
                {
                    var entity = change.Entity;

                    List<Order> orders = null;
                    ordersDict.TryGetValue(entity.ShippingId.Value, out orders);

                    foreach (var order in orders)
                    {
                        if (change.IsChanged(x => x.DeliveryDate))
                            order.DeliveryDate = entity.DeliveryDate;

                        if (change.IsChanged(x => x.DeliveryStatus))
                            order.DeliveryStatus = entity.DeliveryStatus;

                        if (change.IsChanged(x => x.TrucksDowntime))
                            order.TrucksDowntime = entity.TrucksDowntime;
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.DeliveryDate,
                x => x.DeliveryStatus,
                x => x.TrucksDowntime);
        }
    }
}
