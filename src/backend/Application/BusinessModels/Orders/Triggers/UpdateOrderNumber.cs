using Application.BusinessModels.Shared.Triggers;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateOrderNumber : ITrigger<Order>
    {
        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var order in changes.Select(x => x.Entity))
            {
                OrderType newOrderType;
                if (order.OrderNumber?.StartsWith("2") == true)
                    newOrderType = OrderType.FD;
                else
                    newOrderType = OrderType.OR;

                order.OrderType = newOrderType;
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.OrderNumber);
        }
    }
}
