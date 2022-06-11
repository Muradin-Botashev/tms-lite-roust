using Domain.Enums;
using System;

namespace Domain.Persistables
{
    public class NotificationEvent : IPersistable
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public Guid? InitiatorId { get; set; }
        public NotificationType Type { get; set; }
        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProcessed { get; set; }
    }
}
