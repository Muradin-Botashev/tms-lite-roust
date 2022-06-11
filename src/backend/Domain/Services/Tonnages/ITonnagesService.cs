using Domain.Persistables;

namespace Domain.Services.Tonnages
{
    public interface ITonnagesService : IDictonaryService<Tonnage, TonnageDto, TonnageFilterDto>
    {
    }
}
