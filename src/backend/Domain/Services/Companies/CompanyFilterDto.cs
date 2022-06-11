using Domain.Shared.FormFilters;

namespace Domain.Services.Companies
{
    public class CompanyFilterDto: SearchFilterDto
    {
        public string Name { get; set; }

        public string PoolingProductType { get; set; }

        public string PoolingToken { get; set; }

        public string OrderRequiresConfirmation { get; set; }

        public string NewShippingTarifficationType { get; set; }

        public string IsActive { get; set; }
    }
}
