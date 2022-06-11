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
using Domain.Services.DocumentTypes;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.DocumentTypes
{

    public class DocumentTypesService : DictoinaryServiceBase<DocumentType, DocumentTypeDto, DocumentTypeFilterDto>, IDocumentTypesService
    {
        public DocumentTypesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<DocumentTypeDto, DocumentType>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<DocumentType> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public override DetailedValidationResult MapFromDtoToEntity(DocumentType entity, DocumentTypeDto dto)
        {
            entity.Name = dto.Name;
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            return null;
        }
        protected override DetailedValidationResult ValidateDto(DocumentTypeDto dto, DocumentType entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<DocumentType>(x => x.Name == dto.Name
                                                                                    && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                    && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "DocumentType.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<DocumentType>().AsQueryable();
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

        public override DocumentTypeDto MapFromEntityToDto(DocumentType entity)
        {
            return new DocumentTypeDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                IsActive = entity.IsActive
            };
        }

        protected override IQueryable<DocumentType> ApplySort(IQueryable<DocumentType> query, FilterFormDto<DocumentTypeFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<DocumentType> ApplySearch(IQueryable<DocumentType> query, FilterFormDto<DocumentTypeFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<DocumentType>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<DocumentType, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<DocumentType>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""DocumentTypes"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search));
            }

            return query;
        }

        public override DocumentType FindByKey(DocumentTypeDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<DocumentType>()
                               .FirstOrDefault(i => i.Name == dto.Name && i.CompanyId == companyId);
        }

        public override IEnumerable<DocumentType> FindByKey(IEnumerable<DocumentTypeDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(DocumentType entity)
        {
            return entity.Name + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(DocumentTypeDto dto)
        {
            return dto.Name + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override DocumentTypeDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new DocumentTypeDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override ExcelMapper<DocumentTypeDto> CreateExcelMapper()
        {
            return new ExcelMapper<DocumentTypeDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}
