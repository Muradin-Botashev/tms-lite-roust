using System;

namespace Domain.Persistables
{
    public class CarrierRequestDatesStat : IPersistable
    {
        public Guid Id { get; set; }
        public Guid ShippingId { get; set; }
        public Shipping Shipping { get; set; }
        public Guid CarrierId { get; set; }
        public TransportCompany Carrier { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }
}
