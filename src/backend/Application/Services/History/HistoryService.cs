using DAL;
using DAL.Queries;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Application.Services.History
{
    public class HistoryService : IHistoryService
    {
        public HistoryDto Get(Guid entityId)
        {
            var entries = _dataService.GetDbSet<HistoryEntry>()
                .Where(e => e.PersistableId == entityId)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            var user = _userProvider.GetCurrentUser();
            var dtos = entries.Select(e => ConvertEntityToDto(e, user.Language)).ToList();

            return new HistoryDto
            {
                Entries = dtos
            };
        }

        public void Save(Guid entityId, string messageKey, params object[] messageArgs)
        {
            var user = _userProvider.GetCurrentUser();
            SaveInner(user, entityId, messageKey, messageArgs);
        }

        public void SaveImpersonated(CurrentUserDto user, Guid entityId, string messageKey, params object[] messageArgs)
        {
            SaveInner(user, entityId, messageKey, messageArgs);
        }

        private void SaveInner(CurrentUserDto user, Guid entityId, string messageKey, params object[] messageArgs)
        {
            string userName = GetUserName(user);
            string[] valueArgs = messageArgs.Select(GetDisplayValue).ToArray();
            string strArgs = JsonConvert.SerializeObject(valueArgs);
            HistoryEntry entry = new HistoryEntry
            {
                PersistableId = entityId,
                UserId = user?.Id,
                UserName = userName,
                CreatedAt = DateTime.UtcNow,
                MessageKey = messageKey,
                MessageArgs = strArgs
            };
            _dataService.GetDbSet<HistoryEntry>().Add(entry);
        }

        private HistoryEntryDto ConvertEntityToDto(HistoryEntry entity, string lang)
        {
            string[] args = JsonConvert.DeserializeObject<string[]>(entity.MessageArgs ?? string.Empty);
            string[] localArgs = args.Select(x => x.Translate(lang)).ToArray();

            string message = entity.MessageKey.Translate(lang, localArgs);

            return new HistoryEntryDto
            {
                UserName = entity.UserName,
                CreatedAt = entity.CreatedAt,
                Message = message
            };
        }

        private string GetDisplayValue(object value)
        {
            if (value == null || (value is string && (value as string).Length == 0))
            {
                return "emptyValue";
            }
            else if (value is DateTime)
            {
                return ((DateTime)value).FormatDateTime();
            }
            else if (value is Enum)
            {
                return Enum.GetName(value.GetType(), value).ToLowerFirstLetter();
            }
            else
            {
                return value?.ToString();
            }
        }

        private string GetUserName(CurrentUserDto user)
        {
            if (user == null)
            {
                return "System";
            }
            else
            {
                Role role = user.RoleId.HasValue ? _dataService.GetById<Role>(user.RoleId.Value) : null;
                if (role == null)
                {
                    return user.Name;
                }
                else
                {
                    return $"{user.Name} ({role.Name})";
                }
            }
        }

        public HistoryService(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
    }
}
