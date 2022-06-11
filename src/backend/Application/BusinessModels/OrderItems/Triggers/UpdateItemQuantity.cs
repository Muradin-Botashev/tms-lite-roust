using Application.BusinessModels.Shared.Triggers;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System.Collections.Generic;

namespace Application.BusinessModels.OrderItems.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateItemQuantity : ITrigger<OrderItem>
    {
        private readonly IHistoryService _historyService;

        public UpdateItemQuantity(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<OrderItem>> changes)
        {
            foreach (var change in changes)
            {
                if (change.Status != EntityStatus.Added)
                {
                    var entity = change.Entity;
                    _historyService.Save(entity.OrderId, "orderItemChangeQuantity", entity.Nart, entity.Quantity);
                }
            }
        }

        public IEnumerable<EntityChanges<OrderItem>> FilterTriggered(IEnumerable<EntityChanges<OrderItem>> changes)
        {
            return changes.FilterChanged(x => x.Quantity);
        }
    }
}
