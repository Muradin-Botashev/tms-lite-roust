using Domain.Shared.FormFilters;

namespace Domain.Services.Articles
{
    public class ArticleFilterDto : SearchFilterDto
    {
        public string Nart { get; set; }

        public string Description { get; set; }

        public string CompanyId { get; set; }
        public string TemperatureRegime { get; set; }
    }
}