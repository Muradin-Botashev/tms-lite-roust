using Application.BusinessModels.Shared.Validation;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Validation
{
    public class RoutesValidationRule : IValidationRule<ShippingDto, Shipping>
    {
        private readonly IUserProvider _userProvider;

        public RoutesValidationRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return false;
        }

        public DetailedValidationResult Validate(ShippingDto dto, Shipping entity)
        {
            var form = dto as ShippingFormDto;

            if (form == null) return null;

            var lang = _userProvider.GetCurrentUser()?.Language;
            var results = new DetailedValidationResult();

            var loadingDates = form.RoutePoints.Where(x => x.IsLoading).Select(x => x.PlannedDate.ToDateTime()).Where(x => x.HasValue);
            var unloadingDates = form.RoutePoints.Where(x => !x.IsLoading).Select(x => x.PlannedDate.ToDateTime()).Where(x => x.HasValue);

            foreach (var route in form.RoutePoints)
            {
                var plannedDate = route.PlannedDate.ToDateTime();

                if (!plannedDate.HasValue) continue;

                IEnumerable<DateTime?> invalidDates;
                if (route.IsLoading)
                {
                    invalidDates = unloadingDates.Where(i => plannedDate.Value > i);
                }
                else
                {
                    invalidDates = loadingDates.Where(i => plannedDate.Value < i);
                }

                if (invalidDates.Any())
                {
                    results.AddError(nameof(RoutePointDto.PlannedDate), "InvalidDeliveryOrShippingDate".Translate(lang), ValidationErrorType.InvalidDateRange);
                }
            }

            return results;
        }
    }
}
