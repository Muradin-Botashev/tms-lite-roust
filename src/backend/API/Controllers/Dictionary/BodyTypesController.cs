using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.BodyTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/bodyTypes")]
    public class BodyTypesController : DictionaryController<IBodyTypesService, BodyType, BodyTypeDto, BodyTypeFilterDto>
    {
        public BodyTypesController(IBodyTypesService service, IAppConfigurationService appConfigurationService) : base(service, appConfigurationService) { }

    }
}
