using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class ShippingSchedule : IPersistable
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Город отгрузки
        /// </summary>
        public string ShippingCity { get; set; }

        /// <summary>
        /// Город доставки
        /// </summary>
        public string DeliveryCity { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        /// <summary>
        /// День отгрузки
        /// </summary>
        public int[] ShippingDays { get; set; }

        /// <summary>
        /// День доставки
        /// </summary>
        public int[] DeliveryDays { get; set; }
    }
}
