using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.ShippingWarehouses
{
    public class ShippingWarehouseDto : ICompanyDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), OrderNumber(1), IsRequired]
        public string Code { get; set; }

        [FieldType(FieldType.Text), OrderNumber(2), IsRequired]
        public string WarehouseName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5)]
        public string Address { get; set; }

        public string ValidAddress { get; set; }

        public string PostalCode { get; set; }

        [FieldType(FieldType.Text), OrderNumber(3)]
        public string Region { get; set; }

        public string Area { get; set; }

        [FieldType(FieldType.Text), OrderNumber(4)]
        public string City { get; set; }

        public string Street { get; set; }

        public string House { get; set; }

        [FieldType(FieldType.Text), OrderNumber(6)]
        public string PoolingConsolidationId { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), OrderNumber(8)]
        public bool? IsActive { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), IsRequired, OrderNumber(7)]
        public LookUpDto CompanyId { get; set; }
    }
}
