namespace Domain.Services.Autogrouping
{
    public class AutogroupingFilterDto
    {
        public string ShippingNumber { get; set; }
        public string CarrierId { get; set; }
        public string ShippingDate { get; set; }
        public string DeliveryDate { get; set; }
        public string OrdersCount { get; set; }
        public string PalletsCount { get; set; }
        public string Route { get; set; }
        public string AutogroupingType { get; set; }
        public string VehicleTypeId { get; set; }
        public string FtlDirectCost { get; set; }
        public string FtlRouteCost { get; set; }
        public string LtlCost { get; set; }
        public string PoolingCost { get; set; }
        public string MilkrunCost { get; set; }
        public string Search { get; set; }
    }
}
