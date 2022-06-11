using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncDeliveryWarehouse : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly IOrderFieldsSyncService _orderFieldsSyncService;

        public SyncDeliveryWarehouse(ICommonDataService dataService, IOrderFieldsSyncService orderFieldsSyncService)
        {
            _dataService = dataService;
            _orderFieldsSyncService = orderFieldsSyncService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var orders = changes.Select(x => x.Entity).Where(x => x.DeliveryWarehouseId.HasValue);
            var deliveryWarehouseIds = orders.Select(x => x.DeliveryWarehouseId).Distinct().ToList();
            var deliveryWarehousesDict = _dataService.GetDbSet<Warehouse>()
                                                     .Where(x => deliveryWarehouseIds.Contains(x.Id))
                                                     .ToDictionary(x => x.Id);

            foreach (var order in orders)
            {
                if (deliveryWarehousesDict.TryGetValue(order.DeliveryWarehouseId.Value, out Warehouse warehouse))
                {
                    _orderFieldsSyncService.SyncWithDeliveryWarehouse(order, warehouse);
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.DeliveryWarehouseId);
        }
    }
}
