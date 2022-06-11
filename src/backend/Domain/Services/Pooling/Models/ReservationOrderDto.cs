namespace Domain.Services.Pooling.Models
{
    public class ReservationOrderDto
    {
        public string Number { get; set; }

        public string WaybillNumber { get; set; }

        public string ConsignorNumber { get; set; }

        public string PackingList { get; set; }

        public string Type { get; set; }

        public PoolingUnitsDto Units { get; set; }
    }
}
