using Domain.Persistables;

namespace Domain.Services.Leadtime
{
    public interface ILeadtimeService : IDictonaryService<LeadTime, LeadtimeDto, LeadtimeFilterDto>
    {
    }
}