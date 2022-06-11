using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using DAL.Extensions;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Tariffs;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Application.Services.Tariffs
{
    public class TariffsService : DictoinaryServiceBase<Tariff, TariffDto, TariffFilterDto>, ITariffsService
    {
        private Dictionary<Guid, ShippingWarehouse> _shippingWarehouseCache = null;
        private Dictionary<Guid, Warehouse> _warehouseCache = null;
        private Dictionary<Guid, TransportCompany> _carrierCache = null;
        private Dictionary<Guid, BodyType> _bodyTypeCache = null;
        private Dictionary<Guid, VehicleType> _vehicleTypeCache = null;
        private List<Tariff> _tariffsCache = null;

        public TariffsService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<TariffDto, Tariff>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        {
        }

        protected override IQueryable<Tariff> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.ShippingWarehouse)
                .Include(i => i.DeliveryWarehouse)
                .Include(i => i.Carrier)
                .Include(i => i.Company)
                .Include(i => i.VehicleType)
                .Include(i => i.BodyType);
        }

        public override DetailedValidationResult MapFromDtoToEntity(Tariff entity, TariffDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.ShipmentRegion = dto.ShipmentRegion?.Value;
            entity.DeliveryRegion = dto.DeliveryRegion?.Value;
            entity.ShipmentCity = dto.ShipmentCity?.Value;
            entity.DeliveryCity = dto.DeliveryCity?.Value;
            entity.ShippingWarehouseId = dto.ShippingWarehouseId?.Value?.ToGuid();
            entity.DeliveryWarehouseId = dto.DeliveryWarehouseId?.Value?.ToGuid();
            entity.TarifficationType = string.IsNullOrEmpty(dto.TarifficationType?.Value) ? (TarifficationType?)null : MapFromStateDto<TarifficationType>(dto.TarifficationType.Value);
            entity.VehicleTypeId = dto.VehicleTypeId?.Value?.ToGuid();
            entity.CarrierId = dto.CarrierId?.Value?.ToGuid();
            entity.BodyTypeId = dto.BodyTypeId?.Value?.ToGuid();
            entity.CompanyId = dto.CompanyId?.Value?.ToGuid();
            entity.WinterAllowance = dto.WinterAllowance.ToDecimal();
            entity.EffectiveDate = dto.EffectiveDate.ToDate();
            entity.ExpirationDate = dto.ExpirationDate.ToDate();
            entity.StartWinterPeriod = dto.StartWinterPeriod.ToDate();
            entity.EndWinterPeriod = dto.EndWinterPeriod.ToDate();

            entity.FtlRate = dto.FtlRate;
            entity.LtlRate1 = dto.LtlRate1;
            entity.LtlRate2 = dto.LtlRate2;
            entity.LtlRate3 = dto.LtlRate3;
            entity.LtlRate4 = dto.LtlRate4;
            entity.LtlRate5 = dto.LtlRate5;
            entity.LtlRate6 = dto.LtlRate6;
            entity.LtlRate7 = dto.LtlRate7;
            entity.LtlRate8 = dto.LtlRate8;
            entity.LtlRate9 = dto.LtlRate9;
            entity.LtlRate10 = dto.LtlRate10;
            entity.LtlRate11 = dto.LtlRate11;
            entity.LtlRate12 = dto.LtlRate12;
            entity.LtlRate13 = dto.LtlRate13;
            entity.LtlRate14 = dto.LtlRate14;
            entity.LtlRate15 = dto.LtlRate15;
            entity.LtlRate16 = dto.LtlRate16;
            entity.LtlRate17 = dto.LtlRate17;
            entity.LtlRate18 = dto.LtlRate18;
            entity.LtlRate19 = dto.LtlRate19;
            entity.LtlRate20 = dto.LtlRate20;
            entity.LtlRate21 = dto.LtlRate21;
            entity.LtlRate22 = dto.LtlRate22;
            entity.LtlRate23 = dto.LtlRate23;
            entity.LtlRate24 = dto.LtlRate24;
            entity.LtlRate25 = dto.LtlRate25;
            entity.LtlRate26 = dto.LtlRate26;
            entity.LtlRate27 = dto.LtlRate27;
            entity.LtlRate28 = dto.LtlRate28;
            entity.LtlRate29 = dto.LtlRate29;
            entity.LtlRate30 = dto.LtlRate30;
            entity.LtlRate31 = dto.LtlRate31;
            entity.LtlRate32 = dto.LtlRate32;
            entity.LtlRate33 = dto.LtlRate33;
            entity.ExtraPointRate = dto.ExtraPointRate;
            entity.PoolingPalletRate = dto.PoolingPalletRate;

            return null;
        }

        protected override DetailedValidationResult ValidateDto(TariffDto dto, Tariff entity, bool isConfirmed)
        {
            EnsureCache();

            var lang = _userProvider.GetCurrentUser()?.Language;

            var result = base.ValidateDto(dto, entity, isConfirmed);

            var currentCompanyId = dto.CompanyId?.Value.ToGuid();

            bool isAllEmpty = string.IsNullOrEmpty(dto.ShipmentRegion?.Value) && string.IsNullOrEmpty(dto.DeliveryRegion?.Value)
                            && string.IsNullOrEmpty(dto.ShipmentCity?.Value) && string.IsNullOrEmpty(dto.DeliveryCity?.Value)
                            && string.IsNullOrEmpty(dto.ShippingWarehouseId?.Value) && string.IsNullOrEmpty(dto.DeliveryWarehouseId?.Value);

            if (isAllEmpty || (string.IsNullOrEmpty(dto.ShipmentRegion?.Value) && !string.IsNullOrEmpty(dto.DeliveryRegion?.Value)))
            {
                result.AddError(nameof(dto.ShipmentRegion), "Tariff.IncompleteDirection".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            if (isAllEmpty || (!string.IsNullOrEmpty(dto.ShipmentRegion?.Value) && string.IsNullOrEmpty(dto.DeliveryRegion?.Value)))
            {
                result.AddError(nameof(dto.DeliveryRegion), "Tariff.IncompleteDirection".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            if (isAllEmpty || (string.IsNullOrEmpty(dto.ShipmentCity?.Value) && !string.IsNullOrEmpty(dto.DeliveryCity?.Value)))
            {
                result.AddError(nameof(dto.ShipmentCity), "Tariff.IncompleteDirection".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            if (isAllEmpty || (!string.IsNullOrEmpty(dto.ShipmentCity?.Value) && string.IsNullOrEmpty(dto.DeliveryCity?.Value)))
            {
                result.AddError(nameof(dto.DeliveryCity), "Tariff.IncompleteDirection".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            if (isAllEmpty || (string.IsNullOrEmpty(dto.ShippingWarehouseId?.Value) && !string.IsNullOrEmpty(dto.DeliveryWarehouseId?.Value)))
            {
                result.AddError(nameof(dto.ShippingWarehouseId), "Tariff.IncompleteDirection".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            if (isAllEmpty || (!string.IsNullOrEmpty(dto.ShippingWarehouseId?.Value) && string.IsNullOrEmpty(dto.DeliveryWarehouseId?.Value)))
            {
                result.AddError(nameof(dto.DeliveryWarehouseId), "Tariff.IncompleteDirection".Translate(lang), ValidationErrorType.ValueIsRequired);
            }

            var shippingWarehouseId = dto.ShippingWarehouseId?.Value.ToGuid();
            ShippingWarehouse shippingWarehouse = null;
            if (shippingWarehouseId != null)
            {
                _shippingWarehouseCache.TryGetValue(shippingWarehouseId.Value, out shippingWarehouse);
            }
            if (shippingWarehouse?.CompanyId != null && shippingWarehouse.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.ShippingWarehouseId), "invalidCompanyShippingWarehouse".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var deliveryWarehouseId = dto.DeliveryWarehouseId?.Value.ToGuid();
            Warehouse deliveryWarehouse = null;
            if (deliveryWarehouseId != null)
            {
                _warehouseCache.TryGetValue(deliveryWarehouseId.Value, out deliveryWarehouse);
            }
            if (deliveryWarehouse?.CompanyId != null && deliveryWarehouse.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.DeliveryWarehouseId), "invalidCompanyWarehouse".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var carrierId = dto.CarrierId?.Value.ToGuid();
            TransportCompany carrier = null;
            if (carrierId != null)
            {
                _carrierCache.TryGetValue(carrierId.Value, out carrier);
            }
            if (carrier?.CompanyId != null && carrier.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.CarrierId), "invalidCompanyCarrier".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var vehicleTypeId = dto.VehicleTypeId?.Value.ToGuid();
            VehicleType vehicleType = null;
            if (vehicleTypeId != null)
            {
                _vehicleTypeCache.TryGetValue(vehicleTypeId.Value, out vehicleType);
            }
            if (vehicleType?.CompanyId != null && vehicleType.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.VehicleTypeId), "invalidCompanyVehicleType".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var bodyTypeId = dto.BodyTypeId?.Value.ToGuid();
            BodyType bodyType = null;
            if (bodyTypeId != null)
            {
                _bodyTypeCache.TryGetValue(bodyTypeId.Value, out bodyType);
            }
            if (bodyType?.CompanyId != null && bodyType.CompanyId != currentCompanyId)
            {
                result.AddError(nameof(dto.BodyTypeId), "invalidCompanyBodyType".Translate(lang), ValidationErrorType.InvalidDictionaryValue);
            }

            var existingRecords = this.GetByKey(dto).Where(i => i.Id != dto.Id.ToGuid()).ToList();

            var hasDuplicates = !result.IsError && existingRecords.Any(x => IsSamePeriod(x, dto));
            var hasOverlaps = !result.IsError && existingRecords.Any(x => IsPeriodOverlaps(x, dto));

            if (hasDuplicates)
            {
                result.AddError("duplicateTariffs", "Tariff.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }
            else if (hasOverlaps && !isConfirmed)
            {
                result.NeedConfirmation = true;
                result.ConfirmationMessage = "Tariff.PeriodOverlapsConfirmation".Translate(lang);
            }

            return result;
        }

        private bool IsSamePeriod(Tariff tariff, TariffDto dto)
        {
            var expirationDate = dto.ExpirationDate.ToDate().GetValueOrDefault(DateTime.MaxValue);
            var effectiveDate = dto.EffectiveDate.ToDate().GetValueOrDefault(DateTime.MinValue);
            return expirationDate == tariff.EffectiveDate && effectiveDate == tariff.EffectiveDate;
        }

        private bool IsPeriodOverlaps(Tariff tariff, TariffDto dto)
        {
            var expirationDate = dto.ExpirationDate.ToDate().GetValueOrDefault(DateTime.MaxValue);
            var effectiveDate = dto.EffectiveDate.ToDate().GetValueOrDefault(DateTime.MinValue);

            if (expirationDate == tariff.EffectiveDate && effectiveDate == tariff.EffectiveDate)
            {
                return false;
            }

            return !(expirationDate <= tariff.EffectiveDate.GetValueOrDefault(DateTime.MaxValue) && effectiveDate <= tariff.EffectiveDate.GetValueOrDefault(DateTime.MaxValue)
                || effectiveDate >= tariff.ExpirationDate.GetValueOrDefault(DateTime.MinValue) && expirationDate >= tariff.ExpirationDate.GetValueOrDefault(DateTime.MinValue));
        }

        // From ValidationService
        private bool IsDateValid(string dateString)
        {
            return string.IsNullOrEmpty(dateString) || dateString.ToDate().HasValue;
        }

        public override TariffDto MapFromEntityToDto(Tariff entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            return new TariffDto
            {
                Id = entity.Id.FormatGuid(),
                ShipmentRegion = string.IsNullOrEmpty(entity.ShipmentRegion) ? null : new LookUpDto(entity.ShipmentRegion),
                DeliveryRegion = string.IsNullOrEmpty(entity.DeliveryRegion) ? null : new LookUpDto(entity.DeliveryRegion),
                ShipmentCity = string.IsNullOrEmpty(entity.ShipmentCity) ? null : new LookUpDto(entity.ShipmentCity),
                DeliveryCity = string.IsNullOrEmpty(entity.DeliveryCity) ? null : new LookUpDto(entity.DeliveryCity),
                ShippingWarehouseId = entity.ShippingWarehouse == null ? null : new LookUpDto(entity.ShippingWarehouseId.FormatGuid(), entity.ShippingWarehouse.ToString()),
                DeliveryWarehouseId = entity.DeliveryWarehouse == null ? null : new LookUpDto(entity.DeliveryWarehouseId.FormatGuid(), entity.DeliveryWarehouse.ToString()),
                TarifficationType = entity.TarifficationType == null ? null : entity.TarifficationType.GetEnumLookup(lang),
                CarrierId = entity.Carrier == null ? null : new LookUpDto(entity.CarrierId.FormatGuid(), entity.Carrier.ToString()),
                VehicleTypeId = entity.VehicleType == null ? null : new LookUpDto(entity.VehicleTypeId.FormatGuid(), entity.VehicleType.ToString()),
                BodyTypeId = entity.BodyType == null ? null : new LookUpDto(entity.BodyTypeId.FormatGuid(), entity.BodyType.ToString()),
                CompanyId = entity.Company == null ? null : new LookUpDto(entity.CompanyId.FormatGuid(), entity.Company.ToString()),
                StartWinterPeriod = entity.StartWinterPeriod?.FormatDate(),
                EndWinterPeriod = entity.EndWinterPeriod?.FormatDate(),
                WinterAllowance = entity.WinterAllowance.HasValue ? 
                    entity.WinterAllowance.Value.ToString("F3", CultureInfo.InvariantCulture) : null,
                EffectiveDate = entity.EffectiveDate?.FormatDate(),
                ExpirationDate = entity.ExpirationDate?.FormatDate(),
                FtlRate = entity.FtlRate,
                LtlRate1 = entity.LtlRate1,
                LtlRate2 = entity.LtlRate2,
                LtlRate3 = entity.LtlRate3,
                LtlRate4 = entity.LtlRate4,
                LtlRate5 = entity.LtlRate5,
                LtlRate6 = entity.LtlRate6,
                LtlRate7 = entity.LtlRate7,
                LtlRate8 = entity.LtlRate8,
                LtlRate9 = entity.LtlRate9,
                LtlRate10 = entity.LtlRate10,
                LtlRate11 = entity.LtlRate11,
                LtlRate12 = entity.LtlRate12,
                LtlRate13 = entity.LtlRate13,
                LtlRate14 = entity.LtlRate14,
                LtlRate15 = entity.LtlRate15,
                LtlRate16 = entity.LtlRate16,
                LtlRate17 = entity.LtlRate17,
                LtlRate18 = entity.LtlRate18,
                LtlRate19 = entity.LtlRate19,
                LtlRate20 = entity.LtlRate20,
                LtlRate21 = entity.LtlRate21,
                LtlRate22 = entity.LtlRate22,
                LtlRate23 = entity.LtlRate23,
                LtlRate24 = entity.LtlRate24,
                LtlRate25 = entity.LtlRate25,
                LtlRate26 = entity.LtlRate26,
                LtlRate27 = entity.LtlRate27,
                LtlRate28 = entity.LtlRate28,
                LtlRate29 = entity.LtlRate29,
                LtlRate30 = entity.LtlRate30,
                LtlRate31 = entity.LtlRate31,
                LtlRate32 = entity.LtlRate32,
                LtlRate33 = entity.LtlRate33,
                ExtraPointRate = entity.ExtraPointRate,
                PoolingPalletRate = entity.PoolingPalletRate
            };
        }

        protected override ExcelMapper<TariffDto> CreateExcelMapper()
        {
            string lang = _userProvider.GetCurrentUser()?.Language;
            return new ExcelMapper<TariffDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.TarifficationType, new EnumExcelColumn<TarifficationType>(lang))
                .MapColumn(w => w.ShippingWarehouseId, new DictionaryReferenceExcelColumn<ShippingWarehouse>(_dataService, _userProvider, x => x.WarehouseName))
                .MapColumn(w => w.DeliveryWarehouseId, new DictionaryReferenceExcelColumn<Warehouse>(_dataService, _userProvider, x => x.WarehouseName))
                .MapColumn(w => w.CarrierId, new DictionaryReferenceExcelColumn<TransportCompany>(_dataService, _userProvider, x => x.Title))
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name))
                .MapColumn(w => w.VehicleTypeId, new DictionaryReferenceExcelColumn<VehicleType>(_dataService, _userProvider, x => x.Name))
                .MapColumn(w => w.BodyTypeId, new DictionaryReferenceExcelColumn<BodyType>(_dataService, _userProvider, x => x.Name));
        }

        public override TariffDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new TariffDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString())
            };
        }

        protected override IQueryable<Tariff> ApplySort(IQueryable<Tariff> query, FilterFormDto<TariffFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.EffectiveDate, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<Tariff> ApplySearch(IQueryable<Tariff> query, FilterFormDto<TariffFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.ShipmentRegion.ApplyOptionsFilter<Tariff, string>(i => i.ShipmentRegion, ref parameters))
                         .WhereAnd(form.Filter.DeliveryRegion.ApplyOptionsFilter<Tariff, string>(i => i.DeliveryRegion, ref parameters))
                         .WhereAnd(form.Filter.ShipmentCity.ApplyOptionsFilter<Tariff, string>(i => i.ShipmentCity, ref parameters))
                         .WhereAnd(form.Filter.DeliveryCity.ApplyOptionsFilter<Tariff, string>(i => i.DeliveryCity, ref parameters))
                         .WhereAnd(form.Filter.ShippingWarehouseId.ApplyOptionsFilter<Tariff, Guid?>(i => i.ShippingWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.DeliveryWarehouseId.ApplyOptionsFilter<Tariff, Guid?>(i => i.DeliveryWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.StartWinterPeriod.ApplyDateRangeFilter<Tariff>(i => i.StartWinterPeriod, ref parameters))
                         .WhereAnd(form.Filter.EndWinterPeriod.ApplyDateRangeFilter<Tariff>(i => i.EndWinterPeriod, ref parameters))
                         .WhereAnd(form.Filter.EffectiveDate.ApplyDateRangeFilter<Tariff>(i => i.EffectiveDate, ref parameters))
                         .WhereAnd(form.Filter.ExpirationDate.ApplyDateRangeFilter<Tariff>(i => i.ExpirationDate, ref parameters))
                         .WhereAnd(form.Filter.BodyTypeId.ApplyOptionsFilter<Tariff, Guid?>(i => i.BodyTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.CarrierId.ApplyOptionsFilter<Tariff, Guid?>(i => i.CarrierId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<Tariff, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.VehicleTypeId.ApplyOptionsFilter<Tariff, Guid?>(i => i.VehicleTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.TarifficationType.ApplyEnumFilter<Tariff, TarifficationType>(i => i.TarifficationType, ref parameters))
                         .WhereAnd(form.Filter.WinterAllowance.ApplyNumericFilter<Tariff>(i => i.WinterAllowance, ref parameters))
                         .WhereAnd(form.Filter.FtlRate.ApplyNumericFilter<Tariff>(i => i.FtlRate, ref parameters))
                         .WhereAnd(form.Filter.LtlRate1.ApplyNumericFilter<Tariff>(i => i.LtlRate1, ref parameters))
                         .WhereAnd(form.Filter.LtlRate2.ApplyNumericFilter<Tariff>(i => i.LtlRate2, ref parameters))
                         .WhereAnd(form.Filter.LtlRate3.ApplyNumericFilter<Tariff>(i => i.LtlRate3, ref parameters))
                         .WhereAnd(form.Filter.LtlRate4.ApplyNumericFilter<Tariff>(i => i.LtlRate4, ref parameters))
                         .WhereAnd(form.Filter.LtlRate5.ApplyNumericFilter<Tariff>(i => i.LtlRate5, ref parameters))
                         .WhereAnd(form.Filter.LtlRate6.ApplyNumericFilter<Tariff>(i => i.LtlRate6, ref parameters))
                         .WhereAnd(form.Filter.LtlRate7.ApplyNumericFilter<Tariff>(i => i.LtlRate7, ref parameters))
                         .WhereAnd(form.Filter.LtlRate8.ApplyNumericFilter<Tariff>(i => i.LtlRate8, ref parameters))
                         .WhereAnd(form.Filter.LtlRate9.ApplyNumericFilter<Tariff>(i => i.LtlRate9, ref parameters))
                         .WhereAnd(form.Filter.LtlRate10.ApplyNumericFilter<Tariff>(i => i.LtlRate10, ref parameters))
                         .WhereAnd(form.Filter.LtlRate11.ApplyNumericFilter<Tariff>(i => i.LtlRate11, ref parameters))
                         .WhereAnd(form.Filter.LtlRate12.ApplyNumericFilter<Tariff>(i => i.LtlRate12, ref parameters))
                         .WhereAnd(form.Filter.LtlRate13.ApplyNumericFilter<Tariff>(i => i.LtlRate13, ref parameters))
                         .WhereAnd(form.Filter.LtlRate14.ApplyNumericFilter<Tariff>(i => i.LtlRate14, ref parameters))
                         .WhereAnd(form.Filter.LtlRate15.ApplyNumericFilter<Tariff>(i => i.LtlRate15, ref parameters))
                         .WhereAnd(form.Filter.LtlRate16.ApplyNumericFilter<Tariff>(i => i.LtlRate16, ref parameters))
                         .WhereAnd(form.Filter.LtlRate17.ApplyNumericFilter<Tariff>(i => i.LtlRate17, ref parameters))
                         .WhereAnd(form.Filter.LtlRate18.ApplyNumericFilter<Tariff>(i => i.LtlRate18, ref parameters))
                         .WhereAnd(form.Filter.LtlRate19.ApplyNumericFilter<Tariff>(i => i.LtlRate19, ref parameters))
                         .WhereAnd(form.Filter.LtlRate20.ApplyNumericFilter<Tariff>(i => i.LtlRate20, ref parameters))
                         .WhereAnd(form.Filter.LtlRate21.ApplyNumericFilter<Tariff>(i => i.LtlRate21, ref parameters))
                         .WhereAnd(form.Filter.LtlRate22.ApplyNumericFilter<Tariff>(i => i.LtlRate22, ref parameters))
                         .WhereAnd(form.Filter.LtlRate23.ApplyNumericFilter<Tariff>(i => i.LtlRate23, ref parameters))
                         .WhereAnd(form.Filter.LtlRate24.ApplyNumericFilter<Tariff>(i => i.LtlRate24, ref parameters))
                         .WhereAnd(form.Filter.LtlRate25.ApplyNumericFilter<Tariff>(i => i.LtlRate25, ref parameters))
                         .WhereAnd(form.Filter.LtlRate26.ApplyNumericFilter<Tariff>(i => i.LtlRate26, ref parameters))
                         .WhereAnd(form.Filter.LtlRate27.ApplyNumericFilter<Tariff>(i => i.LtlRate27, ref parameters))
                         .WhereAnd(form.Filter.LtlRate28.ApplyNumericFilter<Tariff>(i => i.LtlRate28, ref parameters))
                         .WhereAnd(form.Filter.LtlRate29.ApplyNumericFilter<Tariff>(i => i.LtlRate29, ref parameters))
                         .WhereAnd(form.Filter.LtlRate30.ApplyNumericFilter<Tariff>(i => i.LtlRate30, ref parameters))
                         .WhereAnd(form.Filter.LtlRate31.ApplyNumericFilter<Tariff>(i => i.LtlRate31, ref parameters))
                         .WhereAnd(form.Filter.LtlRate32.ApplyNumericFilter<Tariff>(i => i.LtlRate32, ref parameters))
                         .WhereAnd(form.Filter.LtlRate33.ApplyNumericFilter<Tariff>(i => i.LtlRate33, ref parameters))
                         .WhereAnd(form.Filter.ExtraPointRate.ApplyNumericFilter<Tariff>(i => i.ExtraPointRate, ref parameters))
                         .WhereAnd(form.Filter.PoolingPalletRate.ApplyNumericFilter<Tariff>(i => i.PoolingPalletRate, ref parameters));

            string sql = $@"SELECT * FROM ""Tariffs"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                decimal? searchDecimal = search.ToDecimal();
                var isDecimal = searchDecimal != null;
                decimal precision = 0.01M;

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var shippingWarehouses = this._dataService.GetDbSet<ShippingWarehouse>()
                    .Where(i => i.WarehouseName.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var deliveryWarehouses = this._dataService.GetDbSet<Warehouse>()
                    .Where(i => i.WarehouseName.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var transportCompanies = this._dataService.GetDbSet<TransportCompany>()
                    .Where(i => i.Title.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var vehicleTypes = this._dataService.GetDbSet<VehicleType>()
                    .Where(i => i.Name.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var bodyTypes = this._dataService.GetDbSet<BodyType>()
                    .Where(i => i.Name.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var companies = this._dataService.GetDbSet<Company>()
                    .Where(i => i.Name.ToLower().Contains(search))
                    .Select(i => i.Id);

                var tarifficationTypeNames = Enum.GetNames(typeof(TarifficationType)).Select(i => i.ToLower());

                var tarifficationTypes = this._dataService.GetDbSet<Translation>()
                    .Where(i => tarifficationTypeNames.Contains(i.Name.ToLower()))
                    .WhereTranslation(search)
                    .Select(i => i.Name.ToEnum<TarifficationType>()).ToList();

                query = query.Where(i =>
                           transportCompanies.Any(t => t == i.CarrierId)
                        || vehicleTypes.Any(t => t == i.VehicleTypeId)
                        || bodyTypes.Any(t => t == i.BodyTypeId)
                        || companies.Any(t => t == i.CompanyId)
                        || shippingWarehouses.Any(t => t == i.ShippingWarehouseId)
                        || deliveryWarehouses.Any(t => t == i.DeliveryWarehouseId)
                        || tarifficationTypes.Contains(i.TarifficationType)
                        || i.StartWinterPeriod.HasValue && i.StartWinterPeriod.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                        || i.EndWinterPeriod.HasValue && i.EndWinterPeriod.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                        || i.EffectiveDate.HasValue && i.EffectiveDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                        || i.ExpirationDate.HasValue && i.ExpirationDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                        || i.ShipmentRegion.ToLower().Contains(search)
                        || i.DeliveryRegion.ToLower().Contains(search)
                        || i.ShipmentCity.ToLower().Contains(search)
                        || i.DeliveryCity.ToLower().Contains(search)
                        || isDecimal && i.WinterAllowance >= searchDecimal - precision && i.WinterAllowance <= searchDecimal + precision
                        || isDecimal && i.FtlRate >= searchDecimal - precision && i.FtlRate <= searchDecimal + precision
                        || isDecimal && i.LtlRate1 >= searchDecimal - precision && i.LtlRate1 <= searchDecimal + precision
                        || isDecimal && i.LtlRate2 >= searchDecimal - precision && i.LtlRate2 <= searchDecimal + precision
                        || isDecimal && i.LtlRate3 >= searchDecimal - precision && i.LtlRate3 <= searchDecimal + precision
                        || isDecimal && i.LtlRate4 >= searchDecimal - precision && i.LtlRate4 <= searchDecimal + precision
                        || isDecimal && i.LtlRate5 >= searchDecimal - precision && i.LtlRate5 <= searchDecimal + precision
                        || isDecimal && i.LtlRate6 >= searchDecimal - precision && i.LtlRate6 <= searchDecimal + precision
                        || isDecimal && i.LtlRate7 >= searchDecimal - precision && i.LtlRate7 <= searchDecimal + precision
                        || isDecimal && i.LtlRate8 >= searchDecimal - precision && i.LtlRate8 <= searchDecimal + precision
                        || isDecimal && i.LtlRate9 >= searchDecimal - precision && i.LtlRate9 <= searchDecimal + precision
                        || isDecimal && i.LtlRate10 >= searchDecimal - precision && i.LtlRate10 <= searchDecimal + precision
                        || isDecimal && i.LtlRate11 >= searchDecimal - precision && i.LtlRate11 <= searchDecimal + precision
                        || isDecimal && i.LtlRate12 >= searchDecimal - precision && i.LtlRate12 <= searchDecimal + precision
                        || isDecimal && i.LtlRate13 >= searchDecimal - precision && i.LtlRate13 <= searchDecimal + precision
                        || isDecimal && i.LtlRate14 >= searchDecimal - precision && i.LtlRate14 <= searchDecimal + precision
                        || isDecimal && i.LtlRate15 >= searchDecimal - precision && i.LtlRate15 <= searchDecimal + precision
                        || isDecimal && i.LtlRate16 >= searchDecimal - precision && i.LtlRate16 <= searchDecimal + precision
                        || isDecimal && i.LtlRate17 >= searchDecimal - precision && i.LtlRate17 <= searchDecimal + precision
                        || isDecimal && i.LtlRate18 >= searchDecimal - precision && i.LtlRate18 <= searchDecimal + precision
                        || isDecimal && i.LtlRate19 >= searchDecimal - precision && i.LtlRate19 <= searchDecimal + precision
                        || isDecimal && i.LtlRate20 >= searchDecimal - precision && i.LtlRate20 <= searchDecimal + precision
                        || isDecimal && i.LtlRate21 >= searchDecimal - precision && i.LtlRate21 <= searchDecimal + precision
                        || isDecimal && i.LtlRate22 >= searchDecimal - precision && i.LtlRate22 <= searchDecimal + precision
                        || isDecimal && i.LtlRate23 >= searchDecimal - precision && i.LtlRate23 <= searchDecimal + precision
                        || isDecimal && i.LtlRate24 >= searchDecimal - precision && i.LtlRate24 <= searchDecimal + precision
                        || isDecimal && i.LtlRate25 >= searchDecimal - precision && i.LtlRate25 <= searchDecimal + precision
                        || isDecimal && i.LtlRate26 >= searchDecimal - precision && i.LtlRate26 <= searchDecimal + precision
                        || isDecimal && i.LtlRate27 >= searchDecimal - precision && i.LtlRate27 <= searchDecimal + precision
                        || isDecimal && i.LtlRate28 >= searchDecimal - precision && i.LtlRate28 <= searchDecimal + precision
                        || isDecimal && i.LtlRate29 >= searchDecimal - precision && i.LtlRate29 <= searchDecimal + precision
                        || isDecimal && i.LtlRate30 >= searchDecimal - precision && i.LtlRate30 <= searchDecimal + precision
                        || isDecimal && i.LtlRate31 >= searchDecimal - precision && i.LtlRate31 <= searchDecimal + precision
                        || isDecimal && i.LtlRate32 >= searchDecimal - precision && i.LtlRate32 <= searchDecimal + precision
                        || isDecimal && i.LtlRate33 >= searchDecimal - precision && i.LtlRate33 <= searchDecimal + precision
                        || isDecimal && i.ExtraPointRate >= searchDecimal - precision && i.ExtraPointRate <= searchDecimal + precision
                        || isDecimal && i.PoolingPalletRate >= searchDecimal - precision && i.PoolingPalletRate <= searchDecimal + precision
                    );
            }

            return query;
        }

        public override Tariff FindByKey(TariffDto dto)
        {
            var effectiveDate = dto.EffectiveDate.ToDate();
            var expirationDate = dto.ExpirationDate.ToDate();

            return this.GetByKey(dto)
                .Where(i => i.EffectiveDate == effectiveDate)
                .Where(i => i.ExpirationDate == expirationDate)
                .FirstOrDefault();
        }

        public override IEnumerable<Tariff> FindByKey(IEnumerable<TariffDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(Tariff entity)
        {
            return (entity.CarrierId.FormatGuid() ?? string.Empty) + "#"
                + (entity.VehicleTypeId.FormatGuid() ?? string.Empty) + "#"
                + (entity.BodyTypeId.FormatGuid() ?? string.Empty) + "#"
                + (entity.TarifficationType.FormatEnum() ?? string.Empty) + "#"
                + (entity.CompanyId.FormatGuid() ?? string.Empty) + "#"
                + (entity.ShipmentRegion ?? string.Empty) + "#"
                + (entity.DeliveryRegion ?? string.Empty) + "#"
                + (entity.ShipmentCity ?? string.Empty) + "#"
                + (entity.DeliveryCity ?? string.Empty) + "#"
                + (entity.ShippingWarehouseId.FormatGuid() ?? string.Empty) + "#"
                + (entity.DeliveryWarehouseId.FormatGuid() ?? string.Empty) + "#"
                + (entity.EffectiveDate.FormatDate() ?? string.Empty) + "#"
                + (entity.ExpirationDate.FormatDate() ?? string.Empty) + "#";
        }

        public override string GetDtoKey(TariffDto dto)
        {
            return (dto.CarrierId?.Value ?? string.Empty) + "#"
                + (dto.VehicleTypeId?.Value ?? string.Empty) + "#"
                + (dto.BodyTypeId?.Value ?? string.Empty) + "#"
                + (dto.TarifficationType?.Value ?? string.Empty) + "#"
                + (dto.CompanyId?.Value ?? string.Empty) + "#"
                + (dto.ShipmentRegion?.Value ?? string.Empty) + "#"
                + (dto.DeliveryRegion?.Value ?? string.Empty) + "#"
                + (dto.ShipmentCity?.Value ?? string.Empty) + "#"
                + (dto.DeliveryCity?.Value ?? string.Empty) + "#"
                + (dto.ShippingWarehouseId?.Value ?? string.Empty) + "#"
                + (dto.DeliveryWarehouseId?.Value ?? string.Empty) + "#"
                + (dto.EffectiveDate ?? string.Empty) + "#"
                + (dto.ExpirationDate ?? string.Empty) + "#";
        }

        private IEnumerable<Tariff> GetByKey(TariffDto dto)
        {
            EnsureTariffsCache();

            Guid? carrierId = dto.CarrierId?.Value?.ToGuid();
            Guid? vehicleTypeId = dto.VehicleTypeId?.Value?.ToGuid();
            Guid? bodyTypeId = dto.BodyTypeId?.Value?.ToGuid();
            Guid? companyId = dto.CompanyId?.Value?.ToGuid();
            string shipmentRegion = dto.ShipmentRegion?.Value;
            string deliveryRegion = dto.DeliveryRegion?.Value;
            string shipmentCity = dto.ShipmentCity?.Value;
            string deliveryCity = dto.DeliveryCity?.Value;
            Guid? shippingWarehouseId = dto.ShippingWarehouseId?.Value?.ToGuid();
            Guid? deliveryWarehouseId = dto.DeliveryWarehouseId?.Value?.ToGuid();
            TarifficationType? tarifficationType = dto.TarifficationType?.Value.ToEnum<TarifficationType>();
            return _tariffsCache
                    .Where(i =>
                        i.CarrierId == carrierId
                        && i.VehicleTypeId == vehicleTypeId
                        && i.BodyTypeId == bodyTypeId
                        && i.TarifficationType == tarifficationType
                        && i.CompanyId == companyId
                        && ((i.ShippingWarehouseId == null && shippingWarehouseId == null) || i.ShippingWarehouseId == shippingWarehouseId)
                        && ((i.DeliveryWarehouseId == null && deliveryWarehouseId == null) || i.DeliveryWarehouseId == deliveryWarehouseId)
                        && ((i.ShipmentRegion == null && shipmentRegion == null) || i.ShipmentRegion == shipmentRegion)
                        && ((i.DeliveryRegion == null && deliveryRegion == null) || i.DeliveryRegion == deliveryRegion)
                        && ((i.ShipmentCity == null && shipmentCity == null) || i.ShipmentCity == shipmentCity)
                        && ((i.DeliveryCity == null && deliveryCity == null) || i.DeliveryCity == deliveryCity))
                    .ToList();
        }

        protected override IQueryable<Tariff> ApplyRestrictions(IQueryable<Tariff> query)
        {
            query = base.ApplyRestrictions(query);

            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null)
            {
                query = query.Where(x => x.CompanyId == null || x.CompanyId == currentUser.CompanyId);
            }

            return query;
        }

        private void EnsureCache()
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;

            EnsureEntityCache(companyId, ref _bodyTypeCache);
            EnsureEntityCache(companyId, ref _carrierCache);
            EnsureEntityCache(companyId, ref _shippingWarehouseCache);
            EnsureEntityCache(companyId, ref _vehicleTypeCache);
            EnsureEntityCache(companyId, ref _warehouseCache);

            EnsureTariffsCache();
        }

        private void EnsureTariffsCache()
        {
            if (_tariffsCache == null)
            {
                var companyId = _userProvider.GetCurrentUser()?.CompanyId;
                _tariffsCache = _dataService.GetDbSet<Tariff>()
                                            .Where(x => x.CompanyId == null || companyId == null || x.CompanyId == companyId)
                                            .ToList();
            }
        }

        private void EnsureEntityCache<TEntity>(Guid? companyId, ref Dictionary<Guid, TEntity> cache)
            where TEntity : class, IPersistable, ICompanyPersistable
        {
            if (cache == null)
            {
                cache = _dataService.GetDbSet<TEntity>()
                                    .Where(x => x.CompanyId == null || companyId == null || x.CompanyId == companyId)
                                    .ToDictionary(x => x.Id);
            }
        }
    }
}