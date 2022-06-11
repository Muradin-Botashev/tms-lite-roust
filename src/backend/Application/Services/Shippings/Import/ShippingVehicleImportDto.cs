using Domain.Enums;
using Domain.Extensions;

namespace Application.Services.Shippings.Import
{
    public class ShippingVehicleImportDto
    {
        [FieldType(FieldType.Text), OrderNumber(1)]
        public string ShippingNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(2), MaxLength(100)]
        public string DriverName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(3)]
        public string DriverPassportData { get; set; }

        [FieldType(FieldType.Text), OrderNumber(4), MaxLength(50)]
        public string DriverPhone { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5), MaxLength(20)]
        public string VehicleNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(6), MaxLength(50)]
        public string TrailerNumber { get; set; }

        [FieldType(FieldType.Text), OrderNumber(7), MaxLength(50)]
        public string VehicleMake { get; set; }

        [FieldType(FieldType.Text), OrderNumber(8)]
        public string VehicleType { get; set; }
    }
}
