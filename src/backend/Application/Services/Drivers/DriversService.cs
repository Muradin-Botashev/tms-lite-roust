using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Drivers;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Drivers
{
    public class DriversService : DictoinaryServiceBase<Driver, DriverDto, DriverFilterDto>, IDriversService
    {
        public DriversService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<DriverDto, Driver>> validationRules)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        { }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var query = _dataService.GetDbSet<Driver>().AsQueryable();
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

            foreach (Driver driver in entities)
            {
                yield return new LookUpDto
                {
                    Name = $"{driver.Name} ({driver.Passport})",
                    Value = driver.Id.FormatGuid()
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

        protected override DetailedValidationResult ValidateDto(DriverDto dto, Driver entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();

            var otherDrivers = _dataService.GetDbSet<Driver>().Where(x => x.Id != currentId).ToList();

            bool duplicateDriverLicence = otherDrivers.Any(x => !string.IsNullOrEmpty(dto.DriverLicence) && x.DriverLicence.ToLower() == dto.DriverLicence.ToLower());
            if (duplicateDriverLicence)
            {
                result.AddError(nameof(dto.DriverLicence), "driver.duplicateDriverLicence".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            bool duplicatePassport = otherDrivers.Any(x => !string.IsNullOrEmpty(dto.Passport) && x.Passport.ToLower() == dto.Passport.ToLower());
            if (duplicatePassport)
            {
                result.AddError(nameof(dto.Passport), "driver.duplicatePassport".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        protected override IQueryable<Driver> ApplySort(IQueryable<Driver> query, FilterFormDto<DriverFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true).DefaultOrderBy(i => i.Name, true, true);
        }

        public override DriverDto GetDefaults()
        {
            return new DriverDto
            {
                IsActive = true
            };
        }

        public override Driver FindByKey(DriverDto dto)
        {
            return _dataService.GetDbSet<Driver>().FirstOrDefault(i => i.Passport == dto.Passport);
        }

        public override IEnumerable<Driver> FindByKey(IEnumerable<DriverDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(Driver entity)
        {
            return entity.Passport;
        }

        public override string GetDtoKey(DriverDto dto)
        {
            return dto.Passport;
        }

        public override DriverDto MapFromEntityToDto(Driver entity)
        {
            return new DriverDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                DriverLicence = entity.DriverLicence,
                Passport = entity.Passport,
                Phone = entity.Phone,
                Email = entity.Email,
                IsBlackList = entity.IsBlackList,
                IsActive = entity.IsActive
            };
        }

        public override DetailedValidationResult MapFromDtoToEntity(Driver entity, DriverDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);
            entity.Name = dto.Name;
            entity.DriverLicence = dto.DriverLicence;
            entity.Passport = dto.Passport;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
            entity.IsBlackList = dto.IsBlackList;
            entity.IsActive = dto.IsActive;

            return null;
        }

        protected override IQueryable<Driver> ApplySearch(IQueryable<Driver> query, FilterFormDto<DriverFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Name.ApplyStringFilter<Driver>(i => i.Name, ref parameters))
                         .WhereAnd(form.Filter.DriverLicence.ApplyStringFilter<Driver>(i => i.DriverLicence, ref parameters))
                         .WhereAnd(form.Filter.Passport.ApplyStringFilter<Driver>(i => i.Passport, ref parameters))
                         .WhereAnd(form.Filter.Phone.ApplyStringFilter<Driver>(i => i.Phone, ref parameters))
                         .WhereAnd(form.Filter.Email.ApplyStringFilter<Driver>(i => i.Email, ref parameters))
                         .WhereAnd(form.Filter.IsBlackList.ApplyBooleanFilter<Driver>(i => i.IsBlackList, ref parameters))
                         .WhereAnd(form.Filter.IsActive.ApplyBooleanFilter<Driver>(i => i.IsActive, ref parameters));

            string sql = $@"SELECT * FROM ""Drivers"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();
                int? searchInt = search.ToInt();
                var isInt = searchInt != null;

                query = query.Where(i =>
                           i.Name.ToLower().Contains(search)
                        || i.DriverLicence.ToLower().Contains(search)
                        || i.Passport.ToLower().Contains(search)
                        || i.Phone.ToLower().Contains(search)
                        || i.Email.ToLower().Contains(search));
            }

            return query;
        }
    }
}