using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Autogrouping
{
    public class AutogroupingShippingDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Date), OrderNumber(1)]
        public string ShippingDate { get; set; }

        [FieldType(FieldType.Date), OrderNumber(2)]
        public string DeliveryDate { get; set; }

        [FieldType(FieldType.AutogroupingCarrier, filterType: FieldType.Select, source: nameof(TransportCompanies)), OrderNumber(3)]
        public AutogroupingCarrierDto CarrierId { get; set; }

        [FieldType(FieldType.Enum, source: nameof(AutogroupingType)), OrderNumber(4)]
        public LookUpDto AutogroupingType { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5)]
        public string ShippingNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(6)]
        public string Route { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(7)]
        public int OrdersCount { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(8)]
        public int? PalletsCount { get; set; }

        [FieldType(FieldType.Select, source: nameof(VehicleTypes)), OrderNumber(9)]
        public LookUpDto VehicleTypeId { get; set; }

        [FieldType(FieldType.ValidatedNumber, FilterType = FieldType.Number), Round(2), OrderNumber(10)]
        public RouteCostDto FtlDirectCost { get; set; }

        [FieldType(FieldType.ValidatedNumber, FilterType = FieldType.Number), Round(2), OrderNumber(11)]
        public RouteCostDto FtlRouteCost { get; set; }

        [FieldType(FieldType.ValidatedNumber, FilterType = FieldType.Number), Round(2), OrderNumber(12)]
        public RouteCostDto LtlCost { get; set; }

        [FieldType(FieldType.ValidatedNumber, FilterType = FieldType.Number), Round(2), OrderNumber(13)]
        public RouteCostDto PoolingCost { get; set; }

        [FieldType(FieldType.ValidatedNumber, FilterType = FieldType.Number), Round(2), OrderNumber(14)]
        public RouteCostDto MilkrunCost { get; set; }
    }
}
