using Domain.Shared.FormFilters;

namespace Domain.Services.Tonnages
{
    public class TonnageFilterDto : SearchFilterDto
    {
        public string Name { get; set; }

        public string WeightKg { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}
