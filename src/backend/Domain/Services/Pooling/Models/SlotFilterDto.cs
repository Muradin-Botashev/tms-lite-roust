namespace Domain.Services.Pooling.Models
{
    public class SlotFilterDto
    {
        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string DeliveryDateFrom { get; set; }

        public string DeliveryDateTo { get; set; }

        public string ShippingRegionId { get; set; }

        public string ClientId { get; set; }

        public string ClientForeignId { get; set; }

        public string CarrierId { get; set; }

        public string CarrierForeignId { get; set; }

        public string ProductType { get; set; }

        public string ShippingType { get; set; }


        public string UnloadingWarehouseId { get; set; }

        public string UnloadingWarehouseForeignId { get; set; }

        public string CarType { get; set; }

        public bool OnlyAvailable { get; set; }
    }
}
