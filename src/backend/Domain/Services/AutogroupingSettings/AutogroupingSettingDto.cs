using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.AutogroupingSettings
{
    public class AutogroupingSettingDto : ICompanyDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), IsRequired, OrderNumber(1)]
        public LookUpDto CompanyId { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(2)]
        public int? MaxUnloadingPoints { get; set; }

        [FieldType(FieldType.Number), OrderNumber(3)]
        public decimal? RegionOverrunCoefficient { get; set; }

        [FieldType(FieldType.Number), OrderNumber(4)]
        public decimal? InterregionOverrunCoefficient { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(6)]
        public bool? CheckPoolingSlots { get; set; }

        [FieldType(FieldType.Select, source: nameof(Tonnages)), OrderNumber(7)]
        public LookUpDto TonnageId { get; set; }
    }
}
