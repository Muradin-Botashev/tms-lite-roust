using DAL;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Domain.Services.UserSettings;
using Domain.Shared;
using System;
using System.Linq;

namespace Application.Services.UserSettings
{
    public class UserSettingsService : IUserSettingsService
    {
        public UserSettingDto GetValue(string key)
        {
            var userId = _userProvider.GetCurrentUserId();
            var entity = _db.UserSettings.Where(x => x.UserId == userId && x.Key == key).FirstOrDefault();
            return new UserSettingDto
            {
                Key = key,
                Value = entity?.Value
            };
        }

        public ValidateResult SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new ValidateResult("notFound");
            }

            var userId = _userProvider.GetCurrentUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException();
            }

            var entities = _db.UserSettings.Where(x => x.UserId == userId && x.Key == key).ToList();
            var entity = entities.FirstOrDefault();

            if (entity != null)
            {
                entity.Value = value;
                if (entities.Count > 1)
                {
                    _db.UserSettings.RemoveRange(entities.Skip(1));
                }
            }
            else
            {
                entity = new UserSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    Key = key,
                    Value = value
                };
                _db.UserSettings.Add(entity);
            }

            _db.SaveChanges();

            return new ValidateResult(entity.Id);
        }

        public UserSettingsService(AppDbContext db, IUserProvider userProvider)
        {
            _db = db;
            _userProvider = userProvider;
        }

        private readonly AppDbContext _db;
        private readonly IUserProvider _userProvider;
    }
}
