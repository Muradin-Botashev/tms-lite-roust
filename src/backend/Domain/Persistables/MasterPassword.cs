using System;

namespace Domain.Persistables
{
    public class MasterPassword : IPersistable
    {
        public Guid Id { get; set; }
        public string Hash { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid AuthorId { get; set; }
        public bool IsActive { get; set; }
    }
}
