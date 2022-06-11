using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Drivers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/drivers")]
    public class DriversController : DictionaryController<IDriversService, Driver, DriverDto, DriverFilterDto>
    {
        public DriversController(IDriversService driversService, IAppConfigurationService appConfigurationService) : base(driversService, appConfigurationService)
        {
        }
    }
}