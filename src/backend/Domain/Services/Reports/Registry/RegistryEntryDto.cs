using Domain.Enums;
using Domain.Extensions;
using System;

namespace Domain.Services.Reports.Registry
{
    public class RegistryEntryDto
    {
        public Guid OrderId { get; set; }
        public Guid ShippingId { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(1)]
        public int Number { get; set; }

        [FieldType(FieldType.Number), OrderNumber(2)]
        public decimal? OrderPalletsCount { get; set; }

        [FieldType(FieldType.Number), OrderNumber(3)]
        public decimal? OrderWeight { get; set; }

        [FieldType(FieldType.Number), OrderNumber(4)]
        public decimal? OrderVolume { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5)]
        public string OrderNumber { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(6)]
        public int NumberInDay { get; set; }

        [FieldType(FieldType.Date), OrderNumber(7)]
        public DateTime? ShippingDate { get; set; }

        [FieldType(FieldType.Time), OrderNumber(8)]
        public TimeSpan? ShippingTime { get; set; }

        [FieldType(FieldType.Text), OrderNumber(9)]
        public string CompanyName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(10)]
        public string DeliveryWarehouseName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(11)]
        public string ClientName { get; set; }

        [FieldType(FieldType.Number), OrderNumber(12)]
        public decimal? ShippingWeight { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(13)]
        public int? ShippingPalletsCount { get; set; }

        [FieldType(FieldType.Date), OrderNumber(14)]
        public DateTime? DeliveryDate { get; set; }

        [FieldType(FieldType.Time), OrderNumber(15)]
        public TimeSpan? DeliveryTime { get; set; }

        [FieldType(FieldType.Text), OrderNumber(16)]
        public string BodyTypeName { get; set; }

        [FieldType(FieldType.Number), OrderNumber(17)]
        public decimal? Tonnage { get; set; }

        [FieldType(FieldType.Number), OrderNumber(18)]
        public decimal? TenderDeliveryCost { get; set; }

        [FieldType(FieldType.Number), OrderNumber(19)]
        public decimal? DeliveryCost { get; set; }

        [FieldType(FieldType.Number), OrderNumber(20)]
        public decimal? OldDeliveryCost { get; set; }

        [FieldType(FieldType.Text), OrderNumber(21)]
        public string PlanningCarrierName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(22)]
        public string CarrierName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(23)]
        public string VehicleNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(24)]
        public string DriverName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(25)]
        public string DriverPhone { get; set; }

        [FieldType(FieldType.Text), OrderNumber(26)]
        public string Comments { get; set; }

        [FieldType(FieldType.Text), OrderNumber(27)]
        public string ShippingNumber { get; set; }

        [FieldType(FieldType.Number), OrderNumber(28)]
        public decimal? LoadingDowntimeCost { get; set; }

        [FieldType(FieldType.Number), OrderNumber(29)]
        public decimal? UnloadingDowntimeCost { get; set; }

        [FieldType(FieldType.Number), OrderNumber(30)]
        public decimal? ReturnCost { get; set; }

        [FieldType(FieldType.Number), OrderNumber(31)]
        public decimal? OrderDeliveryCost { get; set; }
    }
}
