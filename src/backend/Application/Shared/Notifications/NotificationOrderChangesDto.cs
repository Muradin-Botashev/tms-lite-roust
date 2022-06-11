using System.Collections.Generic;

namespace Application.Shared.Notifications
{
    public class NotificationOrderChangesDto
    {
        public string OrderNumber { get; set; }
        public List<string> ChangesFields { get; set; }
    }
}
