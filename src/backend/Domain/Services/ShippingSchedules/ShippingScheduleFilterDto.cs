using Domain.Shared.FormFilters;

namespace Domain.Services.ShippingSchedules
{
    public class ShippingScheduleFilterDto : SearchFilterDto
    {
        public string ShippingCity { get; set; }
        public string DeliveryCity { get; set; }
        public string CarrierId { get; set; }
        public string ShippingDays { get; set; }
        public string DeliveryDays { get; set; }
    }
}
