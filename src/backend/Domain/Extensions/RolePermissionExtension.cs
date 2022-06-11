using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    public static class RolePermissionExtension
    {
        public const string ClaimType = "Permission";

        public static string GetPermissionName(this RolePermissions permission)
        {
            return $"{ClaimType}.{permission}";
        }
    }
}
