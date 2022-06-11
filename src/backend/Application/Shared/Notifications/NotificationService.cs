using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Application.Shared.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public NotificationService(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public void SendRequestToCarrierNotification(Guid shippingId)
        {
            SendNotification(shippingId, NotificationType.SendRequestToCarrier, null);
        }

        public void SendAddOrdersToShippingNotification(Guid shippingId, NotificationOrdersListDto data)
        {
            SendNotification(shippingId, NotificationType.AddOrdersToShipping, data);
        }

        public void SendRemoveOrdersFromShippingNotification(Guid shippingId, NotificationOrdersListDto data)
        {
            SendNotification(shippingId, NotificationType.RemoveOrdersFromShipping, data);
        }

        public void SendUpdateShippingRequestDataNotification(Guid shippingId, NotificationShippingChangesDto data)
        {
            SendNotification(shippingId, NotificationType.UpdateShippingRequestData, data);
        }

        public void SendRejectShippingRequestNotification(Guid shippingId)
        {
            SendNotification(shippingId, NotificationType.RejectShippingRequest, null);
        }

        public void SendCancelShippingNotification(Guid shippingId, CancelNotificationDto data)
        {
            SendNotification(shippingId, NotificationType.CancelShipping, data);
        }

        private void SendNotification(Guid entityId, NotificationType type, object data)
        {
            var dbSet = _dataService.GetDbSet<NotificationEvent>();

            NotificationType[] deactivatedTypes = null;
            switch (type)
            {
                case NotificationType.RejectShippingRequest:
                    deactivatedTypes = new[] {
                        NotificationType.AddOrdersToShipping,
                        NotificationType.RemoveOrdersFromShipping,
                        NotificationType.SendRequestToCarrier,
                        NotificationType.UpdateShippingRequestData
                    };
                    break;

                case NotificationType.CancelShipping:
                    deactivatedTypes = new[] {
                        NotificationType.AddOrdersToShipping,
                        NotificationType.RejectShippingRequest,
                        NotificationType.RemoveOrdersFromShipping,
                        NotificationType.SendRequestToCarrier,
                        NotificationType.UpdateShippingRequestData
                    };
                    break;
            }

            if (deactivatedTypes != null && deactivatedTypes.Any())
            {
                var eventsToDeactivate = dbSet.Where(x => x.EntityId == entityId && deactivatedTypes.Contains(x.Type) && !x.IsProcessed).ToList();
                foreach (var @event in eventsToDeactivate)
                {
                    @event.IsProcessed = true;
                }
            }

            var dataStr = data == null ? null : JsonConvert.SerializeObject(data);

            var notification = new NotificationEvent
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                Type = type,
                Data = dataStr,
                InitiatorId = _userProvider.GetCurrentUserId(),
                IsProcessed = false,
                CreatedAt = DateTime.Now
            };
            dbSet.Add(notification);
        }
    }
}
