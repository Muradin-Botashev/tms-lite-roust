using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Application.Services.Autogrouping
{
    public class AutogroupingOrderExportDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Text), OrderNumber(1)]
        public string OrderNumber { get; set; }

        [FieldType(FieldType.Select, source: nameof(ShippingWarehouses)), OrderNumber(2)]
        public LookUpDto ShippingWarehouseId { get; set; }

        [FieldType(FieldType.Select, source: nameof(Warehouses)), OrderNumber(3)]
        public LookUpDto DeliveryWarehouseId { get; set; }

        [FieldType(FieldType.Text), OrderNumber(4)]
        public string DeliveryRegion { get; set; }

        [FieldType(FieldType.Date), OrderNumber(5)]
        public string ShippingDate { get; set; }

        [FieldType(FieldType.Date), OrderNumber(6)]
        public string DeliveryDate { get; set; }

        [FieldType(FieldType.Number), OrderNumber(8)]
        public decimal? PalletsCount { get; set; }

        [FieldType(FieldType.Number), OrderNumber(9)]
        public decimal? WeightKg { get; set; }

        [FieldType(FieldType.Select, source: nameof(BodyTypes)), OrderNumber(10)]
        public LookUpDto BodyTypeId { get; set; }

        [FieldType(FieldType.Select, source: nameof(VehicleTypes)), OrderNumber(11)]
        public LookUpDto VehicleTypeId { get; set; }

        [FieldType(FieldType.Text), OrderNumber(12)]
        public string ShippingNumber { get; set; }
    }
}
