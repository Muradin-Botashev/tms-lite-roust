using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Notifications;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.PostUpdates)]
    public class SendUpdateShippingNotification : ITrigger<Order>
    {
        private readonly INotificationService _notificationService;
        private readonly string[] _watchProperties = new[]
        {
            nameof(Order.ClientName),
            nameof(Order.ClientOrderNumber),
            nameof(Order.DeliveryAddress),
            nameof(Order.DeliveryCity),
            nameof(Order.DeliveryDate),
            nameof(Order.DeliveryRegion),
            nameof(Order.DeliveryWarehouseId),
            nameof(Order.OrderAmountExcludingVAT),
            nameof(Order.PalletsCount),
            nameof(Order.ShippingAddress),
            nameof(Order.ShippingCity),
            nameof(Order.ShippingDate),
            nameof(Order.ShippingRegion),
            nameof(Order.ShippingWarehouseId),
            nameof(Order.WeightKg),
        };

        public SendUpdateShippingNotification(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            foreach (var change in changes)
            {
                var entity = change.Entity;
                if (entity.ShippingId != null
                    && (entity.OrderShippingStatus == ShippingState.ShippingRequestSent
                        || entity.OrderShippingStatus == ShippingState.ShippingConfirmed)
                    && entity.TarifficationType != TarifficationType.Milkrun
                    && entity.TarifficationType != TarifficationType.Pooling)
                {
                    var notificationData = new NotificationShippingChangesDto
                    {
                        Orders = new List<NotificationOrderChangesDto>
                        {
                            new NotificationOrderChangesDto
                            {
                                OrderNumber = entity.OrderNumber,
                                ChangesFields = change.FieldChanges.Select(x => x.FieldName)
                                                                   .Where(x => _watchProperties.Any(y => y.ToLower() == x.ToLower()))
                                                                   .ToList()
                            }
                        }
                    };
                    _notificationService.SendUpdateShippingRequestDataNotification(entity.ShippingId.Value, notificationData);
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(_watchProperties);
        }
    }
}
