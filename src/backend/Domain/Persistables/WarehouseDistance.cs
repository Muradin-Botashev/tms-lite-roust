using System;

namespace Domain.Persistables
{
    public class WarehouseDistance : IPersistable
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Склад отгрузки
        /// </summary>
        public Guid ShippingWarehouseId { get; set; }

        /// <summary>
        /// Склад доставки
        /// </summary>
        public Guid DeliveryWarehouseId { get; set; }

        /// <summary>
        /// Расстояние, м
        /// </summary>
        public decimal? Distance { get; set; }
    }
}
