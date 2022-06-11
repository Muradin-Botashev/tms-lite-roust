using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class OpenRunShipping
    {
        public string ShippingNumber { get; set; }
        public List<string> WaybillNumbers { get; set; }
        public string AutogroupingType { get; set; }
        public string Carrier { get; set; }
        public decimal? DeliveryCost { get; set; }
    }
}
