using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.ShippingWarehouses.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncShippingWarehouseName : ITrigger<ShippingWarehouse>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;

        public SyncShippingWarehouseName(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<ShippingWarehouse>> changes)
        {
            var warehousesDict = changes.Select(x => x.Entity).ToDictionary(x => x.Id);

            var warehouseIds = changes.Select(x => x.Entity.Id).ToList();
            var validStatuses = new[] { OrderState.Draft, OrderState.Created, OrderState.Confirmed, OrderState.InShipping };
            var orders = _dataService.GetDbSet<Order>()
                                     .Where(x => x.ShippingWarehouseId != null
                                                && warehouseIds.Contains(x.ShippingWarehouseId.Value)
                                                && validStatuses.Contains(x.Status)
                                                && (x.ShippingId == null || x.OrderShippingStatus == ShippingState.ShippingCreated))
                                     .ToList();

            foreach (var order in orders)
            {
                var entity = warehousesDict[order.ShippingWarehouseId.Value];
                _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                 nameof(order.ShippingWarehouseId).ToLowerFirstLetter(),
                                                 entity.WarehouseName, entity.WarehouseName);
            }
        }

        public IEnumerable<EntityChanges<ShippingWarehouse>> FilterTriggered(IEnumerable<EntityChanges<ShippingWarehouse>> changes)
        {
            return changes.FilterChanged(x => x.WarehouseName);
        }
    }
}
