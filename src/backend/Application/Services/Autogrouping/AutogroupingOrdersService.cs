using Application.Extensions;
using AutoMapper;
using DAL.Extensions;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.AppConfiguration;
using Domain.Services.Autogrouping;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Application.Services.Autogrouping
{
    public class AutogroupingOrdersService : IAutogroupingOrdersService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IFieldDispatcherService _fieldDispatcherService;
        private readonly IMapper _mapper;

        public AutogroupingOrdersService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IFieldDispatcherService fieldDispatcherService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _fieldDispatcherService = fieldDispatcherService;
            _mapper = ConfigureMapper().CreateMapper();
        }

        public SearchResult<AutogroupingOrderDto> Search(Guid runId, Guid? parentId, FilterFormDto<AutogroupingOrdersFilterDto> dto)
        {
            var dbSet = GetDbSet();

            var query = ApplySearchForm(dbSet, runId, parentId, dto);

            if (dto.Take == 0)
                dto.Take = 1000;

            var totalCount = query.Count();
            var entities = ApplySort(query, dto)
                .Skip(dto.Skip)
                .Take(dto.Take).ToList();

            var a = new SearchResult<AutogroupingOrderDto>
            {
                TotalCount = totalCount,
                Items = entities.Select(x => _mapper.Map<AutogroupingOrderDto>(x)).ToList()
            };

            return a;
        }

        public IEnumerable<LookUpDto> ForSelect(Guid runId, Guid? parentId, string fieldName, FilterFormDto<AutogroupingOrdersFilterDto> form)
        {
            foreach (var prop in form.Filter.GetType().GetProperties())
            {
                if (string.Equals(prop.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    prop.SetValue(form.Filter, null);
                }
            }

            var user = _userProvider.GetCurrentUser();

            var dbSet = GetDbSet();
            var query = ApplySearchForm(dbSet, runId, parentId, form);

            var propertyInfo = typeof(AutogroupingOrder).GetProperties()
                .FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());
            var refType = GetReferenceType(propertyInfo);

            var fields = _fieldDispatcherService.GetDtoFields<AutogroupingOrderDto>();
            var field = fields.FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());

            IEnumerable<LookUpDto> result;

            if (refType != null)
            {
                result = GetReferencedValues(query, refType, fieldName);
            }
            else if (field.FieldType == FieldType.State)
            {
                result = GetStateValues(query, propertyInfo);
            }
            else
            {
                result = GetSelectValues(query, propertyInfo, field.ShowRawReferenceValue);
            }

            if (field.EmptyValueOptions != EmptyValueOptions.NotAllowed)
            {
                var empty = new LookUpDto
                {
                    Name = "emptyValue".Translate(user.Language),
                    Value = LookUpDto.EmptyValue,
                    IsFilterOnly = field.EmptyValueOptions == EmptyValueOptions.FilterOnly
                };

                result = new[] { empty }.Union(result);
            }

            return result;
        }

        public UserConfigurationGridItem GetPreviewConfiguration()
        {
            var columns = new List<UserConfigurationGridColumn>();
            var fields = _fieldDispatcherService.GetDtoFields<AutogroupingOrderDto>();

            foreach (var field in fields.OrderBy(f => f.OrderNumber))
            {
                if (string.IsNullOrEmpty(field.ReferenceSource))
                {
                    columns.Add(new UserConfigurationGridColumn(field));
                }
                else
                {
                    columns.Add(new UserConfigurationGridColumnWhitchSource(field));
                }
            }

            return new UserConfigurationGridItem
            {
                Columns = columns
            };
        }

        private IQueryable<AutogroupingOrder> GetDbSet()
        {
            return _dataService.GetDbSet<AutogroupingOrder>()
                    .Include(x => x.ShippingWarehouse)
                    .Include(x => x.DeliveryWarehouse)
                    .Include(x => x.BodyType)
                    .Include(x => x.VehicleType);
        }

        private IQueryable<AutogroupingOrder> ApplySort(IQueryable<AutogroupingOrder> query, FilterFormDto<AutogroupingOrdersFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.ShippingDate.Value.Date, true)
                .DefaultOrderBy(i => i.DeliveryDate.Value.Date, true)
                .DefaultOrderBy(i => i.CreatedAt, true);
        }

        private IQueryable<AutogroupingOrder> ApplySearchForm(IQueryable<AutogroupingOrder> query, Guid runId, Guid? parentId, FilterFormDto<AutogroupingOrdersFilterDto> searchForm)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(searchForm.Filter.BodyTypeId.ApplyOptionsFilter<AutogroupingOrder, Guid?>(i => i.BodyTypeId, ref parameters, i => i.ToGuid()))
                         .WhereAnd(searchForm.Filter.DeliveryDate.ApplyDateRangeFilter<AutogroupingOrder>(i => i.DeliveryDate, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryRegion.ApplyStringFilter<AutogroupingOrder>(i => i.DeliveryRegion, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryTime.ApplyTimeRangeFilter<AutogroupingOrder>(i => i.DeliveryTime, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryWarehouseId.ApplyOptionsFilter<AutogroupingOrder, Guid?>(i => i.DeliveryWarehouseId, ref parameters, i => i.ToGuid()))
                         .WhereAnd(searchForm.Filter.OrderNumber.ApplyStringFilter<AutogroupingOrder>(i => i.OrderNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.PalletsCount.ApplyNumericFilter<AutogroupingOrder>(i => i.PalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingDate.ApplyDateRangeFilter<AutogroupingOrder>(i => i.ShippingDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingWarehouseId.ApplyOptionsFilter<AutogroupingOrder, Guid?>(i => i.ShippingWarehouseId, ref parameters, i => i.ToGuid()))
                         .WhereAnd(searchForm.Filter.VehicleTypeId.ApplyOptionsFilter<AutogroupingOrder, Guid?>(i => i.VehicleTypeId, ref parameters, i => i.ToGuid()))
                         .WhereAnd(searchForm.Filter.WeightKg.ApplyNumericFilter<AutogroupingOrder>(i => i.WeightKg, ref parameters))
                         .WhereAnd(searchForm.Filter.Errors.ApplyStringFilter<AutogroupingOrder>(i => i.Errors, ref parameters))
                         .WhereAnd($@"""{nameof(AutogroupingOrder.RunId)}"" = '{runId.ToString("D")}'");

            if (parentId == null)
            {
                where = where.WhereAnd($@"""{nameof(AutogroupingOrder.AutogroupingShippingId)}"" is null");
            }
            else
            {
                where = where.WhereAnd($@"""{nameof(AutogroupingOrder.AutogroupingShippingId)}"" = '{parentId.Value.ToString("D")}'");
            }

            string sql = $@"SELECT * FROM ""AutogroupingOrders"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            // Apply Search
            return ApplySearch(query, searchForm?.Filter?.Search);
        }

        private IQueryable<AutogroupingOrder> ApplySearch(IQueryable<AutogroupingOrder> query, string search)
        {
            if (string.IsNullOrEmpty(search)) return query;

            search = search.ToLower().Trim();

            var isInt = int.TryParse(search, out int searchInt);

            decimal? searchDecimal = search.ToDecimal();
            var isDecimal = searchDecimal != null;
            decimal precision = 0.01M;

            var shippingWarehouses = _dataService.GetDbSet<ShippingWarehouse>().Where(i => i.WarehouseName.ToLower().Contains(search));
            var deliveryWarehouses = _dataService.GetDbSet<Warehouse>().Where(i => i.WarehouseName.ToLower().Contains(search));
            var bodyTypes = _dataService.GetDbSet<BodyType>().Where(i => i.Name.ToLower().Contains(search));
            var vehicleTypes = _dataService.GetDbSet<VehicleType>().Where(i => i.Name.ToLower().Contains(search));

            return query.Where(i =>
                   !string.IsNullOrEmpty(i.OrderNumber) && i.OrderNumber.ToLower().Contains(search)
                || !string.IsNullOrEmpty(i.DeliveryRegion) && i.DeliveryRegion.ToLower().Contains(search)
                || !string.IsNullOrEmpty(i.Errors) && i.Errors.ToLower().Contains(search)

                || shippingWarehouses.Any(p => p.Id == i.ShippingWarehouseId)
                || deliveryWarehouses.Any(p => p.Id == i.DeliveryWarehouseId)
                || bodyTypes.Any(p => p.Id == i.BodyTypeId)
                || vehicleTypes.Any(p => p.Id == i.VehicleTypeId)

                || isInt && i.PalletsCount == searchInt

                || isDecimal && i.WeightKg != null && Math.Round(i.WeightKg.Value, 2) >= searchDecimal - precision && Math.Round(i.WeightKg.Value, 2) <= searchDecimal + precision

                || i.ShippingDate.HasValue && i.ShippingDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || i.DeliveryDate.HasValue && i.DeliveryDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || i.DeliveryTime.HasValue && i.DeliveryTime.Value.SqlFormat(StringExt.SqlTimeFormat).Contains(search)
                );
        }

        private MapperConfiguration ConfigureMapper()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AutogroupingOrder, AutogroupingOrderDto>()
                    .ForMember(x => x.ShippingDate, e => e.MapFrom((s, t) => s.ShippingDate.FormatDate()))
                    .ForMember(x => x.DeliveryDate, e => e.MapFrom((s, t) => s.DeliveryDate.FormatDate()))
                    .ForMember(x => x.ShippingWarehouseId, e => e.Condition(s => s.ShippingWarehouseId != null))
                    .ForMember(x => x.ShippingWarehouseId, e => e.MapFrom((s, t) => new LookUpDto(s.ShippingWarehouseId.FormatGuid(), s.ShippingWarehouse?.ToString())))
                    .ForMember(x => x.DeliveryWarehouseId, e => e.Condition(s => s.DeliveryWarehouseId != null))
                    .ForMember(x => x.DeliveryWarehouseId, e => e.MapFrom((s, t) => new LookUpDto(s.DeliveryWarehouseId.FormatGuid(), s.DeliveryWarehouse?.ToString())))
                    .ForMember(x => x.BodyTypeId, e => e.Condition(s => s.BodyTypeId != null))
                    .ForMember(x => x.BodyTypeId, e => e.MapFrom((s, t) => new LookUpDto(s.BodyTypeId.FormatGuid(), s.BodyType?.ToString())));
            });

            return result;
        }

        private Type GetReferenceType(PropertyInfo property)
        {
            var attr = property.GetCustomAttribute<ReferenceTypeAttribute>();
            return attr?.Type;
        }

        private List<LookUpDto> GetReferencedValues(IQueryable<AutogroupingOrder> query, Type refType, string field)
        {
            var ids = query.SelectField(field).Distinct();

            var result = _dataService.QueryAs<IPersistable>(refType)
                .Where(i => ids.Contains(i.Id))
                .ToList();

            return result.Select(i => new LookUpDto
            {
                Name = i.ToString(),
                Value = i.Id.FormatGuid()
            })
            .ToList();
        }

        private List<ColoredLookUpDto> GetStateValues(IQueryable<AutogroupingOrder> query, PropertyInfo propertyInfo)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var getMethod = typeof(Domain.Extensions.Extensions)
               .GetMethod(nameof(Domain.Extensions.Extensions.GetColor))
               .MakeGenericMethod(propertyInfo.PropertyType);

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct()
                .Select(i => new ColoredLookUpDto
                {
                    Name = i.FormatEnum(),
                    Value = i.FormatEnum(),
                    Color = getMethod.Invoke(i, new object[] { i }).FormatEnum()
                })
                .ToList();

            return result;
        }

        List<LookUpDto> GetSelectValues(IQueryable<AutogroupingOrder> query, PropertyInfo propertyInfo, bool showRawName)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct()
                .ToList();

            return result.Select(i => new LookUpDto
            {
                Name = showRawName ? i.ToString() : i.FormatEnum().Translate(lang),
                Value = i.ToString()
            })
            .ToList();
        }
    }
}
