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
    public class UnloadingArrivalTimeValidationRule : IValidationRule<OrderDto, Order>
    {
        private IUserProvider _userProvider;

        public UnloadingArrivalTimeValidationRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return !string.IsNullOrEmpty(fieldName)
                && new[]
                {
                    nameof(OrderDto.LoadingDepartureTime).ToLower(),
                    nameof(OrderDto.UnloadingArrivalTime).ToLower()
                }
                .Contains(fieldName.ToLower());
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var loadingDepartureTime = dto.LoadingDepartureTime.ToDateTime();
            var unloadingArrivalTime = dto.UnloadingArrivalTime.ToDateTime();

            if (loadingDepartureTime.HasValue && unloadingArrivalTime.HasValue && unloadingArrivalTime < loadingDepartureTime)
            {
                return new DetailedValidationResult
                (
                    nameof(dto.UnloadingArrivalTime).ToLowerFirstLetter(),
                    "InvalidUnloadingArrivalTime".Translate(lang),
                    ValidationErrorType.InvalidDateRange
                );
            }

            return null;
        }
    }
}
