using Domain.Persistables;

namespace Domain.Services.PickingTypes
{
    public interface IPickingTypesService : IDictonaryService<PickingType, PickingTypeDto, PickingTypeFilterDto>
    {
    }
}