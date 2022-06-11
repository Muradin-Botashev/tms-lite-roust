using Domain.Persistables;

namespace Domain.Services.ShippingWarehouses
{
    public interface IShippingWarehousesService : IDictonaryService<ShippingWarehouse, ShippingWarehouseDto, ShippingWarehouseFilterDto>
    {
        ShippingWarehouse GetByCode(string code);
    }
}
