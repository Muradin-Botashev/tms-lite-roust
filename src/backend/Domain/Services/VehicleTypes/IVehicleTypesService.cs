using Domain.Persistables;

namespace Domain.Services.VehicleTypes
{
    public interface IVehicleTypesService : IDictonaryService<VehicleType, VehicleTypeDto, VehicleTypeFilterDto>
    {
    }
}
