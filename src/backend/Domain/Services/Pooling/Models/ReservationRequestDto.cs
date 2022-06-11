using System.Collections.Generic;

namespace Domain.Services.Pooling.Models
{
    public class ReservationRequestDto
    {
        public string Id { get; set; }

        public string Number { get; set; }

        public string ForeignId { get; set; }

        public string Version { get; set; }

        public string ShippingType { get; set; }

        public string SlotId { get; set; }

        public PoolingIdDto Carrier { get; set; }

        public PoolingIdDto Client { get; set; }

        public PoolingIdDto Consignor { get; set; }

        public string CarType { get; set; }

        public string CarCapacityType { get; set; }

        public string ProductType { get; set; }

        public PoolingUnitsDto Units { get; set; }

        public List<ReservationOrderDto> Orders { get; set; }

        public List<ReservationPointDto> LoadingPoints { get; set; }

        public List<ReservationPointDto> UnloadingPoints { get; set; }

        public List<string> ServicesNeeded { get; set; }

        public PoolingTemperatureDto Temperature { get; set; }

        public string EditableUntil { get; set; }

    }
}
