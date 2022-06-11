using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Addresses;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateShippingAddress : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly ICleanAddressService _cleanAddressService;

        public UpdateShippingAddress(ICommonDataService dataService, ICleanAddressService cleanAddressService)
        {
            _dataService = dataService;
            _cleanAddressService = cleanAddressService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var allOrders = changes.Select(x => x.Entity).Where(x => !string.IsNullOrEmpty(x.ShippingAddress));
            foreach (var orders in allOrders.GroupBy(x => x.CompanyId))
            {
                var shippingAddresses = changes.Where(x => !string.IsNullOrEmpty(x.Entity.ShippingAddress))
                                               .Select(x => x.Entity.ShippingAddress.Trim())
                                               .Distinct()
                                               .ToList();
                var shippingAddressesList = _dataService.GetDbSet<ShippingWarehouse>()
                                                        .Where(x => (x.CompanyId == orders.Key || x.CompanyId == null)
                                                                    && shippingAddresses.Contains(x.Address.Trim()))
                                                        .ToList();
                var shippingAddressesDict = new Dictionary<string, ShippingWarehouse>();
                foreach (var entry in shippingAddressesList)
                {
                    shippingAddressesDict[entry.Address] = entry;
                }

                var orderWarehousesDict = new Dictionary<Guid, ShippingWarehouse>();
                var orderAddressesDict = new Dictionary<Guid, CleanAddressDto>();

                foreach (var order in orders)
                {
                    var rawAddress = order.ShippingAddress.Trim();
                    if (shippingAddressesDict.TryGetValue(rawAddress, out ShippingWarehouse shippingWarehouse))
                    {
                        orderWarehousesDict[order.Id] = shippingWarehouse;
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
                var shippingValidAddressesDict = _dataService.GetDbSet<ShippingWarehouse>()
                                                             .Where(x => (x.CompanyId == orders.Key || x.CompanyId == null)
                                                                        && validAddresses.Contains(x.ValidAddress))
                                                             .ToDictionary(x => x.ValidAddress);

                foreach (var orderAddress in orderAddressesDict)
                {
                    if (shippingValidAddressesDict.TryGetValue(orderAddress.Value.Address, out ShippingWarehouse shippingWarehouse))
                    {
                        orderWarehousesDict[orderAddress.Key] = shippingWarehouse;
                    }
                }

                foreach (var order in orders)
                {
                    if (orderWarehousesDict.TryGetValue(order.Id, out ShippingWarehouse shippingWarehouse))
                    {
                        order.ShippingAddress = shippingWarehouse.Address;
                        order.ShippingRegion = shippingWarehouse.Region;
                        order.ShippingCity = shippingWarehouse.City;
                        order.ShippingWarehouseId = shippingWarehouse.Id;
                    }
                    else
                    {
                        if (orderAddressesDict.TryGetValue(order.Id, out CleanAddressDto address))
                        {
                            order.ShippingRegion = address?.Region;
                            order.ShippingCity = address?.City ?? address?.Region;
                        }
                        order.ShippingWarehouseId = null;
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.ShippingAddress);
        }
    }
}
