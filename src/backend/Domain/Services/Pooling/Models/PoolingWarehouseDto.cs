namespace Domain.Services.Pooling.Models
{
    public class PoolingWarehouseDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string RegionId { get; set; }

        public bool isCarrierWarehouse { get; set; }

        public PoolingAddressDto AddressInfo { get; set; }
    }
}
