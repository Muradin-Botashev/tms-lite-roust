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
using Domain.Services.FieldProperties;
using Domain.Services.PickingTypes;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.PickingTypes
{
    public class PickingTypesService : DictoinaryServiceBase<PickingType, PickingTypeDto, PickingTypeFilterDto>, IPickingTypesService
    {
        public PickingTypesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<PickingTypeDto, PickingType>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<PickingType> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public override DetailedValidationResult MapFromDtoToEntity(PickingType entity, PickingTypeDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.Name = dto.Name;
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            return null;
        }

        protected override DetailedValidationResult ValidateDto(PickingTypeDto dto, PickingType entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<PickingType>(x => x.Name == dto.Name
                                                                                    && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                    && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "PickingType.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override PickingTypeDto MapFromEntityToDto(PickingType entity)
        {
            return new PickingTypeDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                IsActive = entity.IsActive
            };
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var query = _dataService.GetDbSet<PickingType>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(i => i.IsActive)
                                .OrderBy(c => c.Name)
                                .ToList();

            var empty = new LookUpDto
            {
                Name = "emptyValue".Translate(lang),
                Value = LookUpDto.EmptyValue,
                IsFilterOnly = true
            };
            yield return empty;

            foreach (PickingType pickingType in entities)
            {
                yield return new LookUpDto
                {
                    Name = pickingType.Name,
                    Value = pickingType.Id.FormatGuid(),
                };
            }

            var remove = new LookUpDto
            {
                Name = "removeValue".Translate(lang),
                Value = LookUpDto.EmptyValue,
                IsBulkUpdateOnly = true
            };
            yield return remove;
        }

        protected override IQueryable<PickingType> ApplySort(IQueryable<PickingType> query, FilterFormDto<PickingTypeFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<PickingType> ApplySearch(IQueryable<PickingType> query, FilterFormDto<PickingTypeFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<PickingType>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<PickingType, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<PickingType>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""PickingTypes"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search));
            }

            return query;
        }

        public override PickingType FindByKey(PickingTypeDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<PickingType>()
                               .FirstOrDefault(i => i.Name == dto.Name && i.CompanyId == companyId);
        }

        public override IEnumerable<PickingType> FindByKey(IEnumerable<PickingTypeDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(PickingType entity)
        {
            return entity.Name + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(PickingTypeDto dto)
        {
            return dto.Name + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override PickingTypeDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new PickingTypeDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override ExcelMapper<PickingTypeDto> CreateExcelMapper()
        {
            return new ExcelMapper<PickingTypeDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}
