using Domain.Shared.FormFilters;

namespace Domain.Services.Users
{
    public class UserFilterDto : SearchFilterDto
    {
        public string UserName { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public string RoleId { get; set; }

        public string Password { get; set; }

        public string CarrierId { get; set; }

        public string IsActive { get; set; }

        public string CompanyId { get; set; }
    }
}
