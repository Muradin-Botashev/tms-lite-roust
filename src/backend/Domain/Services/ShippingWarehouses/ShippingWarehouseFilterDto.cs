using Domain.Shared.FormFilters;

namespace Domain.Services.ShippingWarehouses
{
    public class ShippingWarehouseFilterDto : SearchFilterDto
    {
        public string Code { get; set; }

        public string WarehouseName { get; set; }

        public string Address { get; set; }

        public string Region { get; set; }

        public string City { get; set; }

        public string IsActive { get; set; }

        public string PoolingConsolidationId { get; set; }

        public string CompanyId { get; set; }
    }
}
