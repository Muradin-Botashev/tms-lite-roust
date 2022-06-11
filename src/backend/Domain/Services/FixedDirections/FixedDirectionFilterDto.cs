using Domain.Shared.FormFilters;

namespace Domain.Services.FixedDirections
{
    public class FixedDirectionFilterDto : SearchFilterDto
    {
        public string CarrierId { get; set; }
        public string ShippingWarehouseId { get; set; }
        public string DeliveryWarehouseId { get; set; }
        public string ShippingCity { get; set; }
        public string DeliveryCity { get; set; }
        public string ShippingRegion { get; set; }
        public string DeliveryRegion { get; set; }
        public string VehicleTypeIds { get; set; }
        public string Quota { get; set; }
        public string IsActive { get; set; }
    }
}
