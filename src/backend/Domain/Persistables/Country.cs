using System;

namespace Domain.Persistables
{
    public class Country : IPersistable
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
