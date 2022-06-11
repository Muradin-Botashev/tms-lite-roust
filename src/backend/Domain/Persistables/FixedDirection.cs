using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class FixedDirection : IPersistable
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        /// <summary>
        /// Склад отгрузки
        /// </summary>
        [ReferenceType(typeof(ShippingWarehouse))]
        public Guid? ShippingWarehouseId { get; set; }

        [SortKey(nameof(Persistables.ShippingWarehouse.WarehouseName))]
        public ShippingWarehouse ShippingWarehouse { get; set; }

        /// <summary>
        /// Склад доставки
        /// </summary>
        [ReferenceType(typeof(Warehouse))]
        public Guid? DeliveryWarehouseId { get; set; }

        [SortKey(nameof(Warehouse.WarehouseName))]
        public Warehouse DeliveryWarehouse { get; set; }

        /// <summary>
        /// Город отгрузки
        /// </summary>
        public string ShippingCity { get; set; }

        /// <summary>
        /// Город доставки
        /// </summary>
        public string DeliveryCity { get; set; }

        /// <summary>
        /// Регион отгрузки
        /// </summary>
        public string ShippingRegion { get; set; }

        /// <summary>
        /// Регион доставки
        /// </summary>
        public string DeliveryRegion { get; set; }

        /// <summary>
        /// Типы ТС
        /// </summary>
        public Guid[] VehicleTypeIds { get; set; }

        /// <summary>
        /// Квота (%)
        /// </summary>
        public decimal? Quota { get; set; }

        /// <summary>
        /// Активность
        /// </summary>
        public bool IsActive { get; set; }
    }
}
