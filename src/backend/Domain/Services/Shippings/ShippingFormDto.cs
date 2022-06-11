using System.Collections.Generic;

namespace Domain.Services.Shippings
{
    public class ShippingFormDto : ShippingDto
    {
        public List<ShippingOrderDto> Orders { get; set; }
        public List<RoutePointDto> RoutePoints { get; set; }
        public List<OrderCostDto> OrderCosts { get; set; }
    }
}
