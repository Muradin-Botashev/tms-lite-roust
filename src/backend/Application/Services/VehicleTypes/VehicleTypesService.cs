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
using Domain.Services.Translations;
using Domain.Services.VehicleTypes;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.VehicleTypes
{
    public class VehicleTypesService : DictoinaryServiceBase<VehicleType, VehicleTypeDto, VehicleTypeFilterDto>, IVehicleTypesService
    {
        public VehicleTypesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<VehicleTypeDto, VehicleType>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        {
        }

        protected override IQueryable<VehicleType> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Tonnage)
                .Include(i => i.BodyType)
                .Include(i => i.Company);
        }

        public override DetailedValidationResult MapFromDtoToEntity(VehicleType entity, VehicleTypeDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.Name = dto.Name;
            entity.TonnageId = dto.TonnageId?.Value?.ToGuid();
            entity.BodyTypeId = dto.BodyTypeId?.Value?.ToGuid();
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.PalletsCount = dto.PalletsCount.ToInt();
            entity.IsInterregion = dto.IsInterregion;
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            return null;
        }

        protected override DetailedValidationResult ValidateDto(VehicleTypeDto dto, VehicleType entity, bool isConfirmed)
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

            var bodyTypeId = dto.BodyTypeId?.Value.ToGuid();
            var bodyType = bodyTypeId == null ? null : _dataService.GetById<BodyType>(bodyTypeId.Value);
            if (bodyType?.CompanyId != null && bodyType.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.BodyTypeId), "invalidCompanyBodyType".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var currentId = dto.Id.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<VehicleType>(x => x.Name == dto.Name
                                                                                    && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                    && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Name), "VehicleType.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override VehicleType FindByKey(VehicleTypeDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<VehicleType>()
                               .FirstOrDefault(i => i.Name == dto.Name && i.CompanyId == companyId);
        }

        public override IEnumerable<VehicleType> FindByKey(IEnumerable<VehicleTypeDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(VehicleType entity)
        {
            return entity.Name + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(VehicleTypeDto dto)
        {
            return dto.Name + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override VehicleTypeDto MapFromEntityToDto(VehicleType entity)
        {
            return new VehicleTypeDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                TonnageId = entity.Tonnage == null ? null : new LookUpDto(entity.TonnageId.FormatGuid(), entity.Tonnage.ToString()),
                BodyTypeId = entity.BodyType == null ? null : new LookUpDto(entity.BodyTypeId.FormatGuid(), entity.BodyType.ToString()),
                CompanyId = entity.CompanyId == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                PalletsCount = entity.PalletsCount?.ToString(),
                IsInterregion = entity.IsInterregion,
                IsActive = entity.IsActive
            };
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var query = _dataService.GetDbSet<VehicleType>().AsQueryable();
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

            foreach (VehicleType vehicleType in entities)
            {
                yield return new LookUpDto
                {
                    Name = vehicleType.Name,
                    Value = vehicleType.Id.FormatGuid()
                };
            }
        }

        protected override IQueryable<VehicleType> ApplySort(IQueryable<VehicleType> query, FilterFormDto<VehicleTypeFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Name, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<VehicleType> ApplySearch(IQueryable<VehicleType> query, FilterFormDto<VehicleTypeFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<VehicleType>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.PalletsCount.ApplyNumericFilter<VehicleType>(i => i.PalletsCount, ref parameters))
                         .WhereAnd(form.Filter.BodyTypeId.ApplyOptionsFilter<VehicleType, Guid?>(i => i.BodyTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.TonnageId.ApplyOptionsFilter<VehicleType, Guid?>(i => i.TonnageId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<VehicleType, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsInterregion.ApplyBooleanFilter<VehicleType>(i => i.IsInterregion, ref parameters))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<VehicleType>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""VehicleTypes"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var isInt = int.TryParse(search, out int searchInt);

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var bodyTypes = _dataService.GetDbSet<BodyType>()
                    .Where(i => i.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id).ToList();

                var tonnages = _dataService.GetDbSet<Tonnage>()
                    .Where(i => i.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id).ToList();

                query = query.Where(i =>
                       i.Name.ToLower().Contains(search)
                    || bodyTypes.Any(x => x == i.BodyTypeId)
                    || tonnages.Any(x => x == i.TonnageId)
                    || isInt && i.PalletsCount == searchInt);
            }

            return query;
        }

        protected override ExcelMapper<VehicleTypeDto> CreateExcelMapper()
        {
            return new ExcelMapper<VehicleTypeDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.TonnageId, new DictionaryReferenceExcelColumn<Tonnage>(_dataService, _userProvider, x => x.Name))
                .MapColumn(w => w.BodyTypeId, new DictionaryReferenceExcelColumn<BodyType>(_dataService, _userProvider, x => x.Name))
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }

        public override VehicleTypeDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new VehicleTypeDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }
    }
}
