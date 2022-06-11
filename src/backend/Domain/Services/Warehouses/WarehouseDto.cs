using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Warehouses
{
    public class WarehouseDto : ICompanyDto
    {
        public string Id { get; set; }

        [DisplayNameKey("Warehouse.WarehouseName")]
        [FieldType(FieldType.Text), OrderNumber(1), IsRequired]
        public string WarehouseName { get; set; }

        [DisplayNameKey("Warehouse.Client")]
        [FieldType(FieldType.Text), OrderNumber(2), IsRequired]
        public string Client { get; set; }

        [FieldType(FieldType.Text), OrderNumber(3)]
        public string SoldToNumber { get; set; }

        public string PostalCode { get; set; }

        [FieldType(FieldType.Text), OrderNumber(4)]
        public string Region { get; set; }

        public string Area { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5)]
        public string City { get; set; }

        public string Street { get; set; }

        public string House { get; set; }

        [FieldType(FieldType.Text), OrderNumber(6)]
        public string Address { get; set; }

        public string ValidAddress { get; set; }

        public string UnparsedAddressParts { get; set; }

        [FieldType(FieldType.Select, source: nameof(PickingTypes)), OrderNumber(7)]
        public LookUpDto PickingTypeId { get; set; }

        [FieldType(FieldType.Text), OrderNumber(8)]
        public string PickingFeatures { get; set; }

        [FieldType(FieldType.Number), OrderNumber(9)]
        public int? LeadtimeDays { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.DeliveryType)), IsRequired, OrderNumber(10)]
        public LookUpDto DeliveryType { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(11)]
        public bool? IsActive { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), OrderNumber(12)]
        public LookUpDto CompanyId { get; set; }

        public string AdditionalInfo { get; set; }
    }
}