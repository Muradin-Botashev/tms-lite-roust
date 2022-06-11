using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.ShippingWarehouses;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ShippingWarehouses
{
    public class ShippingWarehousesService : DictoinaryServiceBase<ShippingWarehouse, ShippingWarehouseDto, ShippingWarehouseFilterDto>, IShippingWarehousesService
    {
        private readonly IMapper _mapper;


        public ShippingWarehousesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<ShippingWarehouseDto, ShippingWarehouse>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        {
            _mapper = ConfigureMapper().CreateMapper();
        }

        protected override IQueryable<ShippingWarehouse> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public ShippingWarehouse GetByCode(string code)
        {
            return _dataService.GetDbSet<ShippingWarehouse>().FirstOrDefault(x => x.Code == code && x.IsActive);
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<ShippingWarehouse>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.Where(x => x.IsActive)
                                .OrderBy(x => x.WarehouseName)
                                .ToList();

            foreach (var entity in entities)
            {
                yield return new ShippingWarehouseSelectDto
                {
                    Name = entity.WarehouseName,
                    Value = entity.Id.FormatGuid(),
                    Address = entity.Address,
                    City = entity.City,
                    Region = entity.Region
                };
            }
        }

        public override ShippingWarehouse FindByKey(ShippingWarehouseDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<ShippingWarehouse>()
                               .FirstOrDefault(x => x.WarehouseName == dto.WarehouseName && x.CompanyId == companyId);
        }

        public override IEnumerable<ShippingWarehouse> FindByKey(IEnumerable<ShippingWarehouseDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(ShippingWarehouse entity)
        {
            return entity.WarehouseName + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(ShippingWarehouseDto dto)
        {
            return dto.WarehouseName + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        public override DetailedValidationResult MapFromDtoToEntity(ShippingWarehouse entity, ShippingWarehouseDto dto)
        {
            this._mapper.Map(dto, entity);

            return null;
        }

        public override ShippingWarehouseDto MapFromEntityToDto(ShippingWarehouse entity)
        {
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<ShippingWarehouseDto>(entity);
        }

        protected override IQueryable<ShippingWarehouse> ApplySort(IQueryable<ShippingWarehouse> query, FilterFormDto<ShippingWarehouseFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.WarehouseName, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<ShippingWarehouse> ApplySearch(IQueryable<ShippingWarehouse> query, FilterFormDto<ShippingWarehouseFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.WarehouseName.ApplyStringFilter<ShippingWarehouse>(i => i.WarehouseName, ref parameters))
                         .WhereAnd(form.Filter.Address.ApplyStringFilter<ShippingWarehouse>(i => i.Address, ref parameters))
                         .WhereAnd(form.Filter.City.ApplyStringFilter<ShippingWarehouse>(i => i.City, ref parameters))
                         .WhereAnd(form.Filter.Region.ApplyStringFilter<ShippingWarehouse>(i => i.Region, ref parameters))
                         .WhereAnd(form.Filter.Code.ApplyStringFilter<ShippingWarehouse>(i => i.Code, ref parameters))
                         .WhereAnd(form.Filter.PoolingConsolidationId.ApplyStringFilter<ShippingWarehouse>(i => i.PoolingConsolidationId, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<ShippingWarehouse, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<ShippingWarehouse>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""ShippingWarehouses"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                query = query.Where(i =>
                           i.WarehouseName.ToLower().Contains(search)
                        || i.Address.ToLower().Contains(search)
                        || i.City.ToLower().Contains(search)
                        || i.Region.ToLower().Contains(search)
                        || i.Code.ToLower().Contains(search)
                        || i.PoolingConsolidationId.ToLower().Contains(search));
            }

            return query;
        }

        protected override DetailedValidationResult ValidateDto(ShippingWarehouseDto dto, ShippingWarehouse entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<ShippingWarehouse>(x => x.WarehouseName == dto.WarehouseName
                                                                                            && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                            && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.WarehouseName), "ShippingWarehouse.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        private MapperConfiguration ConfigureMapper()
        {
            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShippingWarehouse, ShippingWarehouseDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s, t) => s.CompanyId == null ? null : new LookUpDto(s.CompanyId.FormatGuid(), s.Company.ToString())));

                cfg.CreateMap<ShippingWarehouseDto, ShippingWarehouse>()
                    .ForMember(t => t.Id, e => e.Ignore())
                    .ForMember(t => t.CompanyId, e => e.Condition((s) => s.CompanyId != null))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s) => s.CompanyId.Value.ToGuid()));
            });
            return result;
        }

        public override ShippingWarehouseDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new ShippingWarehouseDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString()),
                IsActive = true
            };
        }

        protected override ExcelMapper<ShippingWarehouseDto> CreateExcelMapper()
        {
            return new ExcelMapper<ShippingWarehouseDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}
