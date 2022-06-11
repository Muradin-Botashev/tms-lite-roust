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
using Domain.Services.BodyTypes;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.BodyTypes
{
    public class BodyTypesService : DictoinaryServiceBase<BodyType, BodyTypeDto, BodyTypeFilterDto>, IBodyTypesService
    {
        public BodyTypesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<BodyTypeDto, BodyType>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<BodyType> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public override DetailedValidationResult MapFromDtoToEntity(BodyType entity, BodyTypeDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.Name = dto.Name;
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            return null;
        }

        public override BodyTypeDto MapFromEntityToDto(BodyType entity)
        {
            return new BodyTypeDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                IsActive = entity.IsActive
            };
        }
        protected override DetailedValidationResult ValidateDto(BodyTypeDto dto, BodyType entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<BodyType>(x => x.Name == dto.Name 
                                                                                && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "BodyType.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<BodyType>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(i => i.IsActive)
                                .OrderBy(x => x.Name)
                                .ToList();

            foreach (var entity in entities)
            {
                yield return new LookUpDto
                {
                    Name = entity.Name,
                    Value = entity.Id.FormatGuid(),
                };
            }
        }

        protected override IQueryable<BodyType> ApplySort(IQueryable<BodyType> query, FilterFormDto<BodyTypeFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<BodyType> ApplySearch(IQueryable<BodyType> query, FilterFormDto<BodyTypeFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<BodyType>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<BodyType, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<BodyType>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""BodyTypes"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search));
            }

            return query;
        }

        public override BodyType FindByKey(BodyTypeDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<BodyType>()
                               .FirstOrDefault(i => i.Name == dto.Name && i.CompanyId == companyId);
        }

        public override IEnumerable<BodyType> FindByKey(IEnumerable<BodyTypeDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(BodyType entity)
        {
            return entity.Name + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(BodyTypeDto dto)
        {
            return dto.Name + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override BodyTypeDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new BodyTypeDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override ExcelMapper<BodyTypeDto> CreateExcelMapper()
        {
            return new ExcelMapper<BodyTypeDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}
