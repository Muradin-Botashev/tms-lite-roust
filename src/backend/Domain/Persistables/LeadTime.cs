using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class LeadTime : IPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Leadtime, дней
        /// </summary>
        public int? LeadtimeDays { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Адрес доставки
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Склад отгрузки
        /// </summary>
        [ReferenceType(typeof(ShippingWarehouse))]
        public Guid? ShippingWarehouseId { get; set; }

        [SortKey(nameof(Persistables.ShippingWarehouse.WarehouseName))]
        public ShippingWarehouse ShippingWarehouse { get; set; }
    }
}