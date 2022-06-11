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
    public class UnloadingDepartureTimeValidationRule : IValidationRule<OrderDto, Order>
    {
        private IUserProvider _userProvider;

        public UnloadingDepartureTimeValidationRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return !string.IsNullOrEmpty(fieldName)
                && new[]
                {
                    nameof(OrderDto.UnloadingDepartureTime).ToLower(),
                    nameof(OrderDto.UnloadingArrivalTime).ToLower()
                }
                .Contains(fieldName.ToLower());
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var unloadingDepartureTime = dto.UnloadingDepartureTime.ToDateTime();
            var unloadingArrivalTime = dto.UnloadingArrivalTime.ToDateTime();

            if (unloadingDepartureTime.HasValue && unloadingArrivalTime.HasValue && unloadingArrivalTime > unloadingDepartureTime)
            {
                return new DetailedValidationResult
                (
                    nameof(dto.UnloadingDepartureTime).ToLowerFirstLetter(),
                    "InvalidUnloadingDepartureTime".Translate(lang),
                    ValidationErrorType.InvalidDateRange
                );
            }

            return null;
        }
    }
}
