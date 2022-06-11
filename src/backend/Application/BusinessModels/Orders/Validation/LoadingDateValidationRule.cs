using Application.BusinessModels.Shared.Validation;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Linq;

namespace Application.BusinessModels.Orders.Validation
{
    public class LoadingDateValidationRule : IValidationRule<OrderDto, Order>
    {
        private IUserProvider _userProvider;

        public LoadingDateValidationRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return !string.IsNullOrEmpty(fieldName)
                && new[]
                {
                    nameof(OrderDto.LoadingArrivalTime).ToLower(),
                    nameof(OrderDto.LoadingDepartureTime).ToLower()
                }
                .Contains(fieldName.ToLower());
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var loadingDepartureTime = dto.LoadingDepartureTime.ToDateTime();
            var loadingArrivalTime = dto.LoadingArrivalTime.ToDateTime();

            if (loadingDepartureTime.HasValue && loadingArrivalTime.HasValue && loadingDepartureTime < loadingArrivalTime)
            {
                return new DetailedValidationResult(
                    nameof(dto.LoadingArrivalTime).ToLowerFirstLetter(),
                    "InvalidLoadingDepartureOrArrivalTime".Translate(lang),
                    ValidationErrorType.InvalidDateRange
                );
            }

            return null;
        }
    }
}
