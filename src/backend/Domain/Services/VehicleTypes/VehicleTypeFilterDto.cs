using Domain.Shared.FormFilters;

namespace Domain.Services.VehicleTypes
{
    public class VehicleTypeFilterDto : SearchFilterDto
    {
        public string Name { get; set; }

        public string TonnageId { get; set; }

        public string BodyTypeId { get; set; }

        public string PalletsCount { get; set; }

        public string IsInterregion { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}
