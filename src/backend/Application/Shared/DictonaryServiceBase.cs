using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared.Excel;
using Application.Shared.Triggers;
using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.AppConfiguration;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Application.Shared
{
    public abstract class DictoinaryServiceBase<TEntity, TListDto, TFilter>
        where TEntity : class, IPersistable, new()
        where TListDto : IDto, new()
        where TFilter : SearchFilterDto, new()
    {
        public abstract DetailedValidationResult MapFromDtoToEntity(TEntity entity, TListDto dto);

        public abstract TListDto MapFromEntityToDto(TEntity entity);

        protected abstract IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, FilterFormDto<TFilter> searchForm, List<string> columns = null);

        protected readonly ICommonDataService _dataService;
        protected readonly IUserProvider _userProvider;
        protected readonly ITriggersService _triggersService;
        protected readonly IFieldDispatcherService _fieldDispatcherService;
        private readonly IValidationService _validationService;
        protected readonly IEnumerable<IValidationRule<TListDto, TEntity>> _validationRules;

        protected DictoinaryServiceBase(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<TListDto, TEntity>> validationRules)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _triggersService = triggersService;
            _validationService = validationService;
            _fieldDispatcherService = fieldDispatcherService;
            _validationRules = validationRules;
        }

        protected virtual IQueryable<TEntity> GetDbSet()
        {
            return _dataService.GetDbSet<TEntity>();
        }

        public TListDto Get(Guid id)
        {
            var entity = GetDbSet().GetById(id);

            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyPersistable).IsAssignableFrom(typeof(TEntity)))
            {
                var companyEntity = (ICompanyPersistable)entity;
                var entityCompanyId = companyEntity?.CompanyId;
                if (entityCompanyId != null && entityCompanyId != currentUser.CompanyId)
                {
                    throw new AccessDeniedException(currentUser?.Language);
                }
            }

            var result = MapFromEntityToDto(entity);
            return result;
        }

        public virtual IEnumerable<LookUpDto> ForSelect()
        {
            return new List<LookUpDto>();
        }

        public virtual IEnumerable<LookUpDto> ForSelect(Guid? companyId)
        {
            return ForSelect();
        }

        public virtual IEnumerable<LookUpDto> ForSelect(string fieldName, FilterFormDto<TFilter> form)
        {
            foreach (var prop in form.Filter.GetType().GetProperties())
            {
                if (string.Equals(prop.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    prop.SetValue(form.Filter, null);
                }
            }

            var dbSet = GetDbSet();

            var query = ApplySearch(dbSet, form);
            query = ApplyRestrictions(query);

            var propertyInfo = typeof(TEntity).GetProperties()
                .FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());
            var refType = GetReferenceType(propertyInfo);

            var fields = _fieldDispatcherService.GetDtoFields<TListDto>();
            var field = fields.FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());

            if (refType != null)
            {
                var result = GetReferencedValues(query, refType, fieldName);
                return result;
            }
            else if (field.FieldType == FieldType.State)
            {
                return GetStateValues(query, propertyInfo);
            }
            else
            {
                return GetSelectValues(query, propertyInfo, field.ShowRawReferenceValue);
            }
        }

        private IEnumerable<LookUpDto> GetReferencedValues(IQueryable<TEntity> query, Type refType, string field)
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

        private IEnumerable<LookUpDto> GetStateValues(IQueryable<TEntity> query, PropertyInfo propertyInfo)
        {
            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Select(i => i.ToString())
                .Distinct();

            return result.Select(i => new LookUpDto
            {
                Name = i,
                Value = i
            })
            .ToList();
        }

        private IEnumerable<LookUpDto> GetSelectValues(IQueryable<TEntity> query, PropertyInfo propertyInfo, bool showRawName)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct()
                .ToList();

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

        public virtual TEntity FindByKey(TListDto dto)
        {
            return FindById(dto);
        }

        public virtual IEnumerable<TEntity> FindByKey(IEnumerable<TListDto> dtos)
        {
            return FindById(dtos);
        }

        public virtual string GetEntityKey(TEntity entity)
        {
            return entity.Id.FormatGuid();
        }

        public virtual string GetDtoKey(TListDto dto)
        {
            return dto.Id ?? string.Empty;
        }

        public SearchResult<TListDto> Search(FilterFormDto<TFilter> form)
        {
            var dbSet = GetDbSet();
            var query = this.ApplySearch(dbSet, form);
            query = ApplyRestrictions(query);

            if (form.Take == 0)
                form.Take = 1000;

            var totalCount = query.Count();
            var entities = ApplySort(query, form)
                .Skip(form.Skip)
                .Take(form.Take).ToList();

            return new SearchResult<TListDto>
            {
                TotalCount = totalCount,
                Items = entities.Select(entity => MapFromEntityToDto(entity)).ToList()
            };
        }

        protected virtual IQueryable<TEntity> ApplySort(IQueryable<TEntity> query, FilterFormDto<TFilter> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Id, true);
        }

        public AppResult Import(IEnumerable<ValidatedRecord<TListDto>> entitiesFrom, bool isConfirmed)
        {
            var dbSet = _dataService.GetDbSet<TEntity>();

            var entities = FindByKey(entitiesFrom.Where(x => !x.Result.IsError).Select(x => x.Data));

            var entitiedDict = new Dictionary<string, TEntity>();
            foreach (var entity in entities)
            {
                string key = GetEntityKey(entity);
                entitiedDict[key] = entity;
            }

            var confirmationMessages = new Dictionary<string, List<string>>();
            var savedDtos = new List<TListDto>();
            foreach (var record in entitiesFrom.Where(x => !x.Result.IsError))
            {
                var dto = record.Data;

                string key = GetDtoKey(dto);
                bool isNew = true;
                TEntity entity = null;
                if (entitiedDict.TryGetValue(key, out entity))
                {
                    isNew = false;
                    dto.Id = entity.Id.FormatGuid();
                }
                else
                {
                    isNew = true;
                    entity = new TEntity
                    {
                        Id = Guid.NewGuid()
                    };
                }

                record.Result = ValidateDto(record.Data, entity, isConfirmed);
                if (record.Result.IsError || record.Result.NeedConfirmation)
                {
                    if (!string.IsNullOrEmpty(record.Result.ConfirmationMessage))
                    {
                        List<string> rows;
                        if (!confirmationMessages.TryGetValue(record.Result.ConfirmationMessage, out rows))
                        {
                            rows = new List<string>();
                            confirmationMessages[record.Result.ConfirmationMessage] = rows;
                        }
                        rows.Add(record.RecordNumber.ToString());
                    }
                    continue;
                }

                record.Result = ValidateImportDuplicates(record.Data, savedDtos);
                if (record.Result.IsError)
                {
                    continue;
                }

                record.Result.ResultType = isNew ? ValidateResultType.Created : ValidateResultType.Updated;
                savedDtos.Add(record.Data);

                // Mapping

                MapFromDtoToEntity(entity, dto);

                if (isNew)
                {
                    dbSet.Add(entity);
                }

                Log.Information($"Запись {entity.Id} в справочнике {typeof(TEntity)} {(isNew ? "создана" : "обновлена")}.");
            }

            var importResult = new ImportResult();
            importResult.Results.AddRange(entitiesFrom.Select(i => i.Result));

            var lang = _userProvider.GetCurrentUser()?.Language;
            var confirmationMessage = string.Join(' ', confirmationMessages.Select(x => "importLineError".Translate(lang, string.Join(", ", x.Value), x.Key)));
            var result = MapFromImportResult(importResult, confirmationMessage);

            if (result.NeedConfirmation)
            {
                return result;
            }

            var changes = _dataService.GetChanges<TEntity>();

            var changedEntityIds = changes.Where(x => x.FieldChanges.Any()).Select(x => x.Entity.Id).ToHashSet();
            foreach (var record in entitiesFrom.Where(x => x.Result.ResultType == ValidateResultType.Updated))
            {
                var id = record.Data.Id.ToGuid();
                if (id != null && !changedEntityIds.Contains(id.Value))
                {
                    record.Result.ResultType = ValidateResultType.Skipped;
                }
            }

            var triggerResult = _triggersService.Execute(true);
            if (triggerResult.IsError)
            {
                return triggerResult;
            }

            _dataService.SaveChanges();

            return result;
        }

        public AppResult Import(IEnumerable<TListDto> entitiesFrom, bool isConfirmed)
        {
            return Import(entitiesFrom.Select(i => new ValidatedRecord<TListDto>(i)), isConfirmed);
        }

        public AppResult ImportFromExcel(Stream fileStream, bool isConfirmed)
        {
            var excel = new ExcelPackage(fileStream);
            var workSheet = excel.Workbook.Worksheets[0];//.ElementAt(0);

            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var excelMapper = CreateExcelMapper();
            var dtos = excelMapper.LoadEntries(workSheet, lang).ToList();

            if (!dtos.Any())
            {
                return new ValidateResult("emptyFileError".Translate(lang));
            }

            var importResult = Import(dtos, isConfirmed);
            return importResult;
        }

        private AppResult MapFromImportResult(ImportResult importResult, string confirmationMessage)
        {
            var user = _userProvider.GetCurrentUser();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("validation.createdCountMessage".Translate(user.Language, importResult.CreatedCount));
            sb.AppendLine("validation.updatedCountMessage".Translate(user.Language, importResult.UpdatedCount));

            if (importResult.DuplicatedRecordErrorsCount > 0)
            {
                sb.AppendLine("validation.duplicatedRecordErrorMessage".Translate(user.Language, importResult.DuplicatedRecordErrorsCount));
            }

            if (importResult.InvalidDictionaryValueErrorsCount > 0)
            {
                sb.AppendLine("validation.invalidDictionaryValueErrorMessage".Translate(user.Language, importResult.InvalidDictionaryValueErrorsCount));
            }

            if (importResult.InvalidValueFormatErrorsCount > 0)
            {
                sb.AppendLine("validation.invalidFormatErrorCountMessage".Translate(user.Language, importResult.InvalidValueFormatErrorsCount));
            }

            if (importResult.RequiredErrorsCount > 0)
            {
                sb.AppendLine("validation.requiredErrorMessage".Translate(user.Language, importResult.RequiredErrorsCount));
            }

            return new AppResult
            {
                Message = sb.ToString(),
                NeedConfirmation = importResult.Results.Any(x => x.NeedConfirmation),
                ConfirmationMessage = confirmationMessage
            };
        }

        public Stream ExportToExcel(FilterFormDto<TFilter> form)
        {
            var excel = new ExcelPackage();

            var user = _userProvider.GetCurrentUser();

            string entityName = typeof(TEntity).Name.Pluralize().ToLowerFirstLetter();
            string entityDisplayName = entityName.Translate(user.Language);
            var workSheet = excel.Workbook.Worksheets.Add(entityDisplayName);

            var dbSet = GetDbSet();
            var query = ApplySearch(dbSet, form);
            query = ApplyRestrictions(query);

            var entities = ApplySort(query, form).ToList();
            var dtos = entities.Select(MapFromEntityToDto);

            var excelMapper = CreateExcelMapper();
            excelMapper.FillSheet(workSheet, dtos, user.Language);

            return new MemoryStream(excel.GetAsByteArray());
        }

        public DetailedValidationResult SaveOrCreate(TListDto entityFrom, bool isConfirmed)
        {
            return SaveOrCreateInner(entityFrom, false, isConfirmed);
        }

        protected TEntity FindById(TListDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id) && Guid.TryParse(dto.Id, out Guid id))
            {
                var dbSet = _dataService.GetDbSet<TEntity>();
                return dbSet.GetById(id);
            }
            else
            {
                return null;
            }
        }

        protected IEnumerable<TEntity> FindById(IEnumerable<TListDto> dtos)
        {
            List<Guid> ids = new List<Guid>();
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrEmpty(dto.Id) && Guid.TryParse(dto.Id, out Guid id))
                {
                    ids.Add(id);
                }
            }
            var result = _dataService.GetDbSet<TEntity>()
                                     .Where(x => ids.Contains(x.Id))
                                     .ToList();
            return result;
        }

        protected DetailedValidationResult SaveOrCreateInner(TListDto dto, bool isImport, bool isConfirmed)
        {
            var dbSet = _dataService.GetDbSet<TEntity>();

            var entity = isImport ? FindByKey(dto) : FindById(dto);
            var isNew = entity == null;

            if (isNew)
            {
                entity = new TEntity
                {
                    Id = Guid.NewGuid()
                };
            }
            else if (isImport)
            {
                dto.Id = entity.Id.FormatGuid();
            }

            // Validation step

            var result = ValidateDto(dto, entity, isConfirmed);

            if (result.IsError)
            {
                Log.Information($"Не удалось сохранить запись в справочник {typeof(TEntity)}: {result.Message}.");
                return result;
            }

            if (result.NeedConfirmation)
            {
                Log.Information($"Сохранение записи в справочник {typeof(TEntity)} требует подтверждения: {result.ConfirmationMessage}.");
                return result;
            }

            // Mapping

            MapFromDtoToEntity(entity, dto);

            if (isNew)
            {
                dbSet.Add(entity);
            }

            if (isNew)
            {
                result.ResultType = ValidateResultType.Created;
            }
            else
            {
                //dbSet.Update(entityFromDb);
                result.ResultType = ValidateResultType.Updated;
            }

            var triggerResult = _triggersService.Execute(true);
            if (triggerResult.IsError)
            {
                return new DetailedValidationResult(triggerResult.Message, entity.Id);
            }

            _dataService.SaveChanges();

            Log.Information($"Запись {entity.Id} в справочнике {typeof(TEntity)} {(isNew ? "создана" : "обновлена")}.");

            return result;
        }

        protected DetailedValidationResult ValidateImportDuplicates(TListDto dto, IEnumerable<TListDto> otherDtos)
        {
            var result = new DetailedValidationResult();
            var lang = _userProvider.GetCurrentUser()?.Language;

            var existKeys = otherDtos.Select(x => GetDtoKey(x) ?? string.Empty).ToHashSet();
            var isDuplicate = existKeys.Contains(GetDtoKey(dto) ?? string.Empty);

            if (isDuplicate)
            {
                result.AddError("duplicate", $"{typeof(TEntity).Name}.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        protected virtual DetailedValidationResult ValidateDto(TListDto dto, TEntity entity, bool isConfirmed)
        {
            var result = _validationService.Validate(dto);

            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyDto).IsAssignableFrom(typeof(TListDto)))
            {
                var companyDto = (ICompanyDto)dto;
                var dtoCompanyId = companyDto.CompanyId?.Value?.ToGuid();

                if (dtoCompanyId == null || dtoCompanyId != currentUser.CompanyId)
                {
                    result.AddError(nameof(companyDto.CompanyId), "companyRequired".Translate(currentUser.Language), ValidationErrorType.ValueIsRequired);
                }
            }

            foreach (var rule in _validationRules)
            {
                var fieldValidationResult = rule.Validate(dto, entity);

                if (fieldValidationResult == null || !fieldValidationResult.Errors.Any()) continue;

                var errors = fieldValidationResult.Errors.Where(i => !result.Errors.Any(j => j.Name.ToLower() == i.Name.ToLower()));

                result.AddErrors(errors);
            }

            return result;
        }

        protected T MapFromStateDto<T>(string dtoStatus) where T : struct, Enum
        {
            var mapFromStateDto = dtoStatus.ToEnum<T>() ?? default;
            return mapFromStateDto;
        }

        protected virtual ExcelMapper<TListDto> CreateExcelMapper()
        {
            return new ExcelMapper<TListDto>(_dataService, _userProvider, _fieldDispatcherService);
        }

        public ValidateResult Delete(Guid id)
        {
            var entity = _dataService.GetById<TEntity>(id);

            if (entity == null) return new ValidateResult("Запись не найдена", id, true);

            _dataService.Remove(entity);
            _dataService.SaveChanges();

            return new ValidateResult(id);
        }

        public virtual TListDto GetDefaults()
        {
            return new TListDto();
        }

        protected virtual IQueryable<TEntity> ApplyRestrictions(IQueryable<TEntity> query)
        {
            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyPersistable).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Cast<ICompanyPersistable>()
                    .Where(i => i.CompanyId == null || i.CompanyId == currentUser.CompanyId)
                    .Cast<TEntity>();
            }

            return query;
        }

        public virtual UserConfigurationDictionaryItem GetFormConfiguration(Guid id, UserConfigurationDictionaryItem defaultConfig)
        {
            var entity = _dataService.GetById<TEntity>(id);

            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null && typeof(ICompanyPersistable).IsAssignableFrom(typeof(TEntity)))
            {
                var column = defaultConfig.Columns.Where(i => i.Name.ToLower() == nameof(ICompanyPersistable.CompanyId).ToLower()).FirstOrDefault();

                if (column != null)
                {
                    column.IsRequired = true;
                }

                var companyEntity = (ICompanyPersistable)entity;

                if (companyEntity.CompanyId != currentUser.CompanyId)
                {
                    defaultConfig.Columns.ToList().ForEach(i => i.IsReadOnly = true);
                }
            }

            return defaultConfig;
        }
    }
}