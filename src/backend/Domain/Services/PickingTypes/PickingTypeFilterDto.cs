using Domain.Shared.FormFilters;

namespace Domain.Services.PickingTypes
{
    public class PickingTypeFilterDto : SearchFilterDto
    {
        public string Name { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}