using Domain.Shared;

namespace Domain.Services.Warehouses
{
    public class SoldToDto : LookUpDto
    {
        public string Id { get; set; }
        public LookUpDto WarehouseName { get; set; }
        public LookUpDto DeliveryWarehouseId { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PickingTypeId { get; set; }
        public int? LeadtimeDays { get; set; }
    }
}
