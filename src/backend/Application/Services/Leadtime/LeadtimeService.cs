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
using Domain.Services.Leadtime;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Leadtime
{
    public class LeadtimeService : DictoinaryServiceBase<LeadTime, LeadtimeDto, LeadtimeFilterDto>, ILeadtimeService
    {
        private readonly IMapper _mapper;

        public LeadtimeService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<LeadtimeDto, LeadTime>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        {
            _mapper = ConfigureMapper().CreateMapper();
        }

        protected override DetailedValidationResult ValidateDto(LeadtimeDto dto, LeadTime entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var duplicateLeadtime = _dataService.GetDbSet<LeadTime>().Any(x =>
            x.Id != dto.Id.ToGuid()
            && dto.ClientName != null && x.ClientName == dto.ClientName.Value
            && dto.DeliveryAddress != null && x.DeliveryAddress == dto.DeliveryAddress.Value
            && dto.ShippingWarehouseId !=null && x.ShippingWarehouseId == dto.ShippingWarehouseId.Value.ToGuid()
            );

            if (duplicateLeadtime)
            {
                result.AddError(nameof(entity), "leadtime.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        protected override ExcelMapper<LeadtimeDto> CreateExcelMapper()
        {
            return new ExcelMapper<LeadtimeDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.ShippingWarehouseId, new DictionaryReferenceExcelColumn<ShippingWarehouse>(_dataService, _userProvider, x => x.WarehouseName));
        }

        protected override IQueryable<LeadTime> ApplySearch(IQueryable<LeadTime> query, FilterFormDto<LeadtimeFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.ClientName.ApplyOptionsFilter<LeadTime, string>(i => i.ClientName, ref parameters))
                         .WhereAnd(form.Filter.DeliveryAddress.ApplyOptionsFilter<LeadTime, string>(i => i.DeliveryAddress, ref parameters))
                         .WhereAnd(form.Filter.ShippingWarehouseId.ApplyOptionsFilter<LeadTime, Guid?>(i => i.ShippingWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.LeadtimeDays.ApplyNumericFilter<LeadTime>(i => i.LeadtimeDays, ref parameters));

            string sql = $@"SELECT * FROM ""LeadTimes"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var isInt = int.TryParse(search, out int searchInt);

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var shippingWarehouses = this._dataService.GetDbSet<ShippingWarehouse>()
                    .Where(i => i.WarehouseName.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                query = query.Where(i =>
                    !string.IsNullOrEmpty(i.ClientName) && i.ClientName.ToLower().Contains(search)
                    || !string.IsNullOrEmpty(i.DeliveryAddress) && i.DeliveryAddress.ToLower().Contains(search)
                    || shippingWarehouses.Any(t => t == i.ShippingWarehouseId)
                    || isInt && i.LeadtimeDays == searchInt);
            }
            return query;
        }

        public override DetailedValidationResult MapFromDtoToEntity(LeadTime entity, LeadtimeDto dto)
        {
            _mapper.Map(dto, entity);
            return null;
        }

        public override LeadtimeDto MapFromEntityToDto(LeadTime entity)
        {
            if (entity == null)
            {
                return null;
            }
            var result = _mapper.Map<LeadtimeDto>(entity);
            return result;
        }

        protected override IQueryable<LeadTime> GetDbSet()
        {
            return _dataService.GetDbSet<LeadTime>().Include(x => x.ShippingWarehouse);
        }

        private MapperConfiguration ConfigureMapper()
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;
            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LeadTime, LeadtimeDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()))
                    .ForMember(t => t.ClientName, e => e.MapFrom((s) => s.ClientName == null ? null : new LookUpDto(s.ClientName)))
                    .ForMember(t => t.DeliveryAddress, e => e.MapFrom((s) => s.DeliveryAddress == null ? null : new LookUpDto(s.DeliveryAddress)))
                    .ForMember(t => t.ShippingWarehouseId, e => e.MapFrom((s) => s.ShippingWarehouse == null ? null : new LookUpDto(s.ShippingWarehouse.Id.FormatGuid(), s.ShippingWarehouse.WarehouseName)));

                cfg.CreateMap<LeadtimeDto, LeadTime>()
                    .ForMember(t => t.Id, e => e.Ignore())
                    .ForMember(t => t.ClientName, e => e.MapFrom((s) => s.ClientName == null ? null : s.ClientName.Value))
                    .ForMember(t => t.DeliveryAddress, e => e.MapFrom((s) => s.DeliveryAddress == null ? null : s.DeliveryAddress.Value))
                    .ForMember(t => t.ShippingWarehouseId, e => e.MapFrom((s) => s.ShippingWarehouseId == null ? null : s.ShippingWarehouseId.Value.ToGuid()));
            });
            return result;
        }
    }
}