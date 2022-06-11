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
using Domain.Services.FixedDirections;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FixedDirections
{
    public class FixedDirectionsService : DictoinaryServiceBase<FixedDirection, FixedDirectionDto, FixedDirectionFilterDto>, IFixedDirectionsService
    {
        public FixedDirectionsService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<FixedDirectionDto, FixedDirection>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<FixedDirection> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Carrier)
                .Include(i => i.ShippingWarehouse)
                .Include(i => i.DeliveryWarehouse);
        }

        public override DetailedValidationResult MapFromDtoToEntity(FixedDirection entity, FixedDirectionDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.CarrierId = dto.CarrierId?.Value?.ToGuid();
            entity.ShippingWarehouseId = dto.ShippingWarehouseId?.Value?.ToGuid();
            entity.DeliveryWarehouseId = dto.DeliveryWarehouseId?.Value?.ToGuid();
            entity.ShippingCity = dto.ShippingCity?.Value;
            entity.DeliveryCity = dto.DeliveryCity?.Value;
            entity.ShippingRegion = dto.ShippingRegion?.Value;
            entity.DeliveryRegion = dto.DeliveryRegion?.Value;
            entity.Quota = dto.Quota;
            entity.IsActive = dto.IsActive.GetValueOrDefault(true);

            entity.VehicleTypeIds = dto.VehicleTypeIds?.Select(x => x.Value.ToGuid())
                                                       .Where(x => x != null)
                                                       .Select(x => x.Value)
                                                       .ToArray();

            return null;
        }

        public override FixedDirectionDto MapFromEntityToDto(FixedDirection entity)
        {
            var vehicleTypesDict = _dataService.GetDbSet<VehicleType>().ToDictionary(x => x.Id);
            var vehicleTypeDtos = entity.VehicleTypeIds?.Select(x => vehicleTypesDict.ContainsKey(x) ? new LookUpDto(x.FormatGuid(), vehicleTypesDict[x].ToString()) : null);

            return new FixedDirectionDto
            {
                Id = entity.Id.FormatGuid(),
                CarrierId = entity.CarrierId == null ? null : new LookUpDto(entity.CarrierId.FormatGuid(), entity.Carrier.ToString()),
                ShippingWarehouseId = entity.ShippingWarehouseId == null ? null : new LookUpDto(entity.ShippingWarehouseId.FormatGuid(), entity.ShippingWarehouse.ToString()),
                DeliveryWarehouseId = entity.DeliveryWarehouseId == null ? null : new LookUpDto(entity.DeliveryWarehouseId.FormatGuid(), entity.DeliveryWarehouse.ToString()),
                ShippingCity = entity.ShippingCity == null ? null : new LookUpDto(entity.ShippingCity),
                DeliveryCity = entity.DeliveryCity == null ? null : new LookUpDto(entity.DeliveryCity),
                ShippingRegion = entity.ShippingRegion == null ? null : new LookUpDto(entity.ShippingRegion),
                DeliveryRegion = entity.DeliveryRegion == null ? null : new LookUpDto(entity.DeliveryRegion),
                VehicleTypeIds = vehicleTypeDtos,
                Quota = entity.Quota,
                IsActive = entity.IsActive
            };
        }
        protected override DetailedValidationResult ValidateDto(FixedDirectionDto dto, FixedDirection entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var carrierId = dto.CarrierId?.Value.ToGuid();
            var shippingWarehouseId = dto.ShippingWarehouseId?.Value.ToGuid();
            var deliveryWarehouseId = dto.DeliveryWarehouseId?.Value.ToGuid();
            var shippingCity = dto.ShippingCity?.Value;
            var deliveryCity = dto.DeliveryCity?.Value;
            var shippingRegion = dto.ShippingRegion?.Value;
            var deliveryRegion = dto.DeliveryRegion?.Value;
            var hasDuplicates = !result.IsError && _dataService.Any<FixedDirection>(x => x.CarrierId == carrierId
                                                                                && x.ShippingWarehouseId == shippingWarehouseId
                                                                                && x.DeliveryWarehouseId == deliveryWarehouseId
                                                                                && x.ShippingCity == shippingCity
                                                                                && x.DeliveryCity == deliveryCity
                                                                                && x.ShippingRegion == shippingRegion
                                                                                && x.DeliveryRegion == deliveryRegion
                                                                                && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.CarrierId), "FixedDirections.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override IEnumerable<LookUpDto> ForSelect(string fieldName, FilterFormDto<FixedDirectionFilterDto> form)
        {
            if (fieldName.ToLower() == nameof(FixedDirectionDto.VehicleTypeIds).ToLower())
            {
                foreach (var prop in form.Filter.GetType().GetProperties())
                {
                    if (string.Equals(prop.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        prop.SetValue(form.Filter, null);
                    }
                }

                var dbSet = GetDbSet();

                var query = ApplySearch(dbSet, form);
                query = ApplyRestrictions(query);

                var vehicleTypeIds = query.Where(x => x.VehicleTypeIds != null && x.VehicleTypeIds.Length > 0)
                                          .SelectMany(x => x.VehicleTypeIds)
                                          .Distinct()
                                          .ToList();

                var vehicleTypes = _dataService.GetDbSet<VehicleType>()
                                               .Where(x => vehicleTypeIds.Contains(x.Id))
                                               .OrderBy(x => x.Name)
                                               .ToList();

                return vehicleTypes.Select(x => new LookUpDto(x.Id.FormatGuid(), x.ToString())).ToList();
            }
            else
            {
                return base.ForSelect(fieldName, form);
            }
        }

        protected override IQueryable<FixedDirection> ApplySort(IQueryable<FixedDirection> query, FilterFormDto<FixedDirectionFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<FixedDirection> ApplySearch(IQueryable<FixedDirection> query, FilterFormDto<FixedDirectionFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.CarrierId.ApplyOptionsFilter<FixedDirection, Guid?>(i => i.CarrierId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.ShippingWarehouseId.ApplyOptionsFilter<FixedDirection, Guid?>(i => i.ShippingWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.DeliveryWarehouseId.ApplyOptionsFilter<FixedDirection, Guid?>(i => i.DeliveryWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.ShippingCity.ApplyStringFilter<FixedDirection>(i => i.ShippingCity, ref parameters))
                         .WhereAnd(form.Filter.DeliveryCity.ApplyStringFilter<FixedDirection>(i => i.DeliveryCity, ref parameters))
                         .WhereAnd(form.Filter.ShippingRegion.ApplyStringFilter<FixedDirection>(i => i.ShippingRegion, ref parameters))
                         .WhereAnd(form.Filter.DeliveryRegion.ApplyStringFilter<FixedDirection>(i => i.DeliveryRegion, ref parameters))
                         .WhereAnd(form.Filter.VehicleTypeIds.ApplyOptionsArrayFilter<FixedDirection, Guid[]>(i => i.VehicleTypeIds, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.Quota.ApplyNumericFilter<FixedDirection>(i => i.Quota, ref parameters))
                         .WhereAnd(form.Filter.IsActive.ApplyBoolenFilter<FixedDirection>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""FixedDirections"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                decimal? searchDecimal = search.ToDecimal();
                var isDecimal = searchDecimal != null;
                decimal precision = 0.01M;

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var carriers = _dataService.GetDbSet<TransportCompany>()
                    .Where(i => i.Title.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var shippingWarehouses = _dataService.GetDbSet<ShippingWarehouse>()
                    .Where(i => i.WarehouseName.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var deliveryWarehouses = _dataService.GetDbSet<Warehouse>()
                    .Where(i => i.WarehouseName.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var vehicleTypes = _dataService.GetDbSet<VehicleType>()
                    .Where(i => i.Name.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                query = query.Where(i =>
                        carriers.Any(t => t == i.CarrierId)
                        || shippingWarehouses.Any(t => t == i.ShippingWarehouseId)
                        || deliveryWarehouses.Any(t => t == i.DeliveryWarehouseId)
                        || vehicleTypes.Any(t => i.VehicleTypeIds != null && i.VehicleTypeIds.Contains(t))
                        || i.ShippingCity.ToLower().Contains(search)
                        || i.DeliveryCity.ToLower().Contains(search)
                        || i.ShippingRegion.ToLower().Contains(search)
                        || i.DeliveryRegion.ToLower().Contains(search)
                        || isDecimal && i.Quota >= searchDecimal - precision && i.Quota <= searchDecimal + precision);
            }

            return query;
        }

        public override FixedDirection FindByKey(FixedDirectionDto dto)
        {
            var carrierId = dto.CarrierId?.Value.ToGuid();
            var shippingWarehouseId = dto.ShippingWarehouseId?.Value.ToGuid();
            var deliveryWarehouseId = dto.DeliveryWarehouseId?.Value.ToGuid();
            var shippingCity = dto.ShippingCity?.Value;
            var deliveryCity = dto.DeliveryCity?.Value;
            var shippingRegion = dto.ShippingRegion?.Value;
            var deliveryRegion = dto.DeliveryRegion?.Value;
            return _dataService.GetDbSet<FixedDirection>()
                               .FirstOrDefault(i => i.CarrierId == carrierId
                                                && i.ShippingWarehouseId == shippingWarehouseId
                                                && i.DeliveryWarehouseId == deliveryWarehouseId
                                                && i.ShippingCity == shippingCity
                                                && i.DeliveryCity == deliveryCity
                                                && i.ShippingRegion == shippingRegion
                                                && i.DeliveryRegion == deliveryRegion);
        }

        public override IEnumerable<FixedDirection> FindByKey(IEnumerable<FixedDirectionDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(FixedDirection entity)
        {
            return string.Join('#',
                entity.CarrierId.FormatGuid() ?? string.Empty,
                entity.ShippingWarehouseId.FormatGuid() ?? string.Empty,
                entity.DeliveryWarehouseId.FormatGuid() ?? string.Empty,
                entity.ShippingCity ?? string.Empty,
                entity.DeliveryCity ?? string.Empty,
                entity.ShippingRegion ?? string.Empty,
                entity.DeliveryRegion ?? string.Empty);
        }

        public override string GetDtoKey(FixedDirectionDto dto)
        {
            return string.Join('#',
                dto.CarrierId?.Value ?? string.Empty,
                dto.ShippingWarehouseId?.Value ?? string.Empty,
                dto.DeliveryWarehouseId?.Value ?? string.Empty,
                dto.ShippingCity?.Value ?? string.Empty,
                dto.DeliveryCity?.Value ?? string.Empty,
                dto.ShippingRegion?.Value ?? string.Empty,
                dto.DeliveryRegion?.Value ?? string.Empty);
        }

        public override FixedDirectionDto GetDefaults()
        {
            return new FixedDirectionDto
            {
                IsActive = true
            };
        }

        protected override ExcelMapper<FixedDirectionDto> CreateExcelMapper()
        {
            return new ExcelMapper<FixedDirectionDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CarrierId, new DictionaryReferenceExcelColumn<TransportCompany>(_dataService, _userProvider, x => x.Title))
                .MapColumn(w => w.ShippingWarehouseId, new DictionaryReferenceExcelColumn<ShippingWarehouse>(_dataService, _userProvider, x => x.WarehouseName))
                .MapColumn(w => w.DeliveryWarehouseId, new DictionaryReferenceExcelColumn<Warehouse>(_dataService, _userProvider, x => x.WarehouseName))
                .MapColumn(w => w.VehicleTypeIds, new ArrayExcelColumn<VehicleType>(_dataService, x => x.Name));
        }
    }
}
