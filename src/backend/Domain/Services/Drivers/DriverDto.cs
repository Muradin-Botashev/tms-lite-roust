using Domain.Extensions;

namespace Domain.Services.Drivers
{
    public class DriverDto : IDto
    {
        public string Id { get; set; }

        [DisplayNameKey("driver.Name")]
        [FieldType(Enums.FieldType.Text), OrderNumber(1), IsRequired]
        public string Name { get; set; }

        [DisplayNameKey("driver.DriverLicence")]
        [FieldType(Enums.FieldType.Text), OrderNumber(2), IsRequired]
        public string DriverLicence { get; set; }

        [DisplayNameKey("driver.Passport")]
        [FieldType(Enums.FieldType.Text), OrderNumber(3), IsRequired]
        public string Passport { get; set; }

        [DisplayNameKey("driver.Phone")]
        [FieldType(Enums.FieldType.Text), OrderNumber(5)]
        public string Phone { get; set; }

        [DisplayNameKey("driver.Email")]
        [FieldType(Enums.FieldType.Text), OrderNumber(6)]
        public string Email { get; set; }

        [DisplayNameKey("driver.IsBlackList")]
        [FieldType(Enums.FieldType.Boolean), OrderNumber(7)]
        public bool IsBlackList { get; set; }

        [DisplayNameKey("driver.IsActive")]
        [FieldType(Enums.FieldType.Boolean), OrderNumber(8)]
        public bool IsActive { get; set; }
    }
}