using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.ShippingSchedules;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ShippingSchedules
{
    public class ShippingSchedulesService : DictoinaryServiceBase<ShippingSchedule, ShippingScheduleDto, ShippingScheduleFilterDto>, IShippingSchedulesService
    {
        public ShippingSchedulesService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<ShippingScheduleDto, ShippingSchedule>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        protected override IQueryable<ShippingSchedule> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Carrier);
        }

        public override DetailedValidationResult MapFromDtoToEntity(ShippingSchedule entity, ShippingScheduleDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);

            entity.ShippingCity = dto.ShippingCity?.Value;
            entity.DeliveryCity = dto.DeliveryCity?.Value;
            entity.CarrierId = dto.CarrierId?.Value?.ToGuid();

            entity.ShippingDays = dto?.ShippingDays?.Select(i => i.Value.ToEnum<WeekDay>())
                                                    .Where(i => i != null)
                                                    .Cast<int>()
                                                    .ToArray();

            entity.DeliveryDays = dto?.DeliveryDays?.Select(i => i.Value.ToEnum<WeekDay>())
                                                    .Where(i => i != null)
                                                    .Cast<int>()
                                                    .ToArray();

            return null;
        }

        public override ShippingScheduleDto MapFromEntityToDto(ShippingSchedule entity)
        {
            string lang = _userProvider.GetCurrentUser()?.Language;
            return new ShippingScheduleDto
            {
                Id = entity.Id.FormatGuid(),
                ShippingCity = entity.ShippingCity == null ? null : new LookUpDto(entity.ShippingCity),
                DeliveryCity = entity.DeliveryCity == null ? null : new LookUpDto(entity.DeliveryCity),
                CarrierId = entity.CarrierId == null ? null : new LookUpDto(entity.CarrierId.FormatGuid(), entity.Carrier.ToString()),
                ShippingDays = entity.ShippingDays?.Cast<WeekDay>()?.Select(x => x.GetEnumLookup(lang)),
                DeliveryDays = entity.DeliveryDays?.Cast<WeekDay>()?.Select(x => x.GetEnumLookup(lang))
            };
        }

        public override IEnumerable<LookUpDto> ForSelect(string fieldName, FilterFormDto<ShippingScheduleFilterDto> form)
        {
            if (fieldName.ToLower() == nameof(ShippingScheduleDto.ShippingDays).ToLower()
                || fieldName.ToLower() == nameof(ShippingScheduleDto.DeliveryDays).ToLower())
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

                List<int> values;
                if (fieldName.ToLower() == nameof(ShippingScheduleDto.ShippingDays).ToLower())
                {
                    values = query.Where(x => x.ShippingDays != null && x.ShippingDays.Length > 0)
                                  .SelectMany(x => x.ShippingDays)
                                  .Distinct()
                                  .ToList();
                }
                else
                {
                    values = query.Where(x => x.DeliveryDays != null && x.DeliveryDays.Length > 0)
                                  .SelectMany(x => x.DeliveryDays)
                                  .Distinct()
                                  .ToList();
                }

                var lang = _userProvider.GetCurrentUser()?.Language;
                return values.Cast<WeekDay>()
                             .OrderBy(x => x)
                             .Select(x => x.GetEnumLookup(lang))
                             .ToList();
            }
            else
            {
                return base.ForSelect(fieldName, form);
            }
        }

        protected override DetailedValidationResult ValidateDto(ShippingScheduleDto dto, ShippingSchedule entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var shippingCity = dto.ShippingCity?.Value;
            var deliveryCity = dto.DeliveryCity?.Value;
            var carrierId = dto.CarrierId?.Value.ToGuid();
            var hasDuplicates = !result.IsError 
                                && _dataService.Any<ShippingSchedule>(x => x.ShippingCity == shippingCity
                                                                        && x.DeliveryCity == deliveryCity
                                                                        && x.CarrierId == carrierId
                                                                        && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.ShippingCity), "ShippingSchedules.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
                result.AddError(nameof(dto.DeliveryCity), "ShippingSchedules.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
                result.AddError(nameof(dto.CarrierId), "ShippingSchedules.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        protected override IQueryable<ShippingSchedule> ApplySort(IQueryable<ShippingSchedule> query, FilterFormDto<ShippingScheduleFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<ShippingSchedule> ApplySearch(IQueryable<ShippingSchedule> query, FilterFormDto<ShippingScheduleFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.ShippingCity.ApplyStringFilter<ShippingSchedule>(i => i.ShippingCity, ref parameters))
                         .WhereAnd(form.Filter.DeliveryCity.ApplyStringFilter<ShippingSchedule>(i => i.DeliveryCity, ref parameters))
                         .WhereAnd(form.Filter.CarrierId.ApplyOptionsFilter<ShippingSchedule, Guid?>(i => i.CarrierId, ref parameters, i => new Guid(i)))
                         .WhereAnd(form.Filter.ShippingDays.ApplyEnumArrayFilter<ShippingSchedule, int[], WeekDay>(i => i.ShippingDays, ref parameters))
                         .WhereAnd(form.Filter.DeliveryDays.ApplyEnumArrayFilter<ShippingSchedule, int[], WeekDay>(i => i.DeliveryDays, ref parameters));

            string sql = $@"SELECT * FROM ""ShippingSchedules"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var companyId = _userProvider.GetCurrentUser()?.CompanyId;

                var carriers = _dataService.GetDbSet<TransportCompany>()
                    .Where(i => i.Title.ToLower().Contains(search)
                            && (i.CompanyId == null || companyId == null || i.CompanyId == companyId))
                    .Select(i => i.Id);

                var weekDayNames = Enum.GetNames(typeof(WeekDay)).Select(i => i.ToLower());
                var weekDays = _dataService.GetDbSet<Translation>()
                    .Where(i => weekDayNames.Contains(i.Name.ToLower()))
                    .WhereTranslation(search)
                    .Select(i => (int)i.Name.ToEnum<WeekDay>()).ToList();

                query = query.Where(i =>
                        carriers.Any(t => t == i.CarrierId)
                        || i.ShippingCity.ToLower().Contains(search)
                        || i.DeliveryCity.ToLower().Contains(search)
                        || (i.ShippingDays != null && i.ShippingDays.Any(t => weekDays.Contains(t)))
                        || (i.DeliveryDays != null && i.DeliveryDays.Any(t => weekDays.Contains(t))));
            }

            return query;
        }

        public override ShippingSchedule FindByKey(ShippingScheduleDto dto)
        {
            var shippingCity = dto.ShippingCity?.Value;
            var deliveryCity = dto.DeliveryCity?.Value;
            var carrierId = dto.CarrierId?.Value.ToGuid();
            return _dataService.GetDbSet<ShippingSchedule>()
                               .FirstOrDefault(i => i.ShippingCity == shippingCity
                                                && i.DeliveryCity == deliveryCity
                                                && i.CarrierId == carrierId);
        }

        public override IEnumerable<ShippingSchedule> FindByKey(IEnumerable<ShippingScheduleDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(ShippingSchedule entity)
        {
            return string.Join('#',
                entity.ShippingCity,
                entity.DeliveryCity,
                entity.CarrierId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(ShippingScheduleDto dto)
        {
            return string.Join('#',
                dto.ShippingCity?.Value ?? string.Empty,
                dto.DeliveryCity?.Value ?? string.Empty,
                dto.CarrierId?.Value ?? string.Empty);
        }

        protected override ExcelMapper<ShippingScheduleDto> CreateExcelMapper()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            return new ExcelMapper<ShippingScheduleDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CarrierId, new DictionaryReferenceExcelColumn<TransportCompany>(_dataService, _userProvider, x => x.Title))
                .MapColumn(w => w.ShippingDays, new EnumArrayExcelColumn<WeekDay>(lang))
                .MapColumn(w => w.DeliveryDays, new EnumArrayExcelColumn<WeekDay>(lang));
        }
    }
}
