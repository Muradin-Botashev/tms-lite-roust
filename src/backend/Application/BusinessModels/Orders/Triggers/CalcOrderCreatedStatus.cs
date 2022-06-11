using Application.BusinessModels.Shared.Triggers;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Calculation)]
    public class CalcOrderCreatedStatus : ITrigger<Order>
    {
        private readonly IHistoryService _historyService;

        public CalcOrderCreatedStatus(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var order in changes.Select(x => x.Entity))
            {
                bool hasRequiredFields =
                    !string.IsNullOrEmpty(order.ShippingAddress)
                    && !string.IsNullOrEmpty(order.DeliveryCity)
                    && !string.IsNullOrEmpty(order.DeliveryRegion)
                    && !string.IsNullOrEmpty(order.DeliveryAddress)
                    && order.PalletsCount.HasValue
                    && order.ShippingDate.HasValue
                    && (order.DeliveryDate.HasValue || order.DeliveryType != DeliveryType.Delivery);

                if (order.Status == OrderState.Draft && hasRequiredFields)
                {
                    order.Status = OrderState.Created;
                    _historyService.Save(order.Id, "orderSetCreated", order.OrderNumber);
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.ShippingAddress,
                x => x.DeliveryCity,
                x => x.DeliveryRegion,
                x => x.DeliveryAddress,
                x => x.PalletsCount,
                x => x.ShippingDate,
                x => x.DeliveryDate);
        }
    }
}
