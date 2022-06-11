using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.PickingTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/pickingTypes")]
    public class PickingTypesController : DictionaryController<IPickingTypesService, PickingType, PickingTypeDto, PickingTypeFilterDto>
    {
        public PickingTypesController(IPickingTypesService vehicleTypesService, IAppConfigurationService appConfigurationService) : base(vehicleTypesService, appConfigurationService)
        {
        }
    }
}