using Domain.Shared.FormFilters;

namespace Domain.Services.Leadtime
{
    public class LeadtimeFilterDto : SearchFilterDto
    {
        public string LeadtimeDays { get; set; }

        public string ClientName { get; set; }

        public string DeliveryAddress { get; set; }

        public string ShippingWarehouseId { get; set; }
    }
}