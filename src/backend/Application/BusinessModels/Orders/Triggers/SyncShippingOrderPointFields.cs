using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncShippingOrderPointFields : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        

        public SyncShippingOrderPointFields(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var changesGroup in changes.Where(x => x.Entity.ShippingId != null)
                                                .GroupBy(x => x.Entity.ShippingWarehouseId))
            {
                var orderIds = changesGroup.Select(x => x.Entity.Id).ToList();
                var shippingIds = changesGroup.Select(x => x.Entity.ShippingId).ToList();

                var ordersDict = _dataService.GetDbSet<Order>()
                                             .Where(x => x.ShippingId != null
                                                    && shippingIds.Contains(x.ShippingId.Value)
                                                    && !orderIds.Contains(x.Id)
                                                    && x.ShippingWarehouseId == changesGroup.Key)
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
                        if (change.IsChanged(x => x.ShippingDate))
                            order.ShippingDate = entity.ShippingDate;

                        if (change.IsChanged(x => x.ShippingStatus))
                        {
                            order.ShippingStatus = entity.ShippingStatus;

                            if (order.ShippingStatus == VehicleState.VehicleArrived)
                            {
                                order.LoadingArrivalTime = DateTime.Now;
                            }
                            else if (order.ShippingStatus == VehicleState.VehicleDepartured)
                            {
                                order.LoadingArrivalTime = order.LoadingArrivalTime ?? DateTime.Now;
                                order.LoadingDepartureTime = DateTime.Now;
                                order.DeliveryStatus = VehicleState.VehicleWaiting;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.ShippingDate,
                x => x.ShippingStatus);
        }
    }
}
