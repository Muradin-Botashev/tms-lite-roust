using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.AutogroupingSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/autogroupingSettings")]
    public class AutogroupingSettingsController : DictionaryController<IAutogroupingSettingsService, AutogroupingSetting, AutogroupingSettingDto, AutogroupingSettingFilterDto>
    {
        public AutogroupingSettingsController(IAutogroupingSettingsService autogroupingSettingsService, IAppConfigurationService appConfigurationService) : base(autogroupingSettingsService, appConfigurationService)
        {
        }
    }
}