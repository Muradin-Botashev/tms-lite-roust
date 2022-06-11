using Domain.Shared;

namespace Domain.Services.ShippingWarehouses
{
    public class ShippingWarehouseDtoForSelect : LookUpDto
    {
        public string Address { get; set; }
    }
}