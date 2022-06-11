using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;

namespace Domain.Services.Leadtime
{
    public class LeadtimeDto : IDto
    {
        public string Id { get; set; }

        [FieldType(FieldType.Number), OrderNumber(1), IsRequired]
        public int? LeadtimeDays { get; set; }

        [DisplayNameKey("leadtime.Client")]
        [FieldType(FieldType.Select, source: nameof(ClientName), showRawValue: true), OrderNumber(2), IsRequired]
        public LookUpDto ClientName { get; set; }

        [FieldType(FieldType.Select, source: nameof(WarehouseAddress), showRawValue: true, dependencies: new string[] { nameof(ClientName) }), OrderNumber(3), IsRequired]
        public LookUpDto DeliveryAddress { get; set; }

        [FieldType(FieldType.Select, source: nameof(ShippingWarehouses)), OrderNumber(4), IsRequired]
        public LookUpDto ShippingWarehouseId { get; set; }
    }
}