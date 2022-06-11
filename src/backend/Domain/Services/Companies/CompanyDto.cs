using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Companies
{
    public class CompanyDto : IDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), OrderNumber(1), IsRequired]
        public string Name { get; set; }

        [FieldType(FieldType.Enum, source: nameof(PoolingProductType)), OrderNumber(2)]
        public LookUpDto PoolingProductType { get; set; }

        [FieldType(FieldType.BigText), OrderNumber(3)]
        public string PoolingToken { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(4)]
        public bool? OrderRequiresConfirmation { get; set; }

        [FieldType(FieldType.Enum, source: nameof(TarifficationType)), OrderNumber(5)]
        public LookUpDto NewShippingTarifficationType { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(6)]
        public bool? IsActive { get; set; }
    }
}
