using System;

namespace Domain.Services.Injections
{
    public class InjectionDto : IDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public DateTime ProcessTimeUtc { get; set; }
    }
}
