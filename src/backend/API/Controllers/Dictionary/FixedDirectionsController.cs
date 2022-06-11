using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.FixedDirections;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/fixedDirections")]
    public class FixedDirectionsController : DictionaryController<IFixedDirectionsService, FixedDirection, FixedDirectionDto, FixedDirectionFilterDto>
    {
        public FixedDirectionsController(IFixedDirectionsService service, IAppConfigurationService appConfigurationService) : base(service, appConfigurationService) { }

    }
}
