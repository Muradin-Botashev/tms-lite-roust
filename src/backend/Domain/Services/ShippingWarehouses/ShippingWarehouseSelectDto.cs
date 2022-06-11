using Domain.Shared;

namespace Domain.Services.ShippingWarehouses
{
    public class ShippingWarehouseSelectDto : LookUpDto
    {
        public string Region { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
    }
}
