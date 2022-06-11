using System;

namespace Application.Services.Autogrouping
{
    public class CostData
    {
        public bool IsValid { get; set; }
        public decimal? Value { get; set; }
        public string Message { get; set; }
        public Guid? CarrierId { get; set; }
    }
}
