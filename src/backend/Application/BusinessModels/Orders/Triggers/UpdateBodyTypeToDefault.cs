using Application.BusinessModels.Shared.Triggers;
using Application.Shared.BodyTypes;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.SyncFields)]
    public class UpdateBodyTypeToDefault : ITrigger<Order>
    {
        private readonly IDefaultBodyTypeService _bodyTypeService;

        public UpdateBodyTypeToDefault(IDefaultBodyTypeService bodyTypeService)
        {
            _bodyTypeService = bodyTypeService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var group in changes.Select(x => x.Entity)
                                         .GroupBy(x => new
                                         {
                                             x.ShippingDate,
                                             x.ShippingWarehouseId,
                                             x.DeliveryWarehouseId,
                                             x.ShippingCity,
                                             x.DeliveryCity,
                                             x.ShippingRegion,
                                             x.DeliveryRegion
                                         }))
            {
                var bodyTypeId = _bodyTypeService.GetDefaultBodyType(group.FirstOrDefault())?.Id;
                foreach (var entity in group)
                {
                    if (!entity.ManualBodyTypeId 
                        && (entity.Status == OrderState.Draft || entity.Status == OrderState.Created || entity.Status == OrderState.Confirmed))
                    {
                        entity.BodyTypeId = bodyTypeId;
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.ShippingDate,
                x => x.ShippingWarehouseId,
                x => x.DeliveryWarehouseId,
                x => x.ShippingCity,
                x => x.DeliveryCity,
                x => x.ShippingRegion,
                x => x.DeliveryRegion);
        }
    }
}
