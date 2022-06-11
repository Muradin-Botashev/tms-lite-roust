using Domain.Shared.FormFilters;

namespace Domain.Services.DocumentTypes
{
    public class DocumentTypeFilterDto : SearchFilterDto
    {
        public string Name { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}
