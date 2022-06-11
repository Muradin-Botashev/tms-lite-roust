using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Articles
{
    public class ArticleDto : ICompanyDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), IsRequired, OrderNumber(1)]
        public string Description { get; set; }

        [FieldType(FieldType.Text), IsRequired, OrderNumber(2)]
        public string Nart { get; set; }

        [DisplayNameKey("article.TemperatureRegime")]
        [FieldType(FieldType.Text), IsRequired, OrderNumber(3)]
        public string TemperatureRegime { get; set; }
        [FieldType(FieldType.Select, source: nameof(Companies)), IsRequired, OrderNumber(4)]
        public LookUpDto CompanyId { get; set; }
    }
}