using Domain.Enums;
using Domain.Extensions;

namespace Application.Services.Import
{
    public class InvoicesImportDto
    {
        [FieldType(FieldType.Text), OrderNumber(1)]
        public string OrderNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(2)]
        public string DeliveryAccountNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(3)]
        public decimal? ActualTotalDeliveryCostWithoutVAT { get; set; }

        [FieldType(FieldType.Text), OrderNumber(4)]
        public decimal? OtherExpenses { get; set; }

        [FieldType(FieldType.Number), OrderNumber(5)]
        public decimal? TrucksDowntime { get; set; }

        [FieldType(FieldType.Number), OrderNumber(6)]
        public decimal? DowntimeAmount { get; set; }

        [FieldType(FieldType.Text), OrderNumber(7)]
        public bool? Return { get; set; }

        [FieldType(FieldType.Number), OrderNumber(8)]
        public decimal? ReturnShippingCost { get; set; }
    }
}