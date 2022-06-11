using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.Orders
{
    public class OrderDto : IListDto, IValidatedDto
    {
        public string Id { get; set; }

        [DisplayNameKey("Order.Status")]
        [FieldType(FieldType.State, source: nameof(OrderState)), IsDefault, OrderNumber(2), IsReadOnly]
        public string Status { get; set; }

        [FieldType(FieldType.Link, source: nameof(Orders)), IsDefault, OrderNumber(1), IsReadOnly, IsRequired]
        public string OrderNumber { get; set; }

        [FieldType(FieldType.Text)]
        public string ClientOrderNumber { get; set; }

        [FieldType(FieldType.Date)]
        public string OrderDate { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.OrderType)), IsReadOnly]
        public LookUpDto OrderType { get; set; }

        [FieldType(FieldType.Text), IsDefault, OrderNumber(6)]
        public string Payer { get; set; }

        [FieldType(FieldType.Select, source: nameof(ClientName), showRawValue: true), IsDefault, OrderNumber(5), IsReadOnly]
        public LookUpDto ClientName { get; set; }

        [FieldType(FieldType.Text), IsReadOnly]
        public string SoldTo { get; set; }

        [FieldType(FieldType.Integer)]
        public int? TemperatureMin { get; set; }

        [FieldType(FieldType.Integer)]
        public int? TemperatureMax { get; set; }

        [FieldType(FieldType.DateTime), AllowBulkUpdate, IsRequired]
        public string ShippingDate { get; set; }

        [FieldType(FieldType.Integer)]
        public int? TransitDays { get; set; }

        [FieldType(FieldType.DateTime), IsDefault, OrderNumber(7), AllowBulkUpdate, IsRequired]
        public string DeliveryDate { get; set; }

        public bool ManualDeliveryDate { get; set; }

        [FieldType(FieldType.Integer)]
        public int? ArticlesCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? BoxesCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? ConfirmedBoxesCount { get; set; }

        [FieldType(FieldType.Number), AllowBulkUpdate, IsRequired]
        public decimal? PalletsCount { get; set; }

        public bool? ManualPalletsCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? ConfirmedPalletsCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? ActualPalletsCount { get; set; }

        [FieldType(FieldType.Number), Round(0)]
        public decimal? WeightKg { get; set; }

        [FieldType(FieldType.Number), Round(0)]
        public decimal? ActualWeightKg { get; set; }

        [FieldType(FieldType.Number), Round(0)]
        public decimal? Volume { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? OrderAmountExcludingVAT { get; set; }

        public decimal? InvoiceAmountExcludingVAT { get; set; }

        [FieldType(FieldType.Text, FieldType.Select, source: nameof(ShippingWarehouseRegion), showRawValue: true)]
        public string ShippingRegion { get; set; }

        [FieldType(FieldType.Text, FieldType.Select, source: nameof(ShippingWarehouseCity), showRawValue: true)]
        public string ShippingCity { get; set; }

        [FieldType(FieldType.Text, FieldType.Select, source: nameof(WarehouseRegion), showRawValue: true)]
        public string DeliveryRegion { get; set; }

        [FieldType(FieldType.Text, FieldType.Select, source: nameof(WarehouseCity), showRawValue: true)]
        public string DeliveryCity { get; set; }

        [FieldType(FieldType.BigText), IsRequired]
        public string ShippingAddress { get; set; }

        [FieldType(FieldType.BigText), IsRequired]
        public string DeliveryAddress { get; set; }

        [FieldType(FieldType.State, source: nameof(VehicleState))]
        public string ShippingStatus { get; set; }

        [FieldType(FieldType.State, source: nameof(VehicleState))]
        public string DeliveryStatus { get; set; }

        [FieldType(FieldType.Text)]
        public string OrderComments { get; set; }

        [FieldType(FieldType.Select, source: nameof(PickingTypes)), AllowBulkUpdate]
        public LookUpDto PickingTypeId { get; set; }

        public string PlannedArrivalTimeSlotBDFWarehouse { get; set; }

        [FieldType(FieldType.DateTime)]
        public string LoadingArrivalTime { get; set; }

        [FieldType(FieldType.DateTime)]
        public string LoadingDepartureTime { get; set; }

        [FieldType(FieldType.DateTime)]
        public string UnloadingArrivalTime { get; set; }

        [FieldType(FieldType.DateTime)]
        public string UnloadingDepartureTime { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? TrucksDowntime { get; set; }

        [FieldType(FieldType.Text)]
        public string ReturnInformation { get; set; }

        [FieldType(FieldType.Text)]
        public string ReturnShippingAccountNo { get; set; }

        [FieldType(FieldType.Date)]
        public string PlannedReturnDate { get; set; }

        [FieldType(FieldType.Date)]
        public string ActualReturnDate { get; set; }

        [FieldType(FieldType.Text)]
        public string MajorAdoptionNumber { get; set; }

        [FieldType(FieldType.LocalDateTime), IsDefault, OrderNumber(8), IsReadOnly]
        public DateTime? OrderCreationDate { get; set; }

        public bool? WaybillTorg12 { get; set; }

        public bool? Invoice { get; set; }

        [FieldType(FieldType.Date)]
        public string DocumentsReturnDate { get; set; }

        [FieldType(FieldType.Date), AllowBulkUpdate]
        public string ActualDocumentsReturnDate { get; set; }

        public string ShippingId { get; set; }

        [FieldType(FieldType.Link, source: nameof(Shippings)), IsDefault, OrderNumber(3), IsReadOnly]
        public string ShippingNumber { get; set; }

        [FieldType(FieldType.State, source: nameof(OrderShippingStatus)), IsDefault, OrderNumber(4), IsReadOnly]
        public string OrderShippingStatus { get; set; }

        public bool? IsActive { get; set; }

        public string AdditionalInfo { get; set; }

        [FieldType(FieldType.Select, source: "ShippingWarehousesForOrderCreation")]
        public LookUpDto ShippingWarehouseId { get; set; }

        [FieldType(FieldType.Select, source: nameof(Warehouses))]
        public LookUpDto DeliveryWarehouseId { get; set; }

        [FieldType(FieldType.LocalDateTime), IsReadOnly]
        public DateTime? OrderChangeDate { get; set; }

        public bool? OrderConfirmed { get; set; }

        [FieldType(FieldType.Boolean)]
        public bool? DocumentReturnStatus { get; set; }

        [FieldType(FieldType.Text), IsReadOnly]
        public string PickingFeatures { get; set; }

        [FieldType(FieldType.Select, source: nameof(TransportCompanies), EmptyValue = EmptyValueOptions.FilterOnly), AllowBulkUpdate]
        public LookUpDto CarrierId { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.DeliveryType))]
        public LookUpDto DeliveryType { get; set; }

        [FieldType(FieldType.BigText)]
        public string DeviationsComment { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? DeliveryCost { get; set; }

        public bool? ManualDeliveryCost { get; set; }

        public string Source { get; set; }

        [FieldType(FieldType.Enum, source: nameof(TarifficationType))]
        public LookUpDto TarifficationType { get; set; }

        [FieldType(FieldType.Select, source: nameof(VehicleTypes))]
        public LookUpDto VehicleTypeId { get; set; }

        [FieldType(FieldType.Text), IsReadOnly]
        public string BookingNumber { get; set; }

        [FieldType(FieldType.Number), IsReadOnly]
        public decimal? DowntimeAmount { get; set; }

        [FieldType(FieldType.Number), IsReadOnly]
        public decimal? OtherExpenses { get; set; }

        [FieldType(FieldType.Number), IsReadOnly]
        public decimal? TotalAmount { get; set; }

        [FieldType(FieldType.Number), IsReadOnly]
        public decimal? TotalAmountNds { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? ReturnShippingCost { get; set; }

        [FieldType(FieldType.Text)]
        public string DeliveryAccountNumber { get; set; }

        [DisplayNameKey("Order.DocumentAttached")]
        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool DocumentAttached { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed), AllowBulkUpdate]
        public bool? AmountConfirmed { get; set; }

        [FieldType(FieldType.Text), MaxLength(100)]
        public string DriverName { get; set; }

        [FieldType(FieldType.Text), MaxLength(50)]
        public string DriverPhone { get; set; }

        [FieldType(FieldType.Text), MaxLength(20)]
        public string VehicleNumber { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? IsPooling { get; set; }

        [FieldType(FieldType.Text)]
        public string DriverPassportData { get; set; }

        [FieldType(FieldType.Text), MaxLength(50)]
        public string VehicleMake { get; set; }

        [FieldType(FieldType.Text), MaxLength(50)]
        public string TrailerNumber { get; set; }

        [FieldType(FieldType.Enum, source: nameof(WarehouseOrderState))]
        public LookUpDto ShippingWarehouseState { get; set; }

        public IEnumerable<string> Backlights { get; set; }

        [FieldType(FieldType.Select, source: nameof(BodyTypes))]
        public LookUpDto BodyTypeId { get; set; }

        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.Allowed)]
        public bool? IsReturn { get; set; }

        [FieldType(FieldType.Select, source: nameof(Companies)), IsReadOnly]
        public LookUpDto CompanyId { get; set; }

        public DetailedValidationResult ValidationResult { get; set; }

        [FieldType(FieldType.Text)]
        public string TransportZone { get; set; }

        [FieldType(FieldType.Number)]
        public int? BottlesCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? Volume9l { get; set; }

        [FieldType(FieldType.Text)]
        public string PaymentCondition { get; set; }

        public override string ToString()
        {
            return OrderNumber;
        }
    }
}