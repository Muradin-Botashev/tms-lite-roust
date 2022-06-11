using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Notifications;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.PostUpdates)]
    public class SendUpdateShippingNotification : ITrigger<Shipping>
    {
        private readonly INotificationService _notificationService;
        private readonly string[] _watchProperties = new[]
        {
            nameof(Shipping.BodyTypeId),
            nameof(Shipping.PalletsCount),
            nameof(Shipping.PoolingProductType),
            nameof(Shipping.TarifficationType),
            nameof(Shipping.TotalDeliveryCost),
            nameof(Shipping.VehicleTypeId),
            nameof(Shipping.WeightKg),
        };

        public SendUpdateShippingNotification(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            foreach (var change in changes)
            {
                var entity = change.Entity;
                if ((entity.Status == ShippingState.ShippingRequestSent || entity.Status == ShippingState.ShippingConfirmed)
                    && entity.TarifficationType != TarifficationType.Milkrun
                    && entity.TarifficationType != TarifficationType.Pooling)
                {
                    var notificationData = new NotificationShippingChangesDto
                    {
                        Orders = new List<NotificationOrderChangesDto>
                        {
                            new NotificationOrderChangesDto
                            {
                                OrderNumber = null,
                                ChangesFields = change.FieldChanges.Select(x => x.FieldName)
                                                                   .Where(x => _watchProperties.Any(y => y.ToLower() == x.ToLower()))
                                                                   .ToList()
                            }
                        }
                    };
                    _notificationService.SendUpdateShippingRequestDataNotification(entity.Id, notificationData);
                }
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(_watchProperties);
        }
    }
}
