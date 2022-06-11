using System;

namespace Application.Shared.Notifications
{
    public class DestinationPointDto
    {
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryWarehouseName { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryRegion { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
