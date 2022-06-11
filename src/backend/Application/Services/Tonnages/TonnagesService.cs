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
using Domain.Services.Tonnages;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Tonnages
{
    public class TonnagesService : DictoinaryServiceBase<Tonnage, TonnageDto, TonnageFilterDto>, ITonnagesService
    {
        public TonnagesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<TonnageDto, Tonnage>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<Tonnage> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public override DetailedValidationResult MapFromDtoToEntity(Tonnage entity, TonnageDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.Name = dto.Name;
            entity.WeightKg = dto.WeightKg;
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            return null;
        }

        public override TonnageDto MapFromEntityToDto(Tonnage entity)
        {
            return new TonnageDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                WeightKg = entity.WeightKg,
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                IsActive = entity.IsActive
            };
        }

        protected override DetailedValidationResult ValidateDto(TonnageDto dto, Tonnage entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<Tonnage>(x => x.Name == dto.Name
                                                                                && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "Tonnage.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<Tonnage>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(i => i.IsActive)
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

        protected override IQueryable<Tonnage> ApplySort(IQueryable<Tonnage> query, FilterFormDto<TonnageFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<Tonnage> ApplySearch(IQueryable<Tonnage> query, FilterFormDto<TonnageFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<Tonnage>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.WeightKg.ApplyNumericFilter<Tonnage>(i => i.WeightKg, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<Tonnage, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<Tonnage>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""Tonnages"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search));
            }

            return query;
        }

        public override Tonnage FindByKey(TonnageDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<Tonnage>()
                               .FirstOrDefault(i => i.Name == dto.Name && i.CompanyId == companyId);
        }

        public override TonnageDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new TonnageDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        public override IEnumerable<Tonnage> FindByKey(IEnumerable<TonnageDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(Tonnage entity)
        {
            return entity.Name + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(TonnageDto dto)
        {
            return dto.Name + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        protected override ExcelMapper<TonnageDto> CreateExcelMapper()
        {
            return new ExcelMapper<TonnageDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}
