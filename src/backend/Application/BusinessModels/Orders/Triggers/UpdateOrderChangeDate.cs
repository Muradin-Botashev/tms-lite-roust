using Application.BusinessModels.Shared.Triggers;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateOrderChangeDate : ITrigger<Order>
    {
        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var entity in changes.Select(x => x.Entity))
            {
                entity.OrderChangeDate = DateTime.UtcNow;
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.ArticlesCount,
                x => x.BoxesCount,
                x => x.DeliveryDate,
                x => x.ClientOrderNumber,
                x => x.ConfirmedBoxesCount,
                x => x.ConfirmedPalletsCount,
                x => x.OrderAmountExcludingVAT,
                x => x.PalletsCount,
                x => x.PickingType,
                x => x.ShippingDate,
                x => x.WeightKg);
        }
    }
}
