using System.Collections.Generic;

namespace Domain.Services.Pooling.Models
{
    public class ReservationPointDto
    {
        public PoolingIdDto Warehouse { get; set; }

        public PoolingAddressDto Address { get; set; }

        public List<string> OrderNumbers { get; set; }

        public PoolingDateRangeDto DateTime { get; set; }
    }
}
