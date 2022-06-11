using System.Collections.Generic;

namespace Domain.Services.Orders
{
    public class OrderFormDto : OrderDto
    {
        public List<OrderItemDto> Items { get; set; }
    }
}
