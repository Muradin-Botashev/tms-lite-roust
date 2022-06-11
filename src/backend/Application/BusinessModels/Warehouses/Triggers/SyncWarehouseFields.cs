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

namespace Application.BusinessModels.Warehouses.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncWarehouseFields : ITrigger<Warehouse>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;

        public SyncWarehouseFields(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<Warehouse>> changes)
        {
            var warehouseChangesDict = changes.ToDictionary(x => x.Entity.Id);
            var pickingTypesDict = _dataService.GetDbSet<PickingType>().ToDictionary(x => x.Id);

            var warehouseIds = changes.Select(x => x.Entity).Select(x => x.Id).ToList();
            var validStatuses = new[] { OrderState.Draft, OrderState.Created, OrderState.Confirmed, OrderState.InShipping };
            var orders = _dataService.GetDbSet<Order>()
                                     .Where(x => x.DeliveryWarehouseId != null
                                                && warehouseIds.Contains(x.DeliveryWarehouseId.Value)
                                                && validStatuses.Contains(x.Status)
                                                && (x.ShippingId == null || x.OrderShippingStatus == ShippingState.ShippingCreated))
                                     .ToList();

            foreach (var order in orders)
            {
                var change = warehouseChangesDict[order.DeliveryWarehouseId.Value];
                var entity = change.Entity;

                if (entity.Address != order.DeliveryAddress && change.IsChanged(x => x.Address))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.DeliveryAddress).ToLowerFirstLetter(),
                                                     order.DeliveryAddress, entity.Address);
                    order.DeliveryAddress = entity.Address;
                }

                if (entity.City != order.DeliveryCity && change.IsChanged(x => x.City))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.DeliveryCity).ToLowerFirstLetter(),
                                                     order.DeliveryCity, entity.City);
                    order.DeliveryCity = entity.City;
                }

                if (entity.Client != order.ClientName && change.IsChanged(x => x.Client))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.ClientName).ToLowerFirstLetter(),
                                                     order.ClientName, entity.Client);
                    order.ClientName = entity.Client;
                }

                if (entity.DeliveryType != order.DeliveryType && change.IsChanged(x => x.DeliveryType))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.DeliveryType).ToLowerFirstLetter(),
                                                     order.DeliveryType, entity.DeliveryType);
                    order.DeliveryType = entity.DeliveryType;
                }

                if (entity.LeadtimeDays != order.TransitDays && change.IsChanged(x => x.LeadtimeDays))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.TransitDays).ToLowerFirstLetter(),
                                                     order.TransitDays, entity.LeadtimeDays);
                    order.TransitDays = entity.LeadtimeDays;
                }

                if (entity.PickingFeatures != order.PickingFeatures && change.IsChanged(x => x.PickingFeatures))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.PickingFeatures).ToLowerFirstLetter(),
                                                     order.PickingFeatures, entity.PickingFeatures);
                    order.PickingFeatures = entity.PickingFeatures;
                }

                if (entity.PickingTypeId != order.PickingTypeId && change.IsChanged(x => x.PickingTypeId))
                {
                    PickingType oldValue = null;
                    if (order.PickingTypeId != null)
                        pickingTypesDict.TryGetValue(order.PickingTypeId.Value, out oldValue);

                    PickingType newValue = null;
                    if (entity.PickingTypeId != null)
                        pickingTypesDict.TryGetValue(entity.PickingTypeId.Value, out newValue);

                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.PickingTypeId).ToLowerFirstLetter(),
                                                     oldValue?.Name, newValue?.Name);
                    order.PickingTypeId = entity.PickingTypeId;
                }

                if (entity.Region != order.DeliveryRegion && change.IsChanged(x => x.Region))
                {
                    _historyService.SaveImpersonated(null, order.Id, "fieldChanged",
                                                     nameof(order.DeliveryRegion).ToLowerFirstLetter(),
                                                     order.DeliveryRegion, entity.Region);
                    order.DeliveryRegion = entity.Region;
                }
            }
        }

        public IEnumerable<EntityChanges<Warehouse>> FilterTriggered(IEnumerable<EntityChanges<Warehouse>> changes)
        {
            return changes.FilterChanged(
                x => x.Address,
                x => x.City,
                x => x.Client,
                x => x.DeliveryType,
                x => x.LeadtimeDays,
                x => x.PickingTypeId,
                x => x.PickingFeatures,
                x => x.Region);
        }
    }
}
