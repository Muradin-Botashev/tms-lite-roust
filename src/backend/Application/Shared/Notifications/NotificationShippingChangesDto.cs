using System.Collections.Generic;

namespace Application.Shared.Notifications
{
    public class NotificationShippingChangesDto
    {
        public List<NotificationOrderChangesDto> Orders { get; set; }
    }
}
