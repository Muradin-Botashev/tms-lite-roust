using Domain.Enums;
using System;

namespace API.Extensions
{
    public class GridPermissionsAttribute: Attribute
    {
        public RolePermissions Search { get; set; } = RolePermissions.None;

        public RolePermissions SaveOrCreate { get; set; } = RolePermissions.None;
    }
}
