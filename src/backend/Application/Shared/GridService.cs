using Application.BusinessModels.Shared.Actions;
using Application.BusinessModels.Shared.Backlights;
using Application.Extensions;
using Application.Shared.Triggers;
using Application.Shared.Excel;
using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Application.BusinessModels.Shared.Validation;

namespace Application.Shared
{
    public abstract class GridService<TEntity, TDto, TFormDto, TSummaryDto, TFilter>: IGridService<TEntity, TDto, TFormDto, TSummaryDto, TFilter>
        where TEntity : class, IPersistable, new() 
        where TDto : IListDto, new() 
        where TFormDto : TDto, IValidatedDto, new()
        where TFilter: SearchFilterDto, new()
        where TSummaryDto : new()
    {
        Dictionary<string, Func<TEntity, bool>> _rules = new Dictionary<string, Func<TEntity, bool>>();

        public abstract void MapFromDtoToEntity(TEntity entity, TDto dto);
        public abstract void MapFromFormDtoToEntity(TEntity entity, TFormDto dto);
        public abstract TDto MapFromEntityToDto(TEntity entity, Role role);
        public abstract TFormDto MapFromEntityToFormDto(TEntity entity);
        public abstract LookUpDto MapFromEntityToLookupDto(TEntity entity);

        public abstract IEnumerable<EntityStatusDto<TEntity>> LoadStatusData(IEnumerable<Guid> ids);

        public abstract IQueryable<TEntity> ApplySearchForm(IQueryable<TEntity> query, FilterFormDto<TFilter> searchForm, List<string> columns = null);

        public virtual TSummaryDto GetSummary(IEnumerable<Guid> ids) { return new TSummaryDto(); }

        protected virtual void OnGetForm(TEntity entity, Role role) { }

        protected virtual void ConfigureFieldAccessRules(Dictionary<string, Func<TEntity, bool>> rules) { }

        protected readonly IUserProvider _userIdProvider;
        protected readonly ICommonDataService _dataService;
        protected readonly IFieldDispatcherService _fieldDispatcherService;
        protected readonly IFieldPropertiesService _fieldPropertiesService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ITriggersService _triggersService;
        protected readonly IValidationService _validationService;
        protected readonly IEnumerable<IValidationRule<TDto, TEntity>> _validationRules;

        protected GridService(
            ICommonDataService dataService, 
            IUserProvider userIdProvider,
            IFieldDispatcherService fieldDispatcherService,
            IFieldPropertiesService fieldPropertiesService,
            IServiceProvider serviceProvider,
            ITriggersService triggersService, 
            IValidationService validationService,
            IEnumerable<IValidationRule<TDto, TEntity>> validationRules)
        {
            _userIdProvider = userIdProvider;
            _dataService = dataService;
            _fieldDispatcherService = fieldDispatcherService;
            _fieldPropertiesService = fieldPropertiesService;
            _serviceProvider = serviceProvider;
            _triggersService = triggersService;
            _validationService = validationService;
            _validationRules = validationRules;

            ConfigureFieldAccessRules(_rules);
        }

        protected virtual IQueryable<TEntity> GetDbSet()
        {
            return _dataService.GetDbSet<TEntity>();
        }

        public TDto Get(Guid id)
        {
            var entity = GetDbSet().GetById(id);

            var currentUser = _userIdProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyPersistable).IsAssignableFrom(typeof(TEntity)))
            {
                var companyEntity = (ICompanyPersistable)entity;
                var entityCompanyId = companyEntity?.CompanyId;
                if (entityCompanyId != null && entityCompanyId != currentUser.CompanyId)
                {
                    throw new AccessDeniedException(currentUser?.Language);
                }
            }

            var result = MapFromEntityToDto(entity, GetRole());
            result = FillLookupNames(result);
            return result;
        }

        public TFormDto GetForm(Guid id)
        {
            var entity = GetDbSet().GetById(id);

            var currentUser = _userIdProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyPersistable).IsAssignableFrom(typeof(TEntity)))
            {
                var companyEntity = (ICompanyPersistable)entity;
                var entityCompanyId = companyEntity?.CompanyId;
                if (entityCompanyId != null && entityCompanyId != currentUser.CompanyId)
                {
                    throw new AccessDeniedException(currentUser?.Language);
                }
            }

            OnGetForm(entity, GetRole());
            
            var result = MapFromEntityToFormDto(entity);

            result = (TFormDto)FillLookupNames(result);
            result.ValidationResult = ValidateDto(result, entity);

            return result;
        }
        
        public IEnumerable<LookUpDto> ForSelect()
        {
            var entries = GetDbSet().ToList();
            var result = entries.Select(MapFromEntityToLookupDto).ToList();
            return result;
        }

        public IEnumerable<LookUpDto> ForSelect(string fieldName, FilterFormDto<TFilter> form)
        {
            foreach (var prop in form.Filter.GetType().GetProperties())
            {
                if (string.Equals(prop.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    prop.SetValue(form.Filter, null);
                }
            }

            var user = _userIdProvider.GetCurrentUser();

            var dbSet = _dataService.GetDbSet<TEntity>();

            var query = ApplySearchForm(dbSet, form);
            query = ApplyRestrictions(query);

            var propertyInfo = typeof(TEntity).GetProperties()
                .FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());
            var refType = GetReferenceType(propertyInfo);

            var fields = _fieldDispatcherService.GetDtoFields<TDto>();
            var field = fields.FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());

            IEnumerable<LookUpDto> result;

            if (refType != null)
            {
                result = GetReferencedValues(query, refType, fieldName);
            }
            else if (field.FieldType == FieldType.State)
            {
                result = GetStateValues(query, propertyInfo);
            }
            else
            {
                result = GetSelectValues(query, propertyInfo, field.ShowRawReferenceValue);
            }

            if (field.EmptyValueOptions != EmptyValueOptions.NotAllowed)
            {
                var empty = new LookUpDto
                {
                    Name = "emptyValue".Translate(user.Language),
                    Value = LookUpDto.EmptyValue,
                    IsFilterOnly = field.EmptyValueOptions == EmptyValueOptions.FilterOnly
                };

                result = new[] { empty }.Union(result);
            }

            return result;

        }

        List<LookUpDto> GetReferencedValues(IQueryable<TEntity> query, Type refType, string field)
        {
            var ids = query.SelectField(field).Distinct();

            var result = _dataService.QueryAs<IPersistable>(refType)
                .Where(i => ids.Contains(i.Id))
                .ToList();

            return result.Select(i => new LookUpDto
            {
                Name = i.ToString(),
                Value = i.Id.FormatGuid()
            })
            .ToList();
        }

        List<ColoredLookUpDto> GetStateValues(IQueryable<TEntity> query, PropertyInfo propertyInfo)
        {
            var lang = _userIdProvider.GetCurrentUser()?.Language;

             var getMethod = typeof(Domain.Extensions.Extensions)
                .GetMethod(nameof(Domain.Extensions.Extensions.GetColor))
                .MakeGenericMethod(propertyInfo.PropertyType);

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct()
                .Select(i => new ColoredLookUpDto
                 {
                     Name = i.FormatEnum(),
                     Value = i.FormatEnum(),
                     Color = getMethod.Invoke(i, new object[] { i }).FormatEnum()
                })
                .ToList();

            return result;
        }

        List<LookUpDto> GetSelectValues(IQueryable<TEntity> query, PropertyInfo propertyInfo, bool showRawName)
        {
            var lang = _userIdProvider.GetCurrentUser()?.Language;

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct();

            return result.Select(i => new LookUpDto
            {
                Name = showRawName ? i.ToString() : i.FormatEnum().Translate(lang),
                Value = i.ToString()
            })
            .ToList();
        }

        //TODO: move to shared code
        private Type GetReferenceType(PropertyInfo property)
        {
            var attr = property.GetCustomAttribute<ReferenceTypeAttribute>();
            return attr?.Type;
        }

        protected virtual IQueryable<TEntity> ApplySort(IQueryable<TEntity> query, FilterFormDto<TFilter> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Id, true);
        }

        public SearchResult<TDto> Search(FilterFormDto<TFilter> form)
        {
            var dbSet = GetDbSet();

            var query = ApplySearchForm(dbSet, form);
            query = ApplyRestrictions(query);
            
            if (form.Take == 0)
                form.Take = 1000;

            var totalCount = query.Count();
            var entities = ApplySort(query, form)
                .Skip(form.Skip)
                .Take(form.Take).ToList();
            
            var role = GetRole();

            var backlights = _serviceProvider.GetService<IEnumerable<IBacklight<TEntity>>>();
            backlights = backlights.Where(x => true); ///TODO: add role check

            var a = new SearchResult<TDto>
            {
                TotalCount = totalCount,
                Items = entities.Select(entity => MapFromEntityToListDto(entity, role, backlights)).ToList()
            };
            a.Items = FillLookupNames(a.Items).ToList();

            return a;
        }

        private TDto MapFromEntityToListDto(TEntity entity, Role role, IEnumerable<IBacklight<TEntity>> backlights)
        {
            var result = MapFromEntityToDto(entity, role);
            result.Backlights = backlights.Where(x => x.IsActive(entity))
                                          .Select(x => x.Type.FormatEnum())
                                          .ToList();
            return result;
        }

        protected Role GetRole()
        {
            Role role = null;
            var currentUserDto = _userIdProvider.GetCurrentUser();
            if (currentUserDto.RoleId.HasValue)
                role = _dataService.GetDbSet<Role>().GetById(currentUserDto.RoleId.Value);
            return role;
        }

        public IEnumerable<string> SearchIds(FilterFormDto<TFilter> form)
        {
            var dbSet = _dataService.GetDbSet<TEntity>();
            
            var query = ApplySearchForm(dbSet, form);
            query = ApplyRestrictions(query);
            
            var ids = query.Select(e => e.Id).ToList();
            
            var result = ids.Select(x => x.FormatGuid()).ToList();
            return result;
        }

        protected virtual DetailedValidationResult ValidateDto(TDto dto, TEntity entity, string updatedFieldName = null)
        {
            DetailedValidationResult result = string.IsNullOrEmpty(updatedFieldName) ?
                _validationService.Validate(dto)
                : _validationService.ValidateFiled(dto, updatedFieldName);

            if (!string.IsNullOrEmpty(updatedFieldName))
            {
                // Single field validation

                var fieldRules = _validationRules.Where(i => i.IsApplicable(updatedFieldName)).ToList();

                if (!fieldRules.Any()) return result;

                foreach (var fieldRule in fieldRules)
                {
                    var fieldValidationResult = fieldRule.Validate(dto, entity);

                    if (fieldValidationResult != null && fieldValidationResult.Errors.Any())
                    {
                        result.AddErrors(fieldValidationResult.Errors);
                    }
                }
                
                return result;
            }

            // General validation

            foreach (var rule in _validationRules)
            {
                var fieldValidationResult = rule.Validate(dto, entity);

                if (fieldValidationResult == null || !fieldValidationResult.Errors.Any()) continue;

                var errors = fieldValidationResult.Errors.Where(i => !result.Errors.Any(j => j.Name.ToLower() == i.Name.ToLower()));

                result.AddErrors(errors);
            }

            return result;
        }

        public ValidateResult SaveOrCreate(TFormDto entityFrom)
        {
            return SaveOrCreateInner(entityFrom);
        }

        private ValidateResult SaveOrCreateInner(TFormDto dto, string updatedFieldName = null)
        {
            ValidateResult mapResult;
            var dbSet = _dataService.GetDbSet<TEntity>();

            TEntity entity = null;
            if (!string.IsNullOrEmpty(dto.Id))
            {
                entity = dbSet.GetById(Guid.Parse(dto.Id));
            }

            // Validation step

            var result = ValidateDto(dto, entity, updatedFieldName);

            if (result.IsError)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(dto.Id))
            {
                if (entity == null)
                    throw new Exception($"Order not found (Id = {dto.Id})");

                MapFromFormDtoToEntity(entity, dto);

                var triggerResult = _triggersService.Execute(true);
                if (triggerResult.IsError)
                {
                    return triggerResult;
                }

                _dataService.SaveChanges();

                return new ValidateResult(entity.Id);
            }
            else
            {
                entity = new TEntity
                {
                    Id = Guid.NewGuid()
                };

                // Mapping

                MapFromFormDtoToEntity(entity, dto);

                dbSet.Add(entity);

                var triggerResult = _triggersService.Execute(true);
                if (triggerResult.IsError)
                {
                    return triggerResult;
                }

                _dataService.SaveChanges();

                return new ValidateResult(entity.Id);
            }
        }

        public IEnumerable<ActionDto> GetActions(IEnumerable<Guid> ids)
        {
            if (ids == null) 
                throw new ArgumentNullException(nameof(ids));
            
            var dbSet = _dataService.GetDbSet<TEntity>();
            var currentUser = _userIdProvider.GetCurrentUser();
            var role = currentUser.RoleId.HasValue ? _dataService.GetById<Role>(currentUser.RoleId.Value) : null;
            
            var actionDtos = new Dictionary<string, ActionInfo>();

            var entities = dbSet.Where(x => ids.Contains(x.Id)).ToList();

            if (!entities.Any()) return new List<ActionDto>();

            var singleActions = _serviceProvider.GetService<IEnumerable<IAppAction<TEntity>>>();
            foreach (var action in singleActions)
            {
                string actionName = action.GetType().Name.ToLowerFirstLetter();
                if ((role?.Actions != null && !role.Actions.Contains(actionName)) || actionDtos.ContainsKey(actionName))
                {
                    continue;
                }

                var validEntities = entities.Where(e => action.IsAvailable(e));
                if (validEntities.Any() && ids.Count() == validEntities.Count())
                {
                    actionDtos[actionName] = ConvertActionToDto(action, actionName, ids);
                }
            }
            
            var groupActions = _serviceProvider.GetService<IEnumerable<IGroupAppAction<TEntity>>>()
                .Where(i => i.IsSingleAllowed || ids.Count() > 1);

            foreach (var action in groupActions)
            {
                string actionName = action.GetType().Name.ToLowerFirstLetter();
                if ((role?.Actions != null && !role.Actions.Contains(actionName)) || actionDtos.ContainsKey(actionName))
                {
                    continue;
                }

                if (action.IsAvailable(entities))
                {
                    actionDtos[actionName] = ConvertActionToDto(action, actionName, ids);
                }
            }
            
            var result = actionDtos.OrderBy(x => x.Value.OrderNumber)
                                   .Select(x => x.Value.Dto)
                                   .ToList();
            return result;
        }

        private ActionInfo ConvertActionToDto<T>(IAction<T> action, string actionName, IEnumerable<Guid> ids)
        {
            string group = null;
            foreach (var attr in action.GetType().GetCustomAttributes(typeof(ActionGroupAttribute), false))
            {
                group = (attr as ActionGroupAttribute)?.Group;
            }

            int orderNumber = 0;
            foreach (var attr in action.GetType().GetCustomAttributes(typeof(OrderNumberAttribute), false))
            {
                orderNumber = (attr as OrderNumberAttribute)?.Value ?? orderNumber;
            }

            ActionAccess access = ActionAccess.Everywhere;
            foreach (var attr in action.GetType().GetCustomAttributes(typeof(ActionAccessAttribute), false))
            {
                access = (attr as ActionAccessAttribute)?.Access ?? access;
            }

            var dto = new ActionDto
            {
                Color = action.Color.FormatEnum(),
                Name = actionName,
                Group = group,
                AllowedFromGrid = access != ActionAccess.FormOnly,
                AllowedFromForm = access != ActionAccess.GridOnly,
                Ids = ids.Select(x => x.FormatGuid())
            };

            return new ActionInfo
            {
                Dto = dto,
                OrderNumber = orderNumber
            };
        }

        public AppResult InvokeAction(string name, Guid id)
        {
            var singleActions = _serviceProvider.GetService<IEnumerable<IAppAction<TEntity>>>();
            var action = singleActions.FirstOrDefault(x => x.GetType().Name.ToLowerFirstLetter() == name);
            
            if(action == null)
                return new AppResult
                {
                    IsError = true,
                    Message = $"Action {name} not found"
                };

            var currentUser = _userIdProvider.GetCurrentUser();
            var role = currentUser.RoleId.HasValue ? _dataService.GetById<Role>(currentUser.RoleId.Value) : null;
            var entity = GetDbSet().FirstOrDefault(x => x.Id == id);
            
            string actionName = action.GetType().Name.ToLowerFirstLetter();
            bool isActionAllowed = role?.Actions == null || role.Actions.Contains(actionName);

            if (!isActionAllowed || !action.IsAvailable(entity))
            {
                return new AppResult
                {
                    IsError = true,
                    Message = "actionUnvailable".Translate(currentUser.Language),
                    ManuallyClosableMessage = true
                };
            }

            AppResult actionResult;
            try
            {
                actionResult = action.Run(currentUser, entity);
            }
            catch (DomainException ex)
            {
                return new AppResult
                {
                    IsError = true,
                    Message = ex.Message
                };
            }

            if (actionResult.IsError)
            {
                return new AppResult
                {
                    IsError = true,
                    Message = actionResult.Message,
                    ManuallyClosableMessage = actionResult.ManuallyClosableMessage
                };
            }

            var triggerResult = _triggersService.Execute(false);
            if (triggerResult.IsError)
            {
                return triggerResult;
            }

            _dataService.SaveChanges();
            
            return new AppResult
            {
                IsError = false,
                Message = actionResult.Message,
                ManuallyClosableMessage = actionResult.ManuallyClosableMessage
            };
        }
        
        public AppResult InvokeAction(string name, IEnumerable<Guid> ids)
        {
            var singleActions = _serviceProvider.GetService<IEnumerable<IAppAction<TEntity>>>();
            var singleAction = singleActions.FirstOrDefault(x => x.GetType().Name.ToLowerFirstLetter() == name);

            var groupActions = _serviceProvider.GetService<IEnumerable<IGroupAppAction<TEntity>>>();
            var groupAction = groupActions.FirstOrDefault(x => x.GetType().Name.ToLowerFirstLetter() == name);

            if (singleAction == null && groupAction == null)
                return new AppResult
                {
                    IsError = true,
                    Message = $"Action {name} not found"
                };

            var currentUser = _userIdProvider.GetCurrentUser();
            var role = currentUser.RoleId.HasValue ? _dataService.GetById<Role>(currentUser.RoleId.Value) : null;
            var dbSet = GetDbSet();

            var entities = dbSet.Where(x => ids.Contains(x.Id));

            AppResult result;
            if (groupAction != null)
            {
                string actionName = groupAction.GetType().Name.ToLowerFirstLetter();
                bool isActionAllowed = role?.Actions == null || role.Actions.Contains(actionName);
                if (!isActionAllowed || !groupAction.IsAvailable(entities))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = "actionUnvailable".Translate(currentUser.Language),
                        ManuallyClosableMessage = true
                    };
                    
                }

                try
                {
                    result = groupAction.Run(currentUser, entities);
                }
                catch (DomainException ex)
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                bool isError = false;
                List<string> messages = new List<string>();
                string actionName = singleAction.GetType().Name.ToLowerFirstLetter();
                bool isActionAllowed = role?.Actions == null || role.Actions.Contains(actionName);
                if (isActionAllowed)
                {
                    foreach (var entity in entities)
                    {
                        if (isActionAllowed && singleAction.IsAvailable(entity))
                        {
                            try
                            {
                                var singleResult = singleAction.Run(currentUser, entity);
                                if (!string.IsNullOrEmpty(singleResult?.Message))
                                {
                                    messages.Add(singleResult.Message);
                                }
                                if (singleResult?.IsError == true)
                                {
                                    isError = true;
                                }
                            }
                            catch (DomainException ex)
                            {
                                return new AppResult
                                {
                                    IsError = true,
                                    Message = ex.Message
                                };
                            }
                        }
                    }
                }
                result = new AppResult
                {
                    IsError = isError,
                    Message = string.Join(". ", messages)
                };
            }

            if (result.IsError)
            {
                return new AppResult
                {
                    IsError = true,
                    Message = result.Message,
                    ManuallyClosableMessage = result.ManuallyClosableMessage
                };
            }

            var triggerResult = _triggersService.Execute(false);
            if (triggerResult.IsError)
            {
                return triggerResult;
            }

            _dataService.SaveChanges();
            
            return result;
        }

        public IEnumerable<BulkUpdateDto> GetBulkUpdates(IEnumerable<Guid> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var dbSet = _dataService.GetDbSet<TEntity>();
            var currentUser = _userIdProvider.GetCurrentUser();
            var role = currentUser.RoleId.HasValue ? _dataService.GetById<Role>(currentUser.RoleId.Value) : null;

            var fields = _fieldDispatcherService.GetDtoFields<TDto>();

            string forEntity = typeof(TEntity) == typeof(Order) 
                ? FieldPropertiesForEntityType.Orders.ToString() 
                : FieldPropertiesForEntityType.Shippings.ToString();
            var fieldsProperties = _fieldPropertiesService.GetFor(forEntity, currentUser?.CompanyId, role?.Id, null);
            
            var result = new List<BulkUpdateDto>();

            var entities = LoadStatusData(ids);
            
            foreach (var field in fields.Where(x => x.IsBulkUpdateAllowed))
            {
                var fieldProperties = fieldsProperties.FirstOrDefault(x => string.Compare(x.FieldName, field.Name, true) == 0);
                var validEntities = entities.Where(e => CanEdit(e, fieldProperties) && CheckRules(e.Entity, fieldProperties));
                if (validEntities.Any())
                {
                    var dto = result.FirstOrDefault(x => x.Name == field.Name.ToLowerFirstLetter());
                    if (dto == null)
                    {
                        result.Add(new BulkUpdateDto
                        {
                            Name = field.Name.ToLowerFirstLetter(),
                            Type = field.FieldType.ToString(),
                            Source = field.ReferenceSource,
                            Ids = validEntities.Select(x => x.Id)
                        });
                    }
                }
            }
            
            return result;
        }

        public AppResult InvokeBulkUpdate(string fieldName, IEnumerable<Guid> ids, string value)
        {
            string entityName = typeof(TEntity).Name;
            string propertyName = fieldName?.ToUpperFirstLetter();

            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var propertyType = typeof(TDto).GetProperty(propertyName);
            if (propertyType == null)
                throw new ArgumentException("Unknown field", nameof(propertyName));

            var rawDbSet = _dataService.GetDbSet<TEntity>();
            var dbSet = GetDbSet();

            var entities = dbSet.Where(x => ids.Contains(x.Id)).ToList();
            var validEntities = entities.Where(i => !_rules.ContainsKey(propertyName) || _rules[propertyName](i));

            var entitiesDict = new Dictionary<Guid, TEntity>();
            foreach(var entity in validEntities)
            {
                entitiesDict[entity.Id] = entity;
            }

            var role = GetRole();
            var dtos = validEntities.Select(x => MapFromEntityToDto(x, role)).ToArray();
            
            object validValue = value;
            if (propertyType.PropertyType == typeof(int?))
            {
                validValue = value.ToInt();
            }
            if (propertyType.PropertyType == typeof(decimal?))
            {
                validValue = value.ToDecimal();
            }
            if (propertyType.PropertyType == typeof(bool?) || propertyType.PropertyType == typeof(bool))
            {
                validValue = value.ToBool();
            }
            if (propertyType.PropertyType == typeof(LookUpDto))
            {
                validValue = (value == null || value == LookUpDto.EmptyValue) ? null : new LookUpDto { Value = value };
            }

            foreach (var dto in dtos)
            {
                propertyType.SetValue(dto, validValue);
            }

            var importResult = new List<ValidateResult>();
            var updatedEntities = new List<TEntity>();

            foreach (var dto in dtos)
            {
                bool isNew = false;
                TEntity entity = null;
                Guid? entityId = dto.Id.ToGuid();
                if (entityId != null)
                {
                    if (!entitiesDict.TryGetValue(entityId.Value, out entity))
                    {
                        throw new Exception($"Order not found (Id = {dto.Id})");
                    }
                }
                else
                {
                    isNew = true;
                    entity = new TEntity
                    {
                        Id = Guid.NewGuid()
                    };
                }

                var validationResult = ValidateDto(dto, entity, fieldName);
                if (validationResult.IsError)
                {
                    importResult.Add(validationResult);
                    continue;
                }

                MapFromDtoToEntity(entity, dto);

                if (isNew)
                {
                    rawDbSet.Add(entity);
                }

                updatedEntities.Add(entity);

                importResult.Add(new ValidateResult
                {
                    Id = entity.Id
                });
            }

            var triggerResult = _triggersService.Execute(true);
            if (triggerResult.IsError)
            {
                return new AppResult
                {
                    IsError = true,
                    Message = triggerResult.Message,
                    ManuallyClosableMessage = true
                };
            }

            _dataService.SaveChanges();

            string errors = string.Join(" ", importResult.Where(x => x.IsError).Select(x => x.Message));
            var result = new AppResult
            {
                IsError = !string.IsNullOrWhiteSpace(errors),
                Message = errors,
                ManuallyClosableMessage = false
            };

            if (!result.IsError)
            {
                string lang = _userIdProvider.GetCurrentUser()?.Language;
                string entityType = typeof(TEntity).Name.ToLower();
                string numbers = string.Join(", ", dtos.Select(x => x?.ToString()));
                
                var resultMessages = new List<string>();

                resultMessages.Insert(0, $"field_bulk_updated_{entityType}".Translate(lang, numbers));

                result.Message = string.Join("; ", resultMessages);
            }

            return result;
        }

        protected virtual IEnumerable<TDto> FillLookupNames(IEnumerable<TDto> dtos)
        {
            return dtos;
        }

        protected TDto FillLookupNames(TDto dto)
        {
            return FillLookupNames(new[] { dto }).FirstOrDefault();
        }

        protected T MapFromStateDto<T>(string dtoStatus) where T : struct, Enum
        {
            var mapFromStateDto = dtoStatus.ToEnum<T>() ?? default;
            return mapFromStateDto;
        }

        public IEnumerable<ValidateResult> Import(IEnumerable<TFormDto> entitiesFrom)
        {
            var dbSet = _dataService.GetDbSet<TEntity>();

            var dtoIds = entitiesFrom.Select(x => x.Id.ToGuid()).Where(x => x.HasValue).Distinct().ToList();
            var entities = dbSet.Where(x => dtoIds.Contains(x.Id)).ToList();

            var entitiedDict = new Dictionary<string, TEntity>();
            foreach (var entity in entities)
            {
                entitiedDict[entity.Id.FormatGuid()] = entity;
            }

            var result = new List<ValidateResult>();

            foreach (var dto in entitiesFrom)
            {
                bool isNew = true;
                TEntity entity = null;
                if (!string.IsNullOrEmpty(dto.Id) && entitiedDict.TryGetValue(dto.Id, out entity))
                {
                    isNew = false;
                }
                else
                {
                    isNew = true;
                    entity = new TEntity
                    {
                        Id = Guid.NewGuid()
                    };
                }

                if (!string.IsNullOrEmpty(dto.Id) && isNew)
                {
                    throw new Exception($"Order not found (Id = {dto.Id})");
                }

                var dtoResult = ValidateDto(dto, entity);
                if (dtoResult.IsError)
                {
                    result.Add(dtoResult);
                    continue;
                }

                MapFromFormDtoToEntity(entity, dto);

                if (isNew)
                {
                    dbSet.Add(entity);
                }

                result.Add(new ValidateResult(entity.Id));

                Log.Information($"Запись {entity.Id} в таблице {typeof(TEntity).Name} {(isNew ? "создана" : "обновлена")}.");
            }

            var triggerResult = _triggersService.Execute(true);
            if (triggerResult.IsError)
            {
                return new[] { triggerResult };
            }

            _dataService.SaveChanges();

            return result;
        }        
        
        public ValidateResult ImportFromExcel(Stream fileStream)
        {
            var excel = new ExcelPackage(fileStream);
            var workSheet = excel.Workbook.Worksheets.ElementAt(0);

            var user = _userIdProvider.GetCurrentUser();
            var lang = user?.Language;

            var excelMapper = CreateExcelMapper();
            var records = excelMapper.LoadEntries(workSheet, lang).ToList();
            var dtos = records.Select(i => i.Data);

            if (!dtos.Any())
            {
                return new ValidateResult("emptyFileError".Translate(lang));
            }

            if (excelMapper.Errors.Any(e => e.IsError))
            {
                string errors = string.Join(". ", excelMapper.Errors.Where(x => x.IsError).Select(x => x.Message));
                return new ValidateResult(errors);
            }

            var importResult = Import(dtos);
            
            if (importResult.Any(e => e.IsError))
            {
                string errors = string.Join(". ", importResult.Where(x => x.IsError).Select(x => x.Message));
                return new ValidateResult(errors);
            }

            return new ValidateResult();
        }
        
        public Stream ExportToExcel(ExportExcelFormDto<TFilter> dto)
        {
            var excel = new ExcelPackage();

            var user = _userIdProvider.GetCurrentUser();

            string entityName = typeof(TEntity).Name.Pluralize().ToLowerFirstLetter();
            string entityDisplayName = entityName.Translate(user.Language);
            var workSheet = excel.Workbook.Worksheets.Add(entityDisplayName);

            var dbSet = GetDbSet();
            var query = this.ApplySearchForm(dbSet, dto, dto.Columns);
            
            query = ApplyRestrictions(query);

            var backlights = _serviceProvider.GetService<IEnumerable<IBacklight<TEntity>>>();

            var entities = ApplySort(query, dto).ToList();
            
            var dtos = entities.Select(x=>MapFromEntityToListDto(x, null, backlights));
            
            dtos = FillLookupNames(dtos).ToList();
            
            var excelMapper = CreateExportExcelMapper();//new ExcelMapper<TDto>(_dataService, _userIdProvider);
            excelMapper.FillSheet(workSheet, dtos, user.Language, dto?.Columns, x => x.Backlights?.Any() == true);
            
            return new MemoryStream(excel.GetAsByteArray());
        }

        protected virtual ExcelMapper<TFormDto> CreateExcelMapper()
        {
            return new ExcelMapper<TFormDto>(_dataService, _userIdProvider, _fieldDispatcherService);
        }

        protected virtual ExcelMapper<TDto> CreateExportExcelMapper()
        {
            return new ExcelMapper<TDto>(_dataService, _userIdProvider, _fieldDispatcherService);
        }

        protected virtual IQueryable<TEntity> ApplyRestrictions(IQueryable<TEntity> query)
        {
            var currentUser = _userIdProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyPersistable).IsAssignableFrom(typeof(TEntity)))
            {
                var entity = Expression.Parameter(typeof(TEntity), string.Empty);
                var convert = Expression.Convert(entity, typeof(ICompanyPersistable));

                var entityCompanyId = Expression.PropertyOrField(convert, nameof(ICompanyPersistable.CompanyId));
                var entityCompanyIdValue = Expression.PropertyOrField(entityCompanyId, "Value");

                var isGlobalEntity = Expression.Equal(entityCompanyId, Expression.Constant(null));
                var isUserEntity = Expression.Equal(entityCompanyIdValue, Expression.Constant(currentUser.CompanyId.Value));
                var accessCheck = Expression.Or(isGlobalEntity, isUserEntity);

                var whereClause = Expression.Lambda<Func<TEntity, bool>>(accessCheck, entity);
                query = query.Where(whereClause);
            }

            return query;
        }

        protected decimal? Round(decimal? value, int decimals)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return decimal.Round(value.Value, decimals);
            }
        }

        private bool CheckRules(TEntity entity, FieldForFieldProperties fieldProperties)
        {
            var propertyName = fieldProperties.FieldName.ToUpperFirstLetter();
            return !_rules.ContainsKey(propertyName) || _rules[propertyName](entity);
        }

        private bool CanEdit(EntityStatusDto<TEntity> dto, FieldForFieldProperties fieldProperties)
        {
            string editValue = FieldPropertiesAccessType.Edit.ToString();
            string accessType = fieldProperties?.AccessTypes?
                                                .Where(x => string.Compare(x.Key, dto.Status, true) == 0)
                                                .Select(x => x.Value)
                                                .FirstOrDefault();
            
            return string.Compare(accessType, editValue, true) == 0;
        }

        private class ActionInfo
        {
            public ActionDto Dto { get; set; }
            public int OrderNumber { get; set; }
        }
    }
}