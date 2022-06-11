using Domain.Persistables;

namespace Domain.Services.BodyTypes
{
    public interface IBodyTypesService : IDictonaryService<BodyType, BodyTypeDto, BodyTypeFilterDto>
    {
    }
}
