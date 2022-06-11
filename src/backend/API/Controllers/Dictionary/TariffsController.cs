using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Tariffs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/tariffs")]
    public class TariffsController : DictionaryController<ITariffsService, Tariff, TariffDto, TariffFilterDto> 
    {
        public TariffsController(ITariffsService tariffsService, IAppConfigurationService appConfigurationService) : base(tariffsService, appConfigurationService)
        {
        }
    }
}