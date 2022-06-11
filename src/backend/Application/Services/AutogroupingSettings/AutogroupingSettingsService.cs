using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.AutogroupingSettings;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.AutogroupingSettings
{
    public class AutogroupingSettingsService : DictoinaryServiceBase<AutogroupingSetting, AutogroupingSettingDto, AutogroupingSettingFilterDto>, IAutogroupingSettingsService
    {
        public AutogroupingSettingsService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<AutogroupingSettingDto, AutogroupingSetting>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<AutogroupingSetting> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company)
                .Include(i => i.Tonnage);
        }

        public override DetailedValidationResult MapFromDtoToEntity(AutogroupingSetting entity, AutogroupingSettingDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.MaxUnloadingPoints = dto.MaxUnloadingPoints;
            entity.RegionOverrunCoefficient = dto.RegionOverrunCoefficient;
            entity.InterregionOverrunCoefficient = dto.InterregionOverrunCoefficient;
            entity.CheckPoolingSlots = dto.CheckPoolingSlots;
            entity.TonnageId = dto.TonnageId?.Value.ToGuid();

            return null;
        }

        public override AutogroupingSettingDto MapFromEntityToDto(AutogroupingSetting entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            return new AutogroupingSettingDto
            {
                Id = entity.Id.FormatGuid(),
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                MaxUnloadingPoints = entity.MaxUnloadingPoints,
                RegionOverrunCoefficient = entity.RegionOverrunCoefficient,
                InterregionOverrunCoefficient = entity.InterregionOverrunCoefficient,
                CheckPoolingSlots = entity.CheckPoolingSlots,
                TonnageId = entity.TonnageId == null ? null : new LookUpDto(entity.TonnageId.FormatGuid(), entity.Tonnage.ToString())
            };
        }
        protected override DetailedValidationResult ValidateDto(AutogroupingSettingDto dto, AutogroupingSetting entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentCompanyId = dto.CompanyId?.Value.ToGuid();

            var tonnageId = dto.TonnageId?.Value.ToGuid();
            var tonnage = tonnageId == null ? null : _dataService.GetById<Tonnage>(tonnageId.Value);
            if (tonnage?.CompanyId != null && tonnage.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.TonnageId), "invalidCompanyTonnage".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var currentId = dto.Id.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<AutogroupingSetting>(x => x.CompanyId == currentCompanyId && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.CompanyId), "AutogroupingSettings.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<AutogroupingSetting>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Include(x => x.Company)
                                .OrderBy(x => x.Company.Name)
                                .ToList();

            foreach (var entity in entities)
            {
                yield return new LookUpDto
                {
                    Name = entity.Company?.Name,
                    Value = entity.Id.FormatGuid(),
                };
            }
        }

        protected override IQueryable<AutogroupingSetting> ApplyRestrictions(IQueryable<AutogroupingSetting> query)
        {
            query = base.ApplyRestrictions(query);

            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            if (companyId != null)
            {
                query = query.Where(x => x.CompanyId == null || x.CompanyId == companyId);
            }

            return query;
        }

        protected override IQueryable<AutogroupingSetting> ApplySort(IQueryable<AutogroupingSetting> query, FilterFormDto<AutogroupingSettingFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<AutogroupingSetting> ApplySearch(IQueryable<AutogroupingSetting> query, FilterFormDto<AutogroupingSettingFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<AutogroupingSetting, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.MaxUnloadingPoints.ApplyNumericFilter<AutogroupingSetting>(i => i.MaxUnloadingPoints, ref parameters))
                         .WhereAnd(form.Filter.RegionOverrunCoefficient.ApplyNumericFilter<AutogroupingSetting>(i => i.RegionOverrunCoefficient, ref parameters))
                         .WhereAnd(form.Filter.InterregionOverrunCoefficient.ApplyNumericFilter<AutogroupingSetting>(i => i.InterregionOverrunCoefficient, ref parameters))
                         .WhereAnd(form.Filter.CheckPoolingSlots.ApplyBooleanFilter<AutogroupingSetting>(i => i.CheckPoolingSlots, ref parameters))
                         .WhereAnd(form.Filter.TonnageId.ApplyOptionsFilter<AutogroupingSetting, Guid?>(i => i.TonnageId, ref parameters, i => new Guid(i)));

            string sql = $@"SELECT * FROM ""AutogroupingSettings"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                decimal? searchDecimal = search.ToDecimal();
                var isDecimal = searchDecimal != null;
                decimal precision = 0.01M;

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var tonnages = _dataService.GetDbSet<Tonnage>()
                    .Where(i => i.Name.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var companies = _dataService.GetDbSet<Company>()
                    .Where(i => i.Name.ToLower().Contains(search))
                    .Select(i => i.Id);

                query = query.Where(i =>
                           tonnages.Any(t => t == i.TonnageId)
                        || companies.Any(t => t == i.CompanyId)
                        || isDecimal && i.MaxUnloadingPoints >= searchDecimal - precision && i.MaxUnloadingPoints <= searchDecimal + precision
                        || isDecimal && i.RegionOverrunCoefficient >= searchDecimal - precision && i.RegionOverrunCoefficient <= searchDecimal + precision
                        || isDecimal && i.InterregionOverrunCoefficient >= searchDecimal - precision && i.InterregionOverrunCoefficient <= searchDecimal + precision
                    );
            }

            return query;
        }

        public override AutogroupingSettingDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new AutogroupingSettingDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString())
            };
        }

        protected override ExcelMapper<AutogroupingSettingDto> CreateExcelMapper()
        {
            return new ExcelMapper<AutogroupingSettingDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name))
                .MapColumn(w => w.TonnageId, new DictionaryReferenceExcelColumn<VehicleType>(_dataService, _userProvider, x => x.Name));
        }
    }
}
