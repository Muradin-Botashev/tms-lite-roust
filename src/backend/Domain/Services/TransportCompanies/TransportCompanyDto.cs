using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.TransportCompanies
{
    public class TransportCompanyDto : ICompanyDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), OrderNumber(1), IsRequired]
        public string Title { get; set; }

        [FieldType(FieldType.Text), OrderNumber(2)]
        public string PowerOfAttorneyNumber { get; set; }

        [FieldType(FieldType.Date), OrderNumber(3)]
        public string DateOfPowerOfAttorney { get; set; }

        [FieldType(FieldType.Text), OrderNumber(4), IsRequired]
        public string Email { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5)]
        public string ContactInfo { get; set; }

        [FieldType(FieldType.Text), OrderNumber(6), IsRequired]
        public string Forwarder { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(7), IsRequired]
        public int? RequestReviewDuration { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(8)]
        public bool? IsActive { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), OrderNumber(9)]
        public LookUpDto CompanyId { get; set; }
    }
}