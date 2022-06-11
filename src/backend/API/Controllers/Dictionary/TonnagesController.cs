using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Tonnages;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/tonnages")]
    public class TonnagesController : DictionaryController<ITonnagesService, Tonnage, TonnageDto, TonnageFilterDto>
    {
        public TonnagesController(ITonnagesService service, IAppConfigurationService appConfigurationService) : base(service, appConfigurationService) { }

    }
}
