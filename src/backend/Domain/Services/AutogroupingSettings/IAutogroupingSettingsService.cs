using Domain.Persistables;

namespace Domain.Services.AutogroupingSettings
{
    public interface IAutogroupingSettingsService : IDictonaryService<AutogroupingSetting, AutogroupingSettingDto, AutogroupingSettingFilterDto>
    {
    }
}