using System;

namespace Domain.Services.Autogrouping
{
    public class OpenRunWaybill
    {
        public string Number { get; set; }
        public string ShippingWarehouseId { get; set; }
        public string ShippingAddress { get; set; }
        public string DeliveryWarehouseId { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string BodyType { get; set; }
        public decimal? PalletsCount { get; set; }
        public decimal? Weight { get; set; }
    }
}
