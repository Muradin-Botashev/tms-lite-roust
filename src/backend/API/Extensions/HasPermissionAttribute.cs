using Domain.Enums;
using Domain.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace API.Extensions
{
    public class HasPermissionAttribute: AuthorizeAttribute
    {
        public HasPermissionAttribute(RolePermissions permission) : base(permission.GetPermissionName())
        { 
        }
    }
}
