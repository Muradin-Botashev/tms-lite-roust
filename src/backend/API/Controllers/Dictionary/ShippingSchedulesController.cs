using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.ShippingSchedules;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/shippingSchedules")]
    public class ShippingSchedulesController : DictionaryController<IShippingSchedulesService, ShippingSchedule, ShippingScheduleDto, ShippingScheduleFilterDto>
    {
        public ShippingSchedulesController(IShippingSchedulesService vehicleTypesService, IAppConfigurationService appConfigurationService) 
            : base(vehicleTypesService, appConfigurationService)
        {
        }
    }
}
