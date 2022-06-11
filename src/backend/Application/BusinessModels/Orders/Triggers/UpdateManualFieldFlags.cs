using Application.BusinessModels.Shared.Triggers;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateManualFieldFlags : ITrigger<Order>
    {
        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var change in changes)
            {
                if (change.IsManuallyChanged(x => x.BodyTypeId))
                    change.Entity.ManualBodyTypeId = true;

                if (change.IsManuallyChanged(x => x.DeliveryCost))
                    change.Entity.ManualDeliveryCost = true;

                if (change.IsManuallyChanged(x => x.DeliveryDate))
                    change.Entity.ManualDeliveryDate = true;

                if (change.IsManuallyChanged(x => x.PickingType))
                    change.Entity.ManualPickingTypeId = true;

                if (change.IsManuallyChanged(x => x.ShippingDate))
                    change.Entity.ManualShippingDate = true;
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.BodyTypeId,
                x => x.DeliveryCost,
                x => x.DeliveryDate,
                x => x.PalletsCount,
                x => x.PickingType,
                x => x.ShippingDate);
        }
    }
}
