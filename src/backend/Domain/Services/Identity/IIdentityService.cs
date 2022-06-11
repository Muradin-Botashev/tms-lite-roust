using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Domain.Services.Identity
{
    public interface IIdentityService : IService
    {
        VerificationResultWith<TokenModel> GetToken(IdentityModel model);
        UserInfo GetUserInfo();
        ValidateResult ChangeMasterPassword(string oldPassword, string newPassword);

        ClaimsIdentity GenerateIdentityForUser(User user, Role role, string language, ApiExtensions.ApiLevel apiLevel);
        string GenerateJwtToken(IEnumerable<Claim> claims, TimeSpan lifetime);

        bool HasPermissions(User user, RolePermissions permission);

        bool HasPermissions(RolePermissions permission);
    }
}