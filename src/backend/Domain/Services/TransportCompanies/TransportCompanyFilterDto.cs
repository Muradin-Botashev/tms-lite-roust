using Domain.Shared.FormFilters;

namespace Domain.Services.TransportCompanies
{
    public class TransportCompanyFilterDto : SearchFilterDto
    {
        public string Title { get; set; }

        public string PowerOfAttorneyNumber { get; set; }

        public string DateOfPowerOfAttorney { get; set; }

        public string Email { get; set; }

        public string ContactInfo { get; set; }

        public string Forwarder { get; set; }

        public string RequestReviewDuration { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}