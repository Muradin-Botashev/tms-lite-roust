using System;

namespace Domain.Persistables
{
    public class HistoryEntry : IPersistable
    {
        public Guid Id { get; set; }
        public Guid PersistableId { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MessageKey { get; set; }
        public string MessageArgs { get; set; }
    }
}
