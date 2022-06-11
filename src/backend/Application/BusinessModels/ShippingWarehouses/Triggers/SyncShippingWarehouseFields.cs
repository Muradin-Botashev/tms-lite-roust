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
    public class SyncShippingWarehouseFields : ITrigger<ShippingWarehouse>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;

        public SyncShippingWarehouseFields(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<ShippingWarehouse>> changes)
        {
            var warehouseChangesDict = changes.ToDictionary(x => x.Entity.Id);

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
                var change = warehouseChangesDict[order.ShippingWarehouseId.Value];
                var entity = change.Entity;

                if (order.ShippingAddress != entity.Address && change.IsChanged(x => x.Address))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.ShippingAddress).ToLowerFirstLetter(),
                                                     order.ShippingAddress, entity.Address);
                    order.ShippingAddress = entity.Address;
                }

                if (order.ShippingCity != entity.City && change.IsChanged(x => x.City))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.ShippingCity).ToLowerFirstLetter(),
                                                     order.ShippingCity, entity.City);
                    order.ShippingCity = entity.City;
                }

                if (order.ShippingRegion != entity.Region && change.IsChanged(x => x.Region))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.ShippingRegion).ToLowerFirstLetter(),
                                                     order.ShippingRegion, entity.Region);
                    order.ShippingRegion = entity.Region;
                }
            }
        }

        public IEnumerable<EntityChanges<ShippingWarehouse>> FilterTriggered(IEnumerable<EntityChanges<ShippingWarehouse>> changes)
        {
            return changes.FilterChanged(
                x => x.Address,
                x => x.City,
                x => x.Region);
        }
    }
}
