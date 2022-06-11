using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Reports
{
    public class OrderReportDto
    {
        [FieldType(FieldType.Enum, source: nameof(Enums.DeliveryType)), OrderNumber(1)]
        public LookUpDto DeliveryType { get; set; }

        [FieldType(FieldType.Select, source: nameof(ClientName), showRawValue: true), OrderNumber(2)]
        public LookUpDto ClientName { get; set; }

        [FieldType(FieldType.Date), OrderNumber(6)]
        public string ShippingDate { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(3)]
        public int? OrdersCount { get; set; }

        [FieldType(FieldType.Integer), OrderNumber(4)]
        public int? PalletsCount { get; set; }

        [FieldType(FieldType.Number), OrderNumber(5)]
        public decimal? OrderAmountExcludingVAT { get; set; }
    }
}
