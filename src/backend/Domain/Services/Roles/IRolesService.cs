using Domain.Persistables;
using Domain.Services.Permissions;
using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.Roles
{
    public interface IRolesService : IDictonaryService<Role, RoleDto, RoleFilterDto>
    {
        ValidateResult SetActive(Guid id, bool active);

        IEnumerable<PermissionInfo> GetAllPermissions();

        IEnumerable<LookUpDto> GetAllBacklights();

        RoleActionsDto GetAllActions();

        RoleDto MapFromEntityToDto(Role entity);
    }
}