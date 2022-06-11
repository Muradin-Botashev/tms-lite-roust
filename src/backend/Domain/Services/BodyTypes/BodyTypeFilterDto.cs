using Domain.Shared.FormFilters;

namespace Domain.Services.BodyTypes
{
    public class BodyTypeFilterDto: SearchFilterDto
    {
        public string Name { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}
