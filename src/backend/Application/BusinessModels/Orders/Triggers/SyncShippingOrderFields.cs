using Application.BusinessModels.Shared.Triggers;
using DAL.Queries;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncShippingOrderFields : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        

        public SyncShippingOrderFields(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
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

            foreach (var change in changes.Where(x => x.Entity.ShippingId != null))
            {
                var entity = change.Entity;

                Shipping shipping = null;
                shippingsDict.TryGetValue(entity.ShippingId.Value, out shipping);

                List<Order> orders = null;
                ordersDict.TryGetValue(shipping.Id, out orders);

                var vehicleTypes = _dataService.GetDbSet<VehicleType>();

                foreach (var order in orders)
                {
                    if (order.TarifficationType != entity.TarifficationType && change.IsChanged(x => x.TarifficationType))
                    {
                        _historyService.Save(order.Id, "fieldChangedBy",
                            nameof(entity.TarifficationType).ToLowerFirstLetter(),
                            order.TarifficationType, entity.TarifficationType, "onChangeInOtherOrderInShipping");

                        order.TarifficationType = entity.TarifficationType;
                    }

                    if (order.VehicleTypeId != entity.VehicleTypeId && change.IsChanged(x => x.VehicleTypeId))
                    {
                        VehicleType oldVehicleType = null;
                        VehicleType newVehicleType = null;

                        if (order.VehicleTypeId.HasValue)
                            oldVehicleType = vehicleTypes.GetById(order.VehicleTypeId.Value);

                        if (entity.VehicleTypeId.HasValue)
                            newVehicleType = vehicleTypes.GetById(entity.VehicleTypeId.Value);

                        order.VehicleTypeId = entity.VehicleTypeId;

                        _historyService.Save(order.Id, "fieldChangedBy",
                            nameof(order.VehicleTypeId).ToLowerFirstLetter(),
                            oldVehicleType, newVehicleType, "onChangeInOtherOrderInShipping");
                    }

                    if (change.IsChanged(x => x.BodyTypeId))
                        order.BodyTypeId = entity.BodyTypeId;

                    if (change.IsChanged(x => x.CarrierId))
                        order.CarrierId = entity.CarrierId;

                    if (change.IsChanged(x => x.DriverName))
                        order.DriverName = entity.DriverName;

                    if (change.IsChanged(x => x.DriverPassportData))
                        order.DriverPassportData = entity.DriverPassportData;

                    if (change.IsChanged(x => x.DriverPhone))
                        order.DriverPhone = entity.DriverPhone;

                    if (change.IsChanged(x => x.VehicleMake))
                        order.VehicleMake = entity.VehicleMake;

                    if (change.IsChanged(x => x.VehicleNumber))
                        order.VehicleNumber = entity.VehicleNumber;

                    if (change.IsChanged(x => x.TrailerNumber))
                        order.TrailerNumber = entity.TrailerNumber;

                    if (change.IsChanged(x => x.IsPooling))
                        order.IsPooling = entity.IsPooling;
                }

                if (change.IsChanged(x => x.BodyTypeId))
                    shipping.BodyTypeId = entity.BodyTypeId;

                if (change.IsChanged(x => x.CarrierId))
                    shipping.CarrierId = entity.CarrierId;

                if (change.IsChanged(x => x.DriverName))
                    shipping.DriverName = entity.DriverName;

                if (change.IsChanged(x => x.DriverPassportData))
                    shipping.DriverPassportData = entity.DriverPassportData;

                if (change.IsChanged(x => x.DriverPhone))
                    shipping.DriverPhone = entity.DriverPhone;

                if (change.IsChanged(x => x.VehicleMake))
                    shipping.VehicleMake = entity.VehicleMake;

                if (change.IsChanged(x => x.VehicleNumber))
                    shipping.VehicleNumber = entity.VehicleNumber;

                if (change.IsChanged(x => x.TrailerNumber))
                    shipping.TrailerNumber = entity.TrailerNumber;

                if (change.IsChanged(x => x.IsPooling))
                    shipping.IsPooling = entity.IsPooling;

                if (shipping.TarifficationType != entity.TarifficationType && change.IsChanged(x => x.TarifficationType))
                {
                    _historyService.Save(shipping.Id, "fieldChangedBy",
                        nameof(shipping.TarifficationType).ToLowerFirstLetter(),
                        shipping.TarifficationType, entity.TarifficationType, "onChangeInIncludedOrder");

                    shipping.TarifficationType = entity.TarifficationType;
                    shipping.ManualTarifficationType = true;
                }

                if (shipping.VehicleTypeId != entity.VehicleTypeId && change.IsChanged(x => x.VehicleTypeId))
                {
                    VehicleType oldVehicleType = null;
                    VehicleType newVehicleType = null;

                    if (shipping.VehicleTypeId.HasValue)
                        oldVehicleType = vehicleTypes.GetById(shipping.VehicleTypeId.Value);

                    if (entity.VehicleTypeId.HasValue)
                        newVehicleType = vehicleTypes.GetById(entity.VehicleTypeId.Value);

                    _historyService.Save(shipping.Id, "fieldChangedBy",
                        nameof(shipping.VehicleTypeId).ToLowerFirstLetter(),
                        oldVehicleType, newVehicleType, "onChangeInIncludedOrder");

                    shipping.VehicleTypeId = entity.VehicleTypeId;
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.BodyTypeId,
                x => x.CarrierId,
                x => x.DriverName,
                x => x.DriverPassportData,
                x => x.DriverPhone,
                x => x.IsPooling,
                x => x.TarifficationType,
                x => x.TrailerNumber,
                x => x.VehicleMake,
                x => x.VehicleNumber,
                x => x.VehicleTypeId);
        }
    }
}
