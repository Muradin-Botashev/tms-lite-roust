using Domain.Persistables;

namespace Domain.Services.Warehouses
{
    public interface IWarehousesService : IDictonaryService<Warehouse, WarehouseDto, WarehouseFilterDto>
    {
        WarehouseDto GetBySoldTo(string soldToNumber);
    }
}