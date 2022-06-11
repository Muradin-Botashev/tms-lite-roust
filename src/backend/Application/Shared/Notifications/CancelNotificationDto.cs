using System.Collections.Generic;

namespace Application.Shared.Notifications
{
    public class CancelNotificationDto
    {
        public List<DestinationPointDto> DeliveryPoints { get; set; }
    }
}
