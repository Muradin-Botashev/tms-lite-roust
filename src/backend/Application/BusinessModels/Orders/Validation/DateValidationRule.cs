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
    public class DateValidationRule : IValidationRule<OrderDto, Order>
    {
        private IUserProvider _userProvider;

        public DateValidationRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return !string.IsNullOrEmpty(fieldName)
                && new[]
                {
                    nameof(OrderDto.ShippingDate).ToLower(),
                    nameof(OrderDto.DeliveryDate).ToLower()
                }
                .Contains(fieldName.ToLower());
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var deliveryDate = dto.DeliveryDate.ToDate();
            var shippingDate = dto.ShippingDate.ToDate();

            if (deliveryDate.HasValue && shippingDate.HasValue && deliveryDate < shippingDate)
            {
                return new DetailedValidationResult(
                    nameof(dto.ShippingDate).ToLowerFirstLetter(),
                    "InvalidDeliveryOrShippingDate".Translate(lang),
                    ValidationErrorType.InvalidDateRange
                );
            }

            return null;
        }
    }
}
