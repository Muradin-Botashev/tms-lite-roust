using Domain.Enums;
using Domain.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Linq;

namespace API.Extensions
{
    public class AuthorizeByDefaultConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (ShouldApplyConvention(action))
            {
                var attr = (GridPermissionsAttribute)action.Controller.Attributes.First(x => x.GetType() == typeof(GridPermissionsAttribute));

                var actionPermission = GetActionPermissions(attr, action.ActionName);

                if (actionPermission.HasValue)
                {
                    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
                    policy = policy.RequireClaim("Permission", actionPermission.Value.GetPermissionName());
                    action.Filters.Add(new AuthorizeFilter(policy.Build()));
                }
            }
        }

        private bool ShouldApplyConvention(ActionModel action)
        {
            return action.Controller.Attributes.Any(x => x.GetType() == typeof(GridPermissionsAttribute));
        }

        private RolePermissions? GetActionPermissions(GridPermissionsAttribute gridPermissions, string action)
        {
            switch(action)
            {
                case "SaveOrCreate": return gridPermissions.SaveOrCreate;
                case "Search": return gridPermissions.Search;
                default: return null;
            }
        }
    }
}
