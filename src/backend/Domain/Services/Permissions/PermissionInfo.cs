using Domain.Enums;
using Domain.Services.Translations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services.Permissions
{
    public class PermissionInfo
    {
        public RolePermissions Code { get; set; }

        public string Name { get; set; }
    }
}
