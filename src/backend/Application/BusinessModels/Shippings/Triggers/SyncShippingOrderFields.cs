using Application.BusinessModels.Shared.Triggers;
using DAL.Queries;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncShippingOrderFields : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;  

        public SyncShippingOrderFields(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;      
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var vehicleTypes = _dataService.GetDbSet<VehicleType>();
            var shippingIds = changes.Select(x => x.Entity.Id).ToList();
            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                         .GroupBy(x => x.ShippingId)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var change in changes)
            {
                var entity = change.Entity;

                List<Order> orders = null;
                if (!ordersDict.TryGetValue(entity.Id, out orders))
                {
                    continue;
                }

                foreach (var orderInShipping in orders)
                {
                    if (change.IsChanged(x => x.BodyTypeId))
                        orderInShipping.BodyTypeId = entity.BodyTypeId;

                    if (change.IsChanged(x => x.CarrierId))
                        orderInShipping.CarrierId = entity.CarrierId;

                    if (change.IsChanged(x => x.DriverName))
                        orderInShipping.DriverName = entity.DriverName;

                    if (change.IsChanged(x => x.DriverPassportData))
                        orderInShipping.DriverPassportData = entity.DriverPassportData;

                    if (change.IsChanged(x => x.DriverPhone))
                        orderInShipping.DriverPhone = entity.DriverPhone;

                    if (change.IsChanged(x => x.LoadingArrivalTime))
                        orderInShipping.LoadingArrivalTime = entity.LoadingArrivalTime;

                    if (change.IsChanged(x => x.LoadingDepartureTime))
                        orderInShipping.LoadingDepartureTime = entity.LoadingDepartureTime;

                    if (change.IsChanged(x => x.TrailerNumber))
                        orderInShipping.TrailerNumber = entity.TrailerNumber;

                    if (change.IsChanged(x => x.VehicleMake))
                        orderInShipping.VehicleMake = entity.VehicleMake;

                    if (change.IsChanged(x => x.VehicleNumber))
                        orderInShipping.VehicleNumber = entity.VehicleNumber;

                    if (change.IsChanged(x => x.Status))
                        orderInShipping.OrderShippingStatus = entity.Status;

                    if (change.IsChanged(x => x.IsPooling))
                        orderInShipping.IsPooling = entity.IsPooling;

                    if (orderInShipping.TarifficationType != entity.TarifficationType && change.IsChanged(x => x.TarifficationType))
                    {
                        _historyService.Save(orderInShipping.Id, "fieldChangedBy",
                            nameof(entity.TarifficationType).ToLowerFirstLetter(),
                            orderInShipping.TarifficationType, entity.TarifficationType, "onChangeInShipping");

                        orderInShipping.TarifficationType = entity.TarifficationType;
                    }

                    if (orderInShipping.VehicleTypeId != entity.VehicleTypeId && change.IsChanged(x => x.VehicleTypeId))
                    {
                        VehicleType oldVehicleType = null;
                        VehicleType newVehicleType = null;

                        if (orderInShipping.VehicleTypeId.HasValue)
                            oldVehicleType = vehicleTypes.GetById(orderInShipping.VehicleTypeId.Value);

                        if (entity.VehicleTypeId.HasValue)
                            newVehicleType = vehicleTypes.GetById(entity.VehicleTypeId.Value);

                        orderInShipping.VehicleTypeId = entity.VehicleTypeId;

                        _historyService.Save(orderInShipping.Id, "fieldChangedBy",
                            nameof(orderInShipping.VehicleTypeId).ToLowerFirstLetter(),
                            oldVehicleType, newVehicleType, "onChangeInShipping");
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(
                x => x.BodyTypeId,
                x => x.CarrierId,
                x => x.DriverName,
                x => x.DriverPassportData,
                x => x.DriverPhone,
                x => x.IsPooling,
                x => x.LoadingArrivalTime,
                x => x.LoadingDepartureTime,
                x => x.Status,
                x => x.TarifficationType,
                x => x.TrailerNumber,
                x => x.VehicleMake,
                x => x.VehicleNumber,
                x => x.VehicleTypeId);
        }
    }
}
