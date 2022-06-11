using Domain.Shared.FormFilters;

namespace Domain.Services.Warehouses
{
    public class WarehouseFilterDto : SearchFilterDto
    {
        public string WarehouseName { get; set; }

        public string Client { get; set; }

        public string SoldToNumber { get; set; }

        public string Region { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string PickingTypeId { get; set; }

        public string PickingFeatures { get; set; }

        public string LeadtimeDays { get; set; }

        public string DeliveryType { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}
