using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace API.Extensions
{
    public static class BasicPrincipalValidation
    {
        public static Task Validate(ValidatePrincipalContext context)
        {
            var dataService = context.HttpContext.RequestServices.GetService<ICommonDataService>();
            var identityService = context.HttpContext.RequestServices.GetService<IIdentityService>();

            var passwordHash = context.Password.GetHash();
            var user = dataService.GetDbSet<User>()
                                  .FirstOrDefault(x => x.Login == context.UserName
                                                    && x.PasswordHash == passwordHash
                                                    && x.IsActive);

            var role = user?.RoleId == null ? null : dataService.GetById<Role>(user.RoleId);

            if (user != null && role != null)
            {
                var identity = identityService.GenerateIdentityForUser(user, role, "ru", ApiExtensions.ApiLevel.Open);
                context.Principal = new ClaimsPrincipal(identity);
            }

            return Task.CompletedTask;
        }
    }
}
