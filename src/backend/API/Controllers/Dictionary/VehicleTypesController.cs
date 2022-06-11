using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.VehicleTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/vehicleTypes")]
    public class VehicleTypesController : DictionaryController<IVehicleTypesService, VehicleType, VehicleTypeDto, VehicleTypeFilterDto>
    {
        public VehicleTypesController(IVehicleTypesService vehicleTypesService, IAppConfigurationService appConfigurationService) : base(vehicleTypesService, appConfigurationService)
        {
        }
    }
}
