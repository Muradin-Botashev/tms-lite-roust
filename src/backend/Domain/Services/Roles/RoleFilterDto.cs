using Domain.Shared.FormFilters;

namespace Domain.Services.Roles
{
    public class RoleFilterDto: SearchFilterDto
    {
        public string Name { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}