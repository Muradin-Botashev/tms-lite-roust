using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Services.Warehouses;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Warehouses
{
    public class WarehousesService : DictoinaryServiceBase<Warehouse, WarehouseDto, WarehouseFilterDto>, IWarehousesService
    {
        private readonly IMapper _mapper;

        public WarehousesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService, 
            IEnumerable<IValidationRule<WarehouseDto, Warehouse>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        {
            _mapper = ConfigureMapper().CreateMapper();
        }

        public WarehouseDto GetBySoldTo(string soldToNumber)
        {
            var entity = GetDbSet().Where(x => x.SoldToNumber == soldToNumber).FirstOrDefault();
            return MapFromEntityToDto(entity);
        }

        protected override IQueryable<Warehouse> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.PickingType)
                .Include(i => i.Company);
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<Warehouse>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(x => x.IsActive)
                                .OrderBy(x => x.WarehouseName)
                                .ToList();

            foreach (var entity in entities)
            {
                yield return new WarehouseSelectDto
                {
                    Name = entity.WarehouseName,
                    Value = entity.Id.FormatGuid(),
                    WarehouseName = new LookUpDto(entity.WarehouseName),
                    Address = entity.Address,
                    City = entity.City,
                    Region = entity.Region,
                    LeadtimeDays = entity.LeadtimeDays,
                    PickingTypeId = entity.PickingTypeId.FormatGuid()
                };
            }
        }

        public override Warehouse FindByKey(WarehouseDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<Warehouse>()
                               .FirstOrDefault(x => x.WarehouseName == dto.WarehouseName && x.CompanyId == companyId);
        }

        public override IEnumerable<Warehouse> FindByKey(IEnumerable<WarehouseDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(Warehouse entity)
        {
            return entity.WarehouseName + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(WarehouseDto dto)
        {
            return dto.WarehouseName + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override DetailedValidationResult MapFromDtoToEntity(Warehouse entity, WarehouseDto dto)
        {
            _mapper.Map(dto, entity);
            return null;
        }

        public override WarehouseDto MapFromEntityToDto(Warehouse entity)
        {
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<WarehouseDto>(entity);
        }

        protected override ExcelMapper<WarehouseDto> CreateExcelMapper()
        {
            string lang = _userProvider.GetCurrentUser()?.Language;
            return new ExcelMapper<WarehouseDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.PickingTypeId, new DictionaryReferenceExcelColumn<PickingType>(_dataService, _userProvider, x => x.Name))
                .MapColumn(w => w.DeliveryType, new EnumExcelColumn<DeliveryType>(lang))
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }

        protected override IQueryable<Warehouse> ApplySort(IQueryable<Warehouse> query, FilterFormDto<WarehouseFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.WarehouseName, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<Warehouse> ApplySearch(IQueryable<Warehouse> query, FilterFormDto<WarehouseFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.WarehouseName.ApplyStringFilter<Warehouse>(i => i.WarehouseName, ref parameters))
                         .WhereAnd(form.Filter.Client.ApplyStringFilter<Warehouse>(i => i.Client, ref parameters))
                         .WhereAnd(form.Filter.Address.ApplyStringFilter<Warehouse>(i => i.Address, ref parameters))
                         .WhereAnd(form.Filter.City.ApplyStringFilter<Warehouse>(i => i.City, ref parameters))
                         .WhereAnd(form.Filter.Region.ApplyStringFilter<Warehouse>(i => i.Region, ref parameters))
                         .WhereAnd(form.Filter.SoldToNumber.ApplyStringFilter<Warehouse>(i => i.SoldToNumber, ref parameters))
                         .WhereAnd(form.Filter.PickingFeatures.ApplyStringFilter<Warehouse>(i => i.PickingFeatures, ref parameters))
                         .WhereAnd(form.Filter.LeadtimeDays.ApplyNumericFilter<Warehouse>(i => i.LeadtimeDays, ref parameters))
                         .WhereAnd(form.Filter.PickingTypeId.ApplyOptionsFilter<Warehouse, Guid?>(i => i.PickingTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<Warehouse, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.DeliveryType.ApplyEnumFilter<Warehouse, DeliveryType>(i => i.DeliveryType, ref parameters))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<Warehouse>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""Warehouses"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var isInt = int.TryParse(search, out int searchInt);

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var pickingTypes = _dataService.GetDbSet<PickingType>()
                    .Where(i => i.Name.ToLower().Contains(search.ToLower())
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id).ToList();

                var deliveryTypeNames = Enum.GetNames(typeof(DeliveryType)).Select(i => i.ToLower());

                var deliveryTypes = _dataService.GetDbSet<Translation>()
                    .Where(i => deliveryTypeNames.Contains(i.Name.ToLower()))
                    .WhereTranslation(search)
                    .Select(i => i.Name.ToEnum<DeliveryType>())
                    .ToList();

                query = query.Where(i =>
                           !string.IsNullOrEmpty(i.WarehouseName) && i.WarehouseName.ToLower().Contains(search)
                        || !string.IsNullOrEmpty(i.Client) && i.Client.ToLower().Contains(search)
                        || !string.IsNullOrEmpty(i.SoldToNumber) && i.SoldToNumber.ToLower().Contains(search)
                        || !string.IsNullOrEmpty(i.Region) && i.Region.ToLower().Contains(search)
                        || !string.IsNullOrEmpty(i.City) && i.City.ToLower().Contains(search)
                        || !string.IsNullOrEmpty(i.Address) && i.Address.ToLower().Contains(search)
                        || !string.IsNullOrEmpty(i.PickingFeatures) && i.PickingFeatures.ToLower().Contains(search)
                        || i.PickingTypeId != null && pickingTypes.Any(t => t == i.PickingTypeId)
                        || i.DeliveryType != null && deliveryTypes.Contains(i.DeliveryType)
                        || isInt && i.LeadtimeDays == searchInt
                    );
            }

            return query;
        }

        protected override DetailedValidationResult ValidateDto(WarehouseDto dto, Warehouse entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentCompanyId = dto.CompanyId?.Value.ToGuid();

            var pickingTypeId = dto.PickingTypeId?.Value.ToGuid();
            var pickingType = pickingTypeId == null ? null : _dataService.GetById<PickingType>(pickingTypeId.Value);
            if (pickingType?.CompanyId != null && pickingType.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.PickingTypeId), "invalidCompanyPickingType".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var currentId = dto.Id.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<Warehouse>(x => x.WarehouseName == dto.WarehouseName
                                                                                    && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                    && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.WarehouseName), "Warehouse.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        private MapperConfiguration ConfigureMapper()
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;
            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Warehouse, WarehouseDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()))
                    //.ForMember(t => t.PickingTypeId, e => e.Condition(s => s.PickingTypeId != null))
                    .ForMember(t => t.PickingTypeId, e => e.MapFrom((s) => s.PickingType != null ? new LookUpDto 
                    {
                        Value = s.PickingTypeId.FormatGuid(),
                        Name = s.PickingType.Name
                    } : null))
                    .ForMember(t => t.DeliveryType, e => e.MapFrom((s, t) => s.DeliveryType == null ? null : s.DeliveryType.GetEnumLookup(lang)))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s, t) => s.CompanyId == null ? null : new LookUpDto(s.CompanyId.FormatGuid(), s.Company.ToString())));

                cfg.CreateMap<WarehouseDto, Warehouse>()
                    .ForMember(t => t.Id, e => e.Ignore())
                    .ForMember(t => t.PickingTypeId, e => e.Condition((s) => s.PickingTypeId != null))
                    .ForMember(t => t.PickingTypeId, e => e.MapFrom((s) => s.PickingTypeId.Value.ToGuid()))
                    //.ForMember(t => t.CompanyId, e => e.Condition((s) => s.CompanyId != null))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s) => s.CompanyId != null ? s.CompanyId.Value.ToGuid() : user.CompanyId))
                    .ForMember(t => t.DeliveryType, e => e.Condition((s) => s.DeliveryType != null && !string.IsNullOrEmpty(s.DeliveryType.Value)))
                    .ForMember(t => t.DeliveryType, e => e.MapFrom((s) => MapFromStateDto<DeliveryType>(s.DeliveryType.Value)));
            });
            return result;
        }

        public override WarehouseDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new WarehouseDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }
    }
}