using System;

namespace Application.Shared.Notifications
{
    public interface INotificationService
    {
        void SendRequestToCarrierNotification(Guid shippingId);
        void SendAddOrdersToShippingNotification(Guid shippingId, NotificationOrdersListDto data);
        void SendRemoveOrdersFromShippingNotification(Guid shippingId, NotificationOrdersListDto data);
        void SendUpdateShippingRequestDataNotification(Guid shippingId, NotificationShippingChangesDto data);
        void SendRejectShippingRequestNotification(Guid shippingId);
        void SendCancelShippingNotification(Guid shippingId, CancelNotificationDto data);
    }
}