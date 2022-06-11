using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Addresses;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateDeliveryAddress : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly ICleanAddressService _cleanAddressService;
        private readonly IOrderFieldsSyncService _orderFieldsSyncService;

        public UpdateDeliveryAddress(
            ICommonDataService dataService, 
            ICleanAddressService cleanAddressService,
            IOrderFieldsSyncService orderFieldsSyncService)
        {
            _dataService = dataService;
            _cleanAddressService = cleanAddressService;
            _orderFieldsSyncService = orderFieldsSyncService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var allOrders = changes.Select(x => x.Entity).Where(x => !string.IsNullOrEmpty(x.DeliveryAddress));
            foreach (var orders in allOrders.GroupBy(x => x.CompanyId))
            {
                var deliveryAddresses = changes.Where(x => !string.IsNullOrEmpty(x.Entity.DeliveryAddress))
                                               .Select(x => x.Entity.DeliveryAddress.Trim())
                                               .Distinct()
                                               .ToList();
                var deliveryAddressesList = _dataService.GetDbSet<Warehouse>()
                                                        .Where(x => (x.CompanyId == orders.Key || x.CompanyId == null)
                                                                    && deliveryAddresses.Contains(x.Address.Trim()))
                                                        .ToList();
                var deliveryAddressesDict = new Dictionary<string, Warehouse>();
                foreach (var entry in deliveryAddressesList)
                {
                    deliveryAddressesDict[entry.Address] = entry;
                }

                var orderWarehousesDict = new Dictionary<Guid, Warehouse>();
                var orderAddressesDict = new Dictionary<Guid, CleanAddressDto>();

                foreach (var order in orders)
                {
                    var rawAddress = order.DeliveryAddress.Trim();
                    if (deliveryAddressesDict.TryGetValue(rawAddress, out Warehouse warehouse))
                    {
                        orderWarehousesDict[order.Id] = warehouse;
                    }
                    else
                    {
                        var address = _cleanAddressService.CleanAddress(rawAddress);
                        if (address != null)
                        {
                            orderAddressesDict[order.Id] = address;
                        }
                    }
                }

                var validAddresses = orderAddressesDict.Values.Select(x => x.Address)
                                                              .Distinct()
                                                              .ToList();
                var deliveryValidAddressesDict = _dataService.GetDbSet<Warehouse>()
                                                             .Where(x => (x.CompanyId == orders.Key || x.CompanyId == null)
                                                                        && validAddresses.Contains(x.ValidAddress))
                                                             .ToDictionary(x => x.ValidAddress);

                foreach (var orderAddress in orderAddressesDict)
                {
                    if (deliveryValidAddressesDict.TryGetValue(orderAddress.Value.Address, out Warehouse warehouse))
                    {
                        orderWarehousesDict[orderAddress.Key] = warehouse;
                    }
                }

                foreach (var order in orders)
                {
                    if (orderWarehousesDict.TryGetValue(order.Id, out Warehouse warehouse))
                    {
                        _orderFieldsSyncService.SyncWithDeliveryWarehouse(order, warehouse);
                    }
                    else
                    {
                        if (orderAddressesDict.TryGetValue(order.Id, out CleanAddressDto address))
                        {
                            order.DeliveryCity = address?.City ?? address?.Region;
                            order.DeliveryRegion = address?.Region;
                        }
                        order.DeliveryType = order.DeliveryType ?? DeliveryType.Delivery;
                        order.DeliveryWarehouseId = null;
                        order.ClientName = null;
                        order.SoldTo = null;
                        order.TransitDays = null;
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.DeliveryAddress);
        }
    }
}
