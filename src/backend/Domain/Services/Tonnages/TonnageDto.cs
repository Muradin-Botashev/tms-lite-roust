using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Tonnages
{
    public class TonnageDto : ICompanyDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), OrderNumber(1), IsRequired]
        public string Name { get; set; }

        [FieldType(FieldType.Number), OrderNumber(2), IsRequired, DisplayNameKey("tonnageWeightKg")]
        public decimal? WeightKg { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(4)]
        public bool? IsActive { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), OrderNumber(3)]
        public LookUpDto CompanyId { get; set; }
    }
}
