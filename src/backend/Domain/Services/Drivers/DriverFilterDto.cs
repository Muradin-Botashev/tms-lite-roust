using Domain.Shared.FormFilters;

namespace Domain.Services.Drivers
{
    public class DriverFilterDto : SearchFilterDto
    {
        public string Name { get; set; }

        public string DriverLicence { get; set; }

        public string Passport { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string IsBlackList { get; set; }

        public string IsActive { get; set; }
    }
}