using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class AutogroupingOrder : IPersistable
    {
        public Guid Id { get; set; }

        public Guid RunId { get; set; }

        [ReferenceType(typeof(AutogroupingShipping))]
        public Guid? AutogroupingShippingId { get; set; }

        [SortKey(nameof(Persistables.AutogroupingShipping.ShippingNumber))]
        public AutogroupingShipping AutogroupingShipping { get; set; }

        [ReferenceType(typeof(Order))]
        public Guid OrderId { get; set; }

        [SortKey(nameof(Persistables.Order.OrderNumber))]
        public Order Order { get; set; }

        public string OrderNumber { get; set; }

        [ReferenceType(typeof(ShippingWarehouse))]
        public Guid? ShippingWarehouseId { get; set; }

        [SortKey(nameof(Persistables.ShippingWarehouse.WarehouseName))]
        public ShippingWarehouse ShippingWarehouse { get; set; }

        [ReferenceType(typeof(Warehouse))]
        public Guid? DeliveryWarehouseId { get; set; }

        [SortKey(nameof(Warehouse.WarehouseName))]
        public Warehouse DeliveryWarehouse { get; set; }

        [ReferenceType(typeof(VehicleType))]
        public Guid? VehicleTypeId { get; set; }

        [SortKey(nameof(Persistables.VehicleType.Name))]
        public VehicleType VehicleType { get; set; }

        [ReferenceType(typeof(BodyType))]
        public Guid? BodyTypeId { get; set; }

        [SortKey(nameof(Persistables.BodyType.Name))]
        public BodyType BodyType { get; set; }

        public DateTime? ShippingDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public TimeSpan? DeliveryTime { get; set; }

        public string DeliveryRegion { get; set; }

        public decimal? PalletsCount { get; set; }

        public decimal? WeightKg { get; set; }

        public string Errors { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
