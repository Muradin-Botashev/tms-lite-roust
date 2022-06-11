using System.Collections.Generic;

namespace Domain.Services.Pooling.Models
{
    public class ReservationDto : ReservationRequestDto
    {
        public string Status { get; set; }

        public PoolingVehicleDto Vehicle { get; set; }

        public PoolingDriverDto Driver { get; set; }

        public List<string> Services { get; set; }

        public int? Carriage { get; set; }
    }
}
