using System;

namespace Domain.Persistables
{
    
    public class Translation : IPersistableWithName
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Ru { get; set; }
        public string En { get; set; }
    }
}