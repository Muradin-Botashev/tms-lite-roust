using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Autogrouping;
using System;

namespace Application.Services.Autogrouping
{
    public class OrderDto : IAutogroupingOrder
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public OrderState Status { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ShippingCity { get; set; }
        public string DeliveryCity { get; set; }
        public string ShippingRegion { get; set; }
        public string DeliveryRegion { get; set; }
        public string ShippingAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public Guid? ShippingWarehouseId { get; set; }
        public ShippingWarehouse ShippingWarehouse { get; set; }
        public Guid? DeliveryWarehouseId { get; set; }
        public Warehouse DeliveryWarehouse { get; set; }
        public Guid? BodyTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public decimal? PalletsCount { get; set; }
        public decimal? WeightKg { get; set; }
        public Guid? CompanyId { get; set; }
        public int? BottlesCount { get; set; }
        public string TransportZone { get; set; }
        public decimal? Volume9l { get; set; }
        public string PaymentCondition { get; set; }
    }
}
