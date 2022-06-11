using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using DAL.Queries;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Identity;
using Domain.Services.Translations;
using Domain.Services.Users;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Services.Users
{
    public class UsersService : DictoinaryServiceBase<User, UserDto, UserFilterDto>, IUsersService
    {
        private readonly IIdentityService _identityService;

        public UsersService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<UserDto, User>> validationRules,
            IIdentityService identityService) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules) 
        {
            _identityService = identityService;
        }

        public ValidateResult SetActive(Guid id, bool active)
        {
            var user = _userProvider.GetCurrentUser();
            var entity = _dataService.GetDbSet<User>().GetById(id);
            if (entity == null)
            {
                return new ValidateResult("userNotFoundEntity".Translate(user.Language));
            }

            if (user.CompanyId != null && entity.CompanyId != user.CompanyId)
            {
                return new ValidateResult("setActiveNotAllowed".Translate(user.Language));
            }

            entity.IsActive = active;
            _dataService.SaveChanges();

            return new ValidateResult(entity.Id);
        }

        public OpenTokenResponseDto CreateOpenToken(Guid id)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var user = _dataService.GetDbSet<User>()
                                   .Include(x => x.Role)
                                   .Where(x => x.Id == id && x.IsActive)
                                   .FirstOrDefault();

            if (user == null)
            {
                return new OpenTokenResponseDto(null, "userNotFoundEntity".Translate(lang), true);
            }

            var identity = _identityService.GenerateIdentityForUser(user, user?.Role, "ru", ApiExtensions.ApiLevel.Open);
            var token = _identityService.GenerateJwtToken(identity.Claims, TimeSpan.FromDays(3650));

            return new OpenTokenResponseDto(token, null, false);
        }

        protected override IQueryable<User> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Role)
                .Include(i => i.Company)
                .Include(i => i.Carrier);
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<User>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(x => x.IsActive)
                                .OrderBy(x => x.Name)
                                .ToList();

            foreach (var entity in entities)
            {
                yield return new LookUpDto
                {
                    Name = entity.Name,
                    Value = entity.Id.FormatGuid()
                };
            }
        }

        public override UserDto MapFromEntityToDto(User entity)
        {
            return new UserDto
            {
                Login = entity.Login,
                Email = entity.Email,
                Id = entity.Id.FormatGuid(),
                UserName = entity.Name,
                Role = entity.Role != null ? entity.Role.ToString() : null,
                RoleId = entity.Role != null ? new LookUpDto(entity.RoleId.FormatGuid(), entity.Role.ToString()) : null,
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company?.ToString()),
                FieldsConfig = entity.FieldsConfig,
                IsActive = entity.IsActive,
                CarrierId = entity.CarrierId == null ? null : new LookUpDto(entity.CarrierId.FormatGuid(), entity.Carrier?.ToString())
            };
        }

        public override DetailedValidationResult MapFromDtoToEntity(User entity, UserDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id)) 
                entity.Id = Guid.Parse(dto.Id);

            var oldRoleId = entity.RoleId;

            entity.Login = dto.Login;
            entity.Email = dto.Email;
            entity.Name = dto.UserName;
            entity.RoleId = Guid.Parse(dto.RoleId?.Value);
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.FieldsConfig = dto.FieldsConfig;
            entity.IsActive = dto.IsActive;
            entity.CarrierId = dto.CarrierId?.Value?.ToGuid();

            // Запрет на попытку сделать глобального пользователя из-под локального пользователя
            if (dto.CompanyId == null)
            {
                var currentUser = _userProvider.GetCurrentUser();
                entity.CompanyId = currentUser.CompanyId;
            }
            
            if (!string.IsNullOrEmpty(dto.Password)) 
                entity.PasswordHash = dto.Password.GetHash();

            return null;
        }

        protected override DetailedValidationResult ValidateDto(UserDto dto, User entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            if (string.IsNullOrEmpty(dto.Id) && string.IsNullOrEmpty(dto.Password))
            {
                result.AddError(nameof(dto.Password), "User.Password.ValueIsRequired".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            var emailRegExp = new Regex(@"^(("")("".+?(?<!\\)""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9][\-a-zA-Z0-9]{0,22}[a-zA-Z0-9]))$");
            if (!string.IsNullOrEmpty(dto.Email) && !emailRegExp.IsMatch(dto.Email))
            {
                result.AddError(nameof(dto.Email), "User.Email.IncorrectFormat".Translate(lang), ValidationErrorType.InvalidValueFormat);
            }

            var companyId = dto.CompanyId?.Value.ToGuid();

            var roleId = dto.RoleId?.Value.ToGuid();
            if (roleId != null)
            {
                var role = _dataService.GetById<Role>(roleId.Value);
                if (role != null && role.CompanyId != companyId)
                {
                    result.AddError(nameof(dto.RoleId), "User.RoleId.WrongCompany".Translate(lang), ValidationErrorType.InvalidValueFormat);
                }
            }

            var carrierId = dto.CarrierId?.Value.ToGuid();
            if (carrierId != null)
            {
                var carrier = _dataService.GetById<TransportCompany>(carrierId.Value);
                if (carrier?.CompanyId != null && carrier.CompanyId != companyId)
                {
                    result.AddError(nameof(dto.CarrierId), "User.CarrierId.WrongCompany".Translate(lang), ValidationErrorType.InvalidValueFormat);
                }
            }

            var hasDuplicates = this._dataService.GetDbSet<User>()
                .Where(i => i.Id != dto.Id.ToGuid())
                .Where(i => i.Login == dto.Login)
                .Any();

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Login), "User.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        protected override IQueryable<User> ApplySort(IQueryable<User> query, FilterFormDto<UserFilterDto> form)
        {
            var sortFieldMapping = new Dictionary<string, string>
            {
                { "userName", "name" }
            };

            if (!string.IsNullOrEmpty(form.Sort?.Name) && sortFieldMapping.ContainsKey(form.Sort?.Name))
            {
                form.Sort.Name = sortFieldMapping[form.Sort?.Name];
            }

            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Login, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<User> ApplySearch(IQueryable<User> query, FilterFormDto<UserFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Login.ApplyStringFilter<User>(i => i.Login, ref parameters))
                         .WhereAnd(form.Filter.Email.ApplyStringFilter<User>(i => i.Email, ref parameters))
                         .WhereAnd(form.Filter.UserName.ApplyStringFilter<User>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.CarrierId.ApplyOptionsFilter<User, Guid?>(i => i.CarrierId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.RoleId.ApplyOptionsFilter<User, Guid>(i => i.RoleId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<User, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<User>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""Users"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query
                    .Include(i => i.Role)
                    .Include(i => i.Carrier)
                    .Where(i =>
                    i.Name.ToLower().Contains(search)
                    || i.Login.ToLower().Contains(search)
                    || i.Email.ToLower().Contains(search)
                    || (i.Role != null && i.Role.Name.ToLower().Contains(search))
                    || (i.Carrier != null && i.Carrier.Title.ToLower().Contains(search))
                );
            }

            return query;
        }

        public override User FindByKey(UserDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<User>()
                               .FirstOrDefault(i => i.Login == dto.Login && i.CompanyId == companyId);
        }

        public override IEnumerable<User> FindByKey(IEnumerable<UserDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(User entity)
        {
            return entity.Login;
        }

        public override string GetDtoKey(UserDto dto)
        {
            return dto.Login;
        }

        public override UserDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new UserDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override IQueryable<User> ApplyRestrictions(IQueryable<User> query)
        {
            query = base.ApplyRestrictions(query);

            var userCompanyId = _userProvider.GetCurrentUser()?.CompanyId;
            if (userCompanyId != null)
            {
                query = query.Where(x => x.CompanyId == userCompanyId);
            }

            return query;
        }

        protected override ExcelMapper<UserDto> CreateExcelMapper()
        {
            return new ExcelMapper<UserDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}