using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using DAL.Extensions;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Services.TransportCompanies;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.TransportCompanies
{
    public class TransportCompaniesService : DictoinaryServiceBase<TransportCompany, TransportCompanyDto, TransportCompanyFilterDto>, ITransportCompaniesService
    {
        public TransportCompaniesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<TransportCompanyDto, TransportCompany>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<TransportCompany> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var query = _dataService.GetDbSet<TransportCompany>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(i => i.IsActive)
                                .OrderBy(c => c.Title)
                                .ToList();

            var empty = new LookUpDto
            {
                Name = "emptyValue".Translate(lang),
                Value = LookUpDto.EmptyValue,
                IsFilterOnly = true
            };
            yield return empty;

            foreach (TransportCompany carrier in entities)
            {
                yield return new LookUpDto
                {
                    Name = carrier.Title,
                    Value = carrier.Id.FormatGuid()
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

        public override IEnumerable<LookUpDto> ForSelect(Guid? companyId)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            companyId = companyId ?? _userProvider.GetCurrentUser()?.CompanyId;
            var entities = _dataService.GetDbSet<TransportCompany>()
                                       .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                       .OrderBy(x => x.Title)
                                       .ToList();

            var empty = new LookUpDto
            {
                Name = "emptyValue".Translate(lang),
                Value = LookUpDto.EmptyValue,
                IsFilterOnly = true
            };
            yield return empty;

            foreach (var entity in entities)
            {
                yield return new LookUpDto
                {
                    Name = entity.Title,
                    Value = entity.Id.FormatGuid()
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

        public override DetailedValidationResult MapFromDtoToEntity(TransportCompany entity, TransportCompanyDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);
            entity.Title = dto.Title;
            entity.PowerOfAttorneyNumber = dto.PowerOfAttorneyNumber;
            entity.DateOfPowerOfAttorney = dto.DateOfPowerOfAttorney.ToDate();
            entity.Email = dto.Email;
            entity.ContactInfo = dto.ContactInfo;
            entity.Forwarder = dto.Forwarder;
            entity.RequestReviewDuration = dto.RequestReviewDuration;
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            return null;
        }

        protected override DetailedValidationResult ValidateDto(TransportCompanyDto dto, TransportCompany entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();

            var otherCarriers = _dataService.GetDbSet<TransportCompany>()
                                            .Where(x => (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                        && x.Id != currentId)
                                            .ToList();

            var hasTitleDuplicates = otherCarriers.Any(x => !string.IsNullOrEmpty(dto.Title) && x.Title != null && x.Title.ToLower() == dto.Title.ToLower());
            if (hasTitleDuplicates)
            {
                result.AddError(nameof(dto.Title), "TransportCompany.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            var hasEmailDuplicates = otherCarriers.Any(x => !string.IsNullOrEmpty(dto.Email) && x.Email != null && x.Email.ToLower() == dto.Email.ToLower());
            if (hasEmailDuplicates)
            {
                result.AddError(nameof(dto.Email), "TransportCompany.DuplicatedEmailRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            var hasForwarderDuplicates = otherCarriers.Any(x => !string.IsNullOrEmpty(dto.Forwarder) && x.Forwarder != null && x.Forwarder.ToLower() == dto.Forwarder.ToLower());
            if (hasForwarderDuplicates)
            {
                result.AddError(nameof(dto.Forwarder), "TransportCompany.DuplicatedForwarderRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override TransportCompanyDto MapFromEntityToDto(TransportCompany entity)
        {
            return new TransportCompanyDto
            {
                Id = entity.Id.FormatGuid(),
                Title = entity.Title,
                PowerOfAttorneyNumber = entity.PowerOfAttorneyNumber,
                DateOfPowerOfAttorney = entity.DateOfPowerOfAttorney.FormatDate(),
                Email = entity.Email,
                ContactInfo = entity.ContactInfo,
                Forwarder = entity.Forwarder,
                RequestReviewDuration = entity.RequestReviewDuration,
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                IsActive = entity.IsActive
            };
        }

        protected override IQueryable<TransportCompany> ApplySort(IQueryable<TransportCompany> query, FilterFormDto<TransportCompanyFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Title, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<TransportCompany> ApplySearch(IQueryable<TransportCompany> query, FilterFormDto<TransportCompanyFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Title.ApplyStringFilter<TransportCompany>(i => i.Title, ref parameters))
                         .WhereAnd(form.Filter.PowerOfAttorneyNumber.ApplyStringFilter<TransportCompany>(i => i.PowerOfAttorneyNumber, ref parameters))
                         .WhereAnd(form.Filter.DateOfPowerOfAttorney.ApplyDateRangeFilter<TransportCompany>(i => i.DateOfPowerOfAttorney, ref parameters))
                         .WhereAnd(form.Filter.Email.ApplyStringFilter<TransportCompany>(i => i.Email, ref parameters))
                         .WhereAnd(form.Filter.ContactInfo.ApplyStringFilter<TransportCompany>(i => i.ContactInfo, ref parameters))
                         .WhereAnd(form.Filter.Forwarder.ApplyStringFilter<TransportCompany>(i => i.Forwarder, ref parameters))
                         .WhereAnd(form.Filter.RequestReviewDuration.ApplyNumericFilter<TransportCompany>(i => i.RequestReviewDuration, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<TransportCompany, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<TransportCompany>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""TransportCompanies"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var searchDateFormat = "dd.mm.yyyy HH24:MI";

                int? searchInt = search.ToInt();
                var isInt = searchInt != null;
                
                query = query.Where(i =>
                           i.Title.ToLower().Contains(search)
                        || i.PowerOfAttorneyNumber.ToLower().Contains(search)
                        || i.Email.ToLower().Contains(search)
                        || i.ContactInfo.ToLower().Contains(search)
                        || i.Forwarder.ToLower().Contains(search)

                        || i.DateOfPowerOfAttorney != null && i.DateOfPowerOfAttorney.Value.SqlFormat(searchDateFormat).Contains(search)

                        || isInt && i.RequestReviewDuration != null && i.RequestReviewDuration == searchInt);
            }

            return query;
        }

        public override TransportCompany FindByKey(TransportCompanyDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<TransportCompany>()
                               .FirstOrDefault(i => i.Title == dto.Title && i.CompanyId == companyId);
        }

        public override IEnumerable<TransportCompany> FindByKey(IEnumerable<TransportCompanyDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(TransportCompany entity)
        {
            return entity.Title + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(TransportCompanyDto dto)
        {
            return dto.Title + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override TransportCompanyDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new TransportCompanyDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override ExcelMapper<TransportCompanyDto> CreateExcelMapper()
        {
            return new ExcelMapper<TransportCompanyDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}
