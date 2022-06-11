using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateShippingWarehouse : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;

        public UpdateShippingWarehouse(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var shippingWarehouseIds = changes.Where(x => x.Entity.ShippingWarehouseId.HasValue)
                                              .Select(x => x.Entity.ShippingWarehouseId)
                                              .Distinct()
                                              .ToList();
            var shippingWarehousesDict = _dataService.GetDbSet<ShippingWarehouse>()
                                                     .Where(x => shippingWarehouseIds.Contains(x.Id))
                                                     .ToDictionary(x => x.Id);

            foreach (var order in changes.Select(x => x.Entity).Where(x => x.ShippingWarehouseId.HasValue))
            {
                if (shippingWarehousesDict.TryGetValue(order.ShippingWarehouseId.Value, out ShippingWarehouse shippingWarehouse))
                {
                    order.ShippingAddress = shippingWarehouse.Address;
                    order.ShippingCity = shippingWarehouse.City;
                    order.ShippingRegion = shippingWarehouse.Region;
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.ShippingWarehouseId);
        }
    }
}
