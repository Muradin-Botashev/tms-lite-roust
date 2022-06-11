using System;

namespace Domain.Persistables
{
    public class Injection : IPersistable
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public DateTime ProcessTimeUtc { get; set; }
    }

    public static class InjectionStatus
    {
        public const string Failed = nameof(Failed);
        public const string Success = nameof(Success);
    }
}
