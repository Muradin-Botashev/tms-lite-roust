using Application.BusinessModels.Shared.Actions;
using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Permissions;
using Domain.Services.Roles;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Application.Services.Roles
{
    public class RolesService : DictoinaryServiceBase<Role, RoleDto, RoleFilterDto>, IRolesService
    {
        public RolesService(
            ICommonDataService dataService,
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<RoleDto, Role>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<Role> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public ValidateResult SetActive(Guid id, bool active)
        {
            var user = _userProvider.GetCurrentUser();
            var entity = _dataService.GetDbSet<Role>().GetById(id);
            if (entity == null)
            {
                return new ValidateResult("roleNotFound".Translate(user.Language));
            }

            if (user.CompanyId != null && entity.CompanyId != user.CompanyId)
            {
                return new ValidateResult("setActiveNotAllowed".Translate(user.Language));
            }

            entity.IsActive = active;

            if (!entity.IsActive)
            {
                _dataService.GetDbSet<User>()
                    .Where(i => i.RoleId == entity.Id)
                    .ToList()
                    .ForEach(i => i.IsActive = entity.IsActive);
            }

            _dataService.SaveChanges();

            return new ValidateResult(entity.Id);
        }

        protected override IQueryable<Role> ApplySort(IQueryable<Role> query, FilterFormDto<RoleFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<Role> ApplySearch(IQueryable<Role> query, FilterFormDto<RoleFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<Role>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<Role, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<Role>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""Roles"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search));
            }

            return query;
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<Role>().AsQueryable();
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

        public override IEnumerable<LookUpDto> ForSelect(Guid? companyId)
        {
            companyId = companyId ?? _userProvider.GetCurrentUser()?.CompanyId;
            var entities = _dataService.GetDbSet<Role>()
                                       .Where(x => x.IsActive && x.CompanyId == companyId)
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

        public override DetailedValidationResult MapFromDtoToEntity(Role entity, RoleDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
            {
                entity.Id = Guid.Parse(dto.Id);
                entity.IsActive = dto.IsActive;
            }
            else
            {
                entity.IsActive = true;
            }
            
            entity.Name = dto.Name;
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.Actions = dto.Actions?.Select(x => x.Value)?.ToArray();

            entity.Permissions = dto?.Permissions?.Select(i => i.Code)?
                                                  .Cast<int>()?
                                                  .ToArray();

            entity.Backlights = dto?.Backlights?.Select(x => x.Value.ToEnum<BacklightType>())?
                                                .Where(x => x.HasValue)?
                                                .Select(x => (int)x.Value)?
                                                .ToArray();

            return null;
        }

        protected override DetailedValidationResult ValidateDto(RoleDto dto, Role entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && this._dataService.GetDbSet<Role>()
                .Where(i => i.Id != dto.Id.ToGuid())
                .Where(i => i.Name == dto.Name)
                .Where(i => i.CompanyId == null || currentCompanyId == null || i.CompanyId == currentCompanyId)
                .Any();

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "Role.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override RoleDto MapFromEntityToDto(Role entity)
        {
            string lang = _userProvider.GetCurrentUser()?.Language;

            return new RoleDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                IsActive = entity.IsActive,
                Actions = entity.Actions?.Select(x => new LookUpDto(x))?.ToArray(),
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                Permissions = entity?.Permissions?.Cast<RolePermissions>()?.Select(i => new PermissionInfo
                {
                    Code = i,
                    Name = i.GetPermissionName()
                }),
                Backlights = entity?.Backlights?.Cast<BacklightType>()?.Select(x => x.GetEnumLookup(lang)),
                UsersCount = _dataService.GetDbSet<User>().Where(i => i.RoleId == entity.Id).Count()
            };
        }

        public IEnumerable<PermissionInfo> GetAllPermissions()
        {
            return Domain.Extensions.Extensions.GetOrderedEnum<RolePermissions>()
                .Where(x => x != RolePermissions.None)
                .Select(i => new PermissionInfo
                {
                    Code = i,
                    Name = i.GetPermissionName()
                });
        }

        public RoleActionsDto GetAllActions()
        {
            var result = new RoleActionsDto
            {
                OrderActions = GetActions<Order>(),
                ShippingActions = GetActions<Shipping>()
            };
            return result;
        }

        public IEnumerable<LookUpDto> GetAllBacklights()
        {
            string lang = _userProvider.GetCurrentUser()?.Language;

            return Domain.Extensions.Extensions.GetOrderedEnum<BacklightType>()
                .Select(i => i.GetEnumLookup(lang))
                .ToList();
        }

        private IEnumerable<LookUpDto> GetActions<TEntity>()
        {
            var actionSingleType = typeof(IAction<TEntity>);
            var actionGroupType = typeof(IAction<IEnumerable<TEntity>>);
            var actions = AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .SelectMany(s => s.GetTypes())
                                   .Where(p => actionSingleType.IsAssignableFrom(p) || actionGroupType.IsAssignableFrom(p));
            return actions.Select(GetActionDto).ToList();
        }

        private LookUpDto GetActionDto(MemberInfo action)
        {
            string value = action.Name.ToLowerFirstLetter();
            string name = value;

            var descAttr = action.GetCustomAttribute<DescriptionKeyAttribute>();
            if (descAttr != null)
            {
                name = descAttr.Key;
            }
            
            return new LookUpDto
            {
                Value = value,
                Name = name
            };
        }

        public override Role FindByKey(RoleDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<Role>()
                               .FirstOrDefault(i => i.Name == dto.Name && i.CompanyId == companyId);
        }

        public override IEnumerable<Role> FindByKey(IEnumerable<RoleDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(Role entity)
        {
            return entity.Name + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(RoleDto dto)
        {
            return dto.Name + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override RoleDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new RoleDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override ExcelMapper<RoleDto> CreateExcelMapper()
        {
            return new ExcelMapper<RoleDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}