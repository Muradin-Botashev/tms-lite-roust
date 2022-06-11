using Domain.Shared.FormFilters;

namespace Domain.Services.AutogroupingSettings
{
    public class AutogroupingSettingFilterDto : SearchFilterDto
    {
        public string CompanyId { get; set; }

        public string MaxUnloadingPoints { get; set; }

        public string RegionOverrunCoefficient { get; set; }

        public string InterregionOverrunCoefficient { get; set; }

        public string CheckPoolingSlots { get; set; }

        public string TonnageId { get; set; }

        public string AutogroupingTypes { get; set; }
    }
}
