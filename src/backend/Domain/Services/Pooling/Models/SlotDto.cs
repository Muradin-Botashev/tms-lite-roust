using System;

namespace Domain.Services.Pooling.Models
{
    public class SlotDto
    {
        public string Id { get; set; }

        public int? DayOfWeek { get; set; }

        public string DistributionCenterName { get; set; }

        public PoolingAddressDto DistributionCenterAddressInfo { get; set; }

        public DateTime AvailableUntil { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int? PalletCount { get; set; }

        public decimal Price { get; set; }

        public string WarehouseId { get; set; }

        public string ConsolidationDate { get; set; }

        public string ShippingType { get; set; }
    }
}
