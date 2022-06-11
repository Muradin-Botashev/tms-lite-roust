using Application.BusinessModels.Shared.Validation;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Validation
{
    public class DeliveryRegionValidationRule : IValidationRule<OrderDto, Order>
    {
        private IUserProvider _userProvider;

        public DeliveryRegionValidationRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == nameof(OrderDto.DeliveryRegion).ToLower();
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            if (string.IsNullOrEmpty(dto.DeliveryRegion) && !string.IsNullOrEmpty(dto.Id))
            {
                var lang = _userProvider.GetCurrentUser()?.Language;

                return new DetailedValidationResult(
                    nameof(dto.DeliveryRegion).ToLowerFirstLetter(),
                    "ValueIsRequired".Translate(lang, nameof(dto.DeliveryRegion).ToLowerFirstLetter().Translate(lang)),
                    ValidationErrorType.ValueIsRequired
                );
            }
            return null;
        }
    }
}
