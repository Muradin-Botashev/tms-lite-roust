using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Profile;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;

        private readonly IValidationService _validationService;

        public ProfileService(IUserProvider userProvider, ICommonDataService dataService, IValidationService validationService)
        {
            this._userProvider = userProvider;
            this._dataService = dataService;
            this._validationService = validationService;
        }
        public ProfileDto GetProfile()
        {
            var lang = _userProvider.GetCurrentUser().Language;
            var currentUserId = _userProvider.GetCurrentUserId();

            var user = _dataService
                .GetDbSet<User>()
                .GetById(currentUserId.Value);

            var role = _dataService
                .GetDbSet<Role>()
                .GetById(user.RoleId);

            var notifications = user.Notifications?
                                    .Select(x => ((NotificationType)x).GetEnumLookup(lang))?
                                    .ToArray();

            return new ProfileDto
            {
                Email = user.Email,
                UserName = user.Name,
                RoleName = role.Name,
                Notifications = notifications
            };
        }

        public ValidateResult Save(SaveProfileDto dto)
        {
            var currentUserId = _userProvider.GetCurrentUserId();
            var user = _dataService.GetDbSet<User>().GetById(currentUserId.Value);
            
            var lang = _userProvider.GetCurrentUser().Language;

            var result = this._validationService.Validate(dto);

            if (string.IsNullOrEmpty(dto.Email))
            {
                result.AddError(nameof(dto.Email), "userEmailIsEmpty".Translate(lang), ValidationErrorType.ValueIsRequired);
            }
            else
            {
                var emailRegExp = new Regex(@"^(("")("".+?(?<!\\)""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9][\-a-zA-Z0-9]{0,22}[a-zA-Z0-9]))$");
                if (!emailRegExp.IsMatch(dto.Email))
                {
                    result.AddError(nameof(dto.Email), "User.Email.IncorrectFormat".Translate(lang), ValidationErrorType.InvalidValueFormat);
                }
            }

            if (string.IsNullOrEmpty(dto.UserName))
                result.AddError(nameof(dto.UserName), "userNameIsEmpty".Translate(lang), ValidationErrorType.ValueIsRequired);

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (user.PasswordHash != dto.OldPassword.GetHash())
                {
                    result.AddError(nameof(dto.OldPassword), "wrongOldPassword".Translate(lang), ValidationErrorType.ValueIsRequired);
                }
                else
                {
                    user.PasswordHash = dto.NewPassword.GetHash();
                }
            }

            var notifications = dto.Notifications?
                                   .Select(x => x?.Value.ToEnum<NotificationType>())
                                   .Where(x => x != null)
                                   .Select(x => (int)x.Value)
                                   .ToArray();

            if (!result.IsError)
            {
                user.Email = dto.Email;
                user.Name = dto.UserName;
                user.Notifications = notifications;


                _dataService.SaveChanges();
            }
            
            return result;
        }

        public IEnumerable<LookUpDto> GetAllNotifications()
        {
            var lang = _userProvider.GetCurrentUser().Language;
            var values = Domain.Extensions.Extensions.GetOrderedEnum<NotificationType>();
            var result = values.Select(x => x.GetEnumLookup(lang))
                               .OrderBy(x => x.Name)
                               .ToList();
            return result;
        }
    }
}