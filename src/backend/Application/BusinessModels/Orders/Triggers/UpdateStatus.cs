using Application.BusinessModels.Shared.Triggers;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateStatus : ITrigger<Order>
    {
        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var order in changes.Select(x => x.Entity))
            {
                order.StatusChangedAt = DateTime.Now;
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.Status);
        }
    }
}
