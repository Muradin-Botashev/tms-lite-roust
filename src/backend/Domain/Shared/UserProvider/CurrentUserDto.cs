using System;

namespace Domain.Shared.UserProvider
{
    public class CurrentUserDto
    {
        public Guid? Id { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? CompanyId { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
