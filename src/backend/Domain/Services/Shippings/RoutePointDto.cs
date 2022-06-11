using Domain.Enums;
using Domain.Extensions;
using System.Collections.Generic;

namespace Domain.Services.Shippings
{
    public class RoutePointDto
    {
        public string WarehouseName { get; set; }

        [FieldType(FieldType.Date)]
        public string PlannedDate { get; set; }

        [FieldType(FieldType.DateTime)]
        public string ArrivalTime { get; set; }

        [FieldType(FieldType.DateTime)]
        public string DepartureTime { get; set; }

        [FieldType(FieldType.State, source: nameof(VehicleState))]
        public string VehicleStatus { get; set; }

        [FieldType(FieldType.BigText)]
        public string Address { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? TrucksDowntime { get; set; }

        public bool IsLoading { get; set; }

        public List<string> OrderIds { get; set; }
    }
}
