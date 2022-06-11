using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Leadtime;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/leadtime")]
    public class LeadtimesController : DictionaryController<ILeadtimeService, LeadTime, LeadtimeDto, LeadtimeFilterDto>
    {
        public LeadtimesController(ILeadtimeService leadtimeService, IAppConfigurationService appConfigurationService) : base(leadtimeService, appConfigurationService)
        {
        }
    }
}