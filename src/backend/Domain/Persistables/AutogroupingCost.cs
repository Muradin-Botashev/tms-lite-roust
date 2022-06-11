using Domain.Enums;
using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class AutogroupingCost : IPersistable
    {
        public Guid Id { get; set; }

        [ReferenceType(typeof(AutogroupingShipping))]
        public Guid AutogroupingShippingId { get; set; }

        [SortKey(nameof(Persistables.AutogroupingShipping.ShippingNumber))]
        public AutogroupingShipping AutogroupingShipping { get; set; }

        [ReferenceType(typeof(TransportCompany))]
        public Guid CarrierId { get; set; }

        [SortKey(nameof(TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        public AutogroupingType AutogroupingType { get; set; }

        public decimal? Value { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
