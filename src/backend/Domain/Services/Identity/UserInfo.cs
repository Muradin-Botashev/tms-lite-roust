using Domain.Services.Permissions;
using Domain.Services.Roles;
using System.Collections.Generic;

namespace Domain.Services.Identity
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string UserRole { get; set; }

        public RoleDto Role { get; set; }
    }
}