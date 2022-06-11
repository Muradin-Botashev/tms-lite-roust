using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class CarrierShippingAction : IPersistable
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
        /// Перевозка
        /// </summary>
        [ReferenceType(typeof(Shipping))]
        public Guid ShippingId { get; set; }

        [SortKey(nameof(Persistables.Shipping.ShippingNumber))]
        public Shipping Shipping { get; set; }

        /// <summary>
        /// Дата/время операции
        /// </summary>
        public DateTime ActionTime { get; set; }

        /// <summary>
        /// Описание операции
        /// </summary>
        public string ActionName { get; set; }
    }
}
