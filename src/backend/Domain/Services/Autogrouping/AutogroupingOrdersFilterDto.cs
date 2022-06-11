namespace Domain.Services.Autogrouping
{
    public class AutogroupingOrdersFilterDto
    {
        public string OrderNumber { get; set; }
        public string ShippingWarehouseId { get; set; }
        public string DeliveryWarehouseId { get; set; }
        public string DeliveryRegion { get; set; }
        public string ShippingDate { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string PalletsCount { get; set; }
        public string WeightKg { get; set; }
        public string BodyTypeId { get; set; }
        public string VehicleTypeId { get; set; }
        public string Errors { get; set; }
        public string Search { get; set; }
    }
}
