using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Shippings
{
    public class CreateShippingOrderDto
    {
        [FieldType(FieldType.Integer), IsRequired]
        public int? PalletsFrom { get; set; }

        [FieldType(FieldType.Integer), IsRequired]
        public int? PalletsTo { get; set; }

        [FieldType(FieldType.Text), IsRequired]
        public string ClientOrderNumber { get; set; }

        [FieldType(FieldType.Text), IsRequired]
        public string OrderNumber { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.OrderType))]
        public LookUpDto OrderType { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? WeightKg { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? OrderAmountExcludingVAT { get; set; }
    }
}
