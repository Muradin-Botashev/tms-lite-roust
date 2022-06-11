using Domain.Persistables;
using Domain.Shared.FormFilters;
using System.Collections.Generic;

namespace Domain.Services.TaskProperties
{
    public interface ITaskPropertiesService : IDictonaryService<TaskProperty, TaskPropertyDto, SearchFilterDto>
    {
        IEnumerable<TaskPropertyDto> GetByTaskName(string taskName);
    }
}
