using Application.BusinessModels.Shared.Validation;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Shippings.Validation
{
    public class TarifficationTypeReadonlyRule : IValidationRule<ShippingDto, Shipping>
    {
        private readonly IUserProvider _userProvider;
        private readonly string _field = nameof(ShippingDto.TarifficationType);

        public TarifficationTypeReadonlyRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == _field.ToLower();
        }

        public DetailedValidationResult Validate(ShippingDto dto, Shipping entity)
        {
            if (entity.Status == ShippingState.ShippingSlotBooked 
                && dto.TarifficationType?.Value?.ToEnum<TarifficationType>() != entity.TarifficationType)
            {
                var lang = _userProvider.GetCurrentUser()?.Language;
                return new DetailedValidationResult
                (
                    _field.ToLowerFirstLetter(),
                    "valueIsReadonlyForPooling".Translate(lang, _field.ToLowerFirstLetter().Translate(lang)),
                    ValidationErrorType.ValueIsReadonly
                );
            }

            return null;
        }
    }
}
