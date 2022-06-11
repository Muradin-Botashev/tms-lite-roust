using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Companies;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Companies
{
    public class CompaniesService : DictoinaryServiceBase<Company, CompanyDto, CompanyFilterDto>, ICompaniesService
    {
        public CompaniesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService,
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<CompanyDto, Company>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        public override DetailedValidationResult MapFromDtoToEntity(Company entity, CompanyDto dto)
        {
            var oldIsActive = entity.IsActive;

            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.Name = dto.Name;
            entity.PoolingProductType = dto.PoolingProductType?.Value.ToEnum<PoolingProductType>();
            entity.PoolingToken = dto.PoolingToken;
            entity.OrderRequiresConfirmation = dto.OrderRequiresConfirmation;
            entity.NewShippingTarifficationType = dto.NewShippingTarifficationType?.Value.ToEnum<TarifficationType>();
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            if (oldIsActive && !entity.IsActive)
            {
                var users = _dataService.GetDbSet<User>()
                                        .Where(x => x.CompanyId == entity.Id)
                                        .ToList();
                foreach (var user in users)
                {
                    user.IsActive = false;
                }
            }

            return null;
        }

        public override CompanyDto MapFromEntityToDto(Company entity)
        {
            return new CompanyDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                PoolingProductType = entity.PoolingProductType == null ? null : new LookUpDto(entity.PoolingProductType.Value.FormatEnum()),
                PoolingToken = entity.PoolingToken,
                OrderRequiresConfirmation = entity.OrderRequiresConfirmation,
                NewShippingTarifficationType = entity.NewShippingTarifficationType == null ? null : new LookUpDto(entity.NewShippingTarifficationType.Value.FormatEnum()),
                IsActive = entity.IsActive
            };
        }
        protected override DetailedValidationResult ValidateDto(CompanyDto dto, Company entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<Company>(x => x.Name == dto.Name && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "Company.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<Company>().AsQueryable();
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

        protected override IQueryable<Company> ApplySort(IQueryable<Company> query, FilterFormDto<CompanyFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<Company> ApplySearch(IQueryable<Company> query, FilterFormDto<CompanyFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<Company>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.PoolingProductType.ApplyEnumFilter<Company, PoolingProductType>(i => i.PoolingProductType, ref parameters))
                         .WhereAnd(form.Filter.PoolingToken.ApplyStringFilter<Company>(i => i.PoolingToken, ref parameters))
                         .WhereAnd(form.Filter.NewShippingTarifficationType.ApplyEnumFilter<Company, TarifficationType>(i => i.NewShippingTarifficationType, ref parameters))
                         .WhereAnd(form.Filter.OrderRequiresConfirmation.ApplyBooleanFilter<Company>(i => i.OrderRequiresConfirmation, ref parameters))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<Company>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""Companies"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var productTypeNames = Enum.GetNames(typeof(PoolingProductType)).Select(i => i.ToLower());
                var tarifficationTypeNames = Enum.GetNames(typeof(TarifficationType)).Select(i => i.ToLower());

                var productTypes = _dataService.GetDbSet<Translation>()
                    .Where(i => productTypeNames.Contains(i.Name.ToLower()))
                    .WhereTranslation(search)
                    .Select(i => i.Name.ToEnum<PoolingProductType>())
                    .ToList();

                var tarifficationTypes = _dataService.GetDbSet<Translation>()
                    .Where(i => tarifficationTypeNames.Contains(i.Name.ToLower()))
                    .WhereTranslation(search)
                    .Select(i => i.Name.ToEnum<TarifficationType>())
                    .ToList();

                query = query.Where(i =>
                            !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(search)
                            || !string.IsNullOrEmpty(i.PoolingToken) && i.PoolingToken.ToLower().Contains(search)
                            || i.PoolingProductType != null && productTypes.Contains(i.PoolingProductType)
                            || i.NewShippingTarifficationType != null && tarifficationTypes.Contains(i.NewShippingTarifficationType));
            }

            return query;
        }

        public override Company FindByKey(CompanyDto dto)
        {
            return _dataService.GetDbSet<Company>()
                .FirstOrDefault(i => i.Name == dto.Name);
        }

        public override IEnumerable<Company> FindByKey(IEnumerable<CompanyDto> dtos)
        {
            var keys = dtos.Select(x => x.Name)
                        .Where(x => x != null)
                        .Distinct()
                        .ToList();
            return _dataService.GetDbSet<Company>()
                .Where(i => keys.Contains(i.Name))
                .ToList();
        }

        public override string GetEntityKey(Company entity)
        {
            return entity.Name;
        }

        public override string GetDtoKey(CompanyDto dto)
        {
            return dto.Name;
        }

        protected override IQueryable<Company> ApplyRestrictions(IQueryable<Company> query)
        {
            query = base.ApplyRestrictions(query);

            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null)
            {
                query = query.Where(x => x.Id == currentUser.CompanyId.Value);
            }

            return query;
        }

        public override CompanyDto GetDefaults()
        {
            return new CompanyDto
            {
                IsActive = true
            };
        }
    }
}
