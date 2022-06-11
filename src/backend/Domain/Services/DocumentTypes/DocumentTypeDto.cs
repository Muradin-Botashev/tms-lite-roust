using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.DocumentTypes
{
    public class DocumentTypeDto : ICompanyDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), OrderNumber(1), IsRequired]
        public string Name { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(4)]
        public bool? IsActive { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), IsRequired, OrderNumber(3)]
        public LookUpDto CompanyId { get; set; }
    }
}
