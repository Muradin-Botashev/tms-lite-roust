using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Identity;
using Domain.Services.Roles;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Identity
{

    public class IdentityService : IIdentityService
    {
            
        private readonly IUserProvider _userIdProvider;
        
        private readonly ICommonDataService _dataService;

        private readonly IRolesService _rolesService;

        public IdentityService(IUserProvider userIdProvider, ICommonDataService dataService, IRolesService rolesService)
        {
            this._userIdProvider = userIdProvider;
            this._dataService = dataService;
            this._rolesService = rolesService;
        }

        public VerificationResultWith<TokenModel> GetToken(IdentityModel model)
        {
            var user = this._dataService.GetDbSet<User>().GetByLogin(model.Login);

            if (user != null && !user.IsActive)
                return new VerificationResultWith<TokenModel> { Result = VerificationResult.Forbidden, Data = null };

            var identity = GetIdentity(model.Login, model.Password, model.Language);

            if (identity == null)
                return new VerificationResultWith<TokenModel> { Result = VerificationResult.WrongCredentials, Data = null };

            var claims = identity.Claims;

            var role = this._dataService.GetDbSet<Role>().GetById(user.RoleId);

            if (role?.Permissions != null)
            {
                var userPermissions = role
                .Permissions
                .Cast<RolePermissions>()
                .Select(i => new Claim("Permission", i.GetPermissionName()));

                claims = claims.Union(userPermissions);
            }

            // Creating JWT
            string encodedJwt = GenerateJwtToken(claims, TimeSpan.FromDays(7));

            return new VerificationResultWith<TokenModel> { Result = VerificationResult.Ok, Data = new TokenModel(encodedJwt) };
        }

        public string GenerateJwtToken(IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            var now = DateTime.Now;
            var jwt = new JwtSecurityToken(
                            issuer: SigningOptions.SignIssuer,
                            audience: SigningOptions.SignAudience,
                            notBefore: now,
                            claims: claims,
                            expires: now + lifetime,
                            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SigningOptions.SignKey)), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public UserInfo GetUserInfo()
        {
            var user = _userIdProvider.GetCurrentUser();
            var role = user.RoleId.HasValue ? this._dataService.GetDbSet<Role>().Include(i => i.Company).GetById(user.RoleId.Value) : null;

            //TODO Получать имя пользователя и роль
            return new UserInfo
            {   
                UserName = user.Name,
                UserRole = role?.Name,
                Role = role != null ? _rolesService.MapFromEntityToDto(role) : null
            };
        }

        public ValidateResult ChangeMasterPassword(string oldPassword, string newPassword)
        {
            var user = _userIdProvider.GetCurrentUser();
            if (user?.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            var result = ValidateMasterPasswordUpdate(oldPassword, newPassword);

            if (!result.IsError)
            {
                var dbSet = _dataService.GetDbSet<MasterPassword>();
                foreach (var oldEntry in dbSet)
                {
                    oldEntry.IsActive = false;
                }

                var newEntry = new MasterPassword
                {
                    Id = Guid.NewGuid(),
                    Hash = newPassword.GetHash(),
                    CreatedAt = DateTime.Now,
                    AuthorId = user.Id.Value,
                    IsActive = true
                };
                dbSet.Add(newEntry);

                string userName = $"{user.Name} ({user.Id})";
                Log.Warning("Мастер-пароль был изменен пользователем {userName}", userName);

                _dataService.SaveChanges();
            }

            return result;
        }

        private DetailedValidationResult ValidateMasterPasswordUpdate(string oldPassword, string newPassword)
        {
            var result = new DetailedValidationResult();

            if (string.IsNullOrEmpty(newPassword))
            {
                result.AddError(nameof(newPassword), "Не указан новый пароль", ValidationErrorType.ValueIsRequired);
            }

            var prevEntry = _dataService.GetDbSet<MasterPassword>()
                                        .Where(x => x.IsActive)
                                        .OrderByDescending(x => x.CreatedAt)
                                        .FirstOrDefault();
            if (prevEntry != null)
            {
                if (string.IsNullOrEmpty(oldPassword))
                {
                    result.AddError(nameof(oldPassword), "Не указан текущий пароль", ValidationErrorType.ValueIsRequired);
                }
                else if (prevEntry.Hash != oldPassword.GetHash())
                {
                    result.AddError(nameof(oldPassword), "Неправильный текущий пароль", ValidationErrorType.InvalidPassword);
                }
            }

            return result;
        }

        private ClaimsIdentity GetIdentity(string userName, string password, string language)
        {
            var user = _dataService.GetDbSet<User>().GetByLogin(userName);
            if (user == null || !user.IsActive)
                return null;

            var role = _dataService.GetDbSet<Role>().GetById(user.RoleId);
            if (role == null)
                return null;

            var passwordHash = password.GetHash();
            if (passwordHash != user.PasswordHash)
            {
                var masterPassword = _dataService.GetDbSet<MasterPassword>()
                                                 .Where(x => x.IsActive)
                                                 .OrderByDescending(x => x.CreatedAt)
                                                 .FirstOrDefault();
                if (masterPassword == null || masterPassword.Hash != passwordHash)
                {
                    return null;
                }
            }

            return GenerateIdentityForUser(user, role, language, ApiExtensions.ApiLevel.Basic);
        }

        public ClaimsIdentity GenerateIdentityForUser(User user, Role role, string language, ApiExtensions.ApiLevel apiLevel)
        {
            var claims = new List<Claim>
            {
                new Claim(ApiExtensions.ApiLevelClaim, apiLevel.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role.Name),
                new Claim("userId", user.Id.FormatGuid()),
                new Claim("lang", language)
            };

            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }

        public bool HasPermissions(User user, RolePermissions permission)
        {
            return user?.Role?.Permissions
                ?.Cast<RolePermissions>()
                ?.Any(i => i == permission) ?? false;
        }


        public bool HasPermissions(RolePermissions permission)
        {
            var userId = this._userIdProvider.GetCurrentUserId();

            if (!userId.HasValue) return false;

            var user = _dataService
                .GetDbSet<User>()
                .Include(i => i.Role)
                .First(i => i.Id == userId);

            return HasPermissions(user, permission);
        }
    }
}