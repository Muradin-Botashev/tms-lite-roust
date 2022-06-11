using System;

namespace Domain.Persistables
{
    public class FileStorage : IPersistableWithName
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}
