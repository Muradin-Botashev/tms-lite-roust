using Domain.Enums;
using Domain.Persistables;
using System;

namespace Domain.Services.Autogrouping
{
    public interface IAutogroupingOrder
    {
        Guid Id { get; }
        string OrderNumber { get; }
        OrderState Status { get; }
        DateTime? ShippingDate { get; }
        DateTime? DeliveryDate { get; }
        string ShippingCity { get; }
        string DeliveryCity { get; }
        string ShippingRegion { get; }
        string DeliveryRegion { get; }
        string ShippingAddress { get; }
        string DeliveryAddress { get; }
        Guid? ShippingWarehouseId { get; }
        ShippingWarehouse ShippingWarehouse { get; }
        Guid? DeliveryWarehouseId { get; }
        Warehouse DeliveryWarehouse { get; }
        Guid? BodyTypeId { get; }
        Guid? VehicleTypeId { get; }
        decimal? PalletsCount { get; }
        decimal? WeightKg { get; }
        Guid? CompanyId { get; }
    }
}
