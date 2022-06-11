using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.Shippings
{
    public class ShippingDto : IListDto, IValidatedDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Link, source: nameof(Shippings)), IsDefault, OrderNumber(1), IsReadOnly]
        public string ShippingNumber { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.DeliveryType)), IsDefault, OrderNumber(4)]
        public LookUpDto DeliveryType { get; set; }

        [FieldType(FieldType.Integer)]
        public int? TemperatureMin { get; set; }

        [FieldType(FieldType.Integer)]
        public int? TemperatureMax { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.TarifficationType)), IsDefault, OrderNumber(5)]
        public LookUpDto TarifficationType { get; set; }

        [FieldType(FieldType.Select, source: nameof(TransportCompanies)), IsDefault, OrderNumber(3)]
        public LookUpDto CarrierId { get; set; }

        [FieldType(FieldType.Select, source: nameof(VehicleTypes))]
        public LookUpDto VehicleTypeId { get; set; }

        [FieldType(FieldType.Select, source: nameof(BodyTypes))]
        public LookUpDto BodyTypeId { get; set; }

        [FieldType(FieldType.Select, source: nameof(ShippingWarehouses)), IsReadOnly]
        public LookUpDto ShippingWarehouseId { get; set; }

        [FieldType(FieldType.Text), IsReadOnly]
        public string ShippingAddress { get; set; }

        [FieldType(FieldType.Select, source: nameof(Warehouses)), IsReadOnly]
        public LookUpDto DeliveryWarehouseId { get; set; }

        [FieldType(FieldType.Text), IsReadOnly]
        public string DeliveryAddress { get; set; }

        [FieldType(FieldType.Integer)]
        public int? PalletsCount { get; set; }

        [FieldType(FieldType.Integer)]
        public int? ActualPalletsCount { get; set; }

        [FieldType(FieldType.Integer)]
        public int? ConfirmedPalletsCount { get; set; }

        [FieldType(FieldType.Number), Round(0)]
        public decimal? WeightKg { get; set; }

        [FieldType(FieldType.Number), Round(0)]
        public decimal? ActualWeightKg { get; set; }

        public string PlannedArrivalTimeSlotBDFWarehouse { get; set; }

        [FieldType(FieldType.DateTime)]
        public string LoadingArrivalTime { get; set; }

        [FieldType(FieldType.DateTime)]
        public string LoadingDepartureTime { get; set; }

        [FieldType(FieldType.Text)]
        public string DeliveryInvoiceNumber { get; set; }

        [FieldType(FieldType.Text)]
        public string DeviationReasonsComments { get; set; }

        [FieldType(FieldType.Number), IsReadOnly]
        public decimal? TotalDeliveryCostWithoutVAT { get; set; }

        [FieldType(FieldType.Number), IsReadOnly]
        public decimal? TotalDeliveryCost { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? OtherCosts { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? BasicDeliveryCostWithoutVAT { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? ReturnCostWithoutVAT { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? InvoiceAmountWithoutVAT { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? AdditionalCostsWithoutVAT { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? ExtraPointCostsWithoutVAT { get; set; }

        [FieldType(FieldType.Text)]
        public string AdditionalCostsComments { get; set; }

        [FieldType(FieldType.Text)]
        public string CostsComments { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? TrucksDowntime { get; set; }

        public decimal? ReturnRate { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? AdditionalPointRate { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? DowntimeRate { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? BlankArrivalRate { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? BlankArrival { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? Waybill { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? WaybillTorg12 { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? TransportWaybill { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? Invoice { get; set; }

        [FieldType(FieldType.Date)]
        public string DocumentsReturnDate { get; set; }

        [FieldType(FieldType.Date)]
        public string ActualDocumentsReturnDate { get; set; }

        [FieldType(FieldType.Text)]
        public string InvoiceNumber { get; set; }

        [DisplayNameKey("Shipping.Status")]
        [FieldType(FieldType.State, source: nameof(ShippingState)), IsDefault, OrderNumber(2), IsReadOnly]
        public string Status { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? CostsConfirmedByShipper { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? CostsConfirmedByCarrier { get; set; }

        [FieldType(FieldType.LocalDateTime), IsDefault, OrderNumber(6), IsReadOnly]
        public DateTime? ShippingCreationDate { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? IsPooling { get; set; }

        [FieldType(FieldType.Text), MaxLength(100)]
        public string DriverName { get; set; }

        [FieldType(FieldType.Text), MaxLength(50)]
        public string DriverPhone { get; set; }

        [FieldType(FieldType.Text)]
        public string DriverPassportData { get; set; }

        [FieldType(FieldType.Text), MaxLength(20)]
        public string VehicleNumber { get; set; }

        [FieldType(FieldType.Text), MaxLength(50)]
        public string VehicleMake { get; set; }

        [FieldType(FieldType.Text), MaxLength(50)]
        public string TrailerNumber { get; set; }

        [FieldType(FieldType.Number), Round(2), IsReadOnly]
        public decimal? LoadingDowntimeCost { get; set; }

        [FieldType(FieldType.Number), Round(2), IsReadOnly]
        public decimal? UnloadingDowntimeCost { get; set; }

        [DisplayNameKey("Shipping.ShippingDate")]
        [FieldType(FieldType.DateTime), IsReadOnly]
        public string ShippingDate { get; set; }

        [DisplayNameKey("Shipping.DeliveryDate")]
        [FieldType(FieldType.DateTime), IsReadOnly]
        public string DeliveryDate { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.PoolingProductType))]
        public LookUpDto PoolingProductType { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), IsReadOnly]
        public LookUpDto CompanyId { get; set; }

        public IEnumerable<string> Backlights { get; set; }

        public DetailedValidationResult ValidationResult { get; set; }

        [FieldType(FieldType.Text)]
        public string RouteNumber { get; set; }

        [FieldType(FieldType.Number)]
        public int? BottlesCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? Volume9l { get; set; }

        public override string ToString()
        {
            return ShippingNumber;
        }
    }
}