using Domain.Persistables;
using Domain.Shared.FormFilters;
using System.Collections.Generic;

namespace Domain.Services.Injections
{
    public interface IInjectionsService : IDictonaryService<Injection, InjectionDto, SearchFilterDto>
    {
        IEnumerable<InjectionDto> GetByTaskName(string taskName);
    }
}
