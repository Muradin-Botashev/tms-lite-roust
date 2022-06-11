using Application.BusinessModels.Shared.Validation;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Shared;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Validation
{
    public abstract class BaseReadonlyRule : IValidationRule<OrderDto, Order>
    {
        private IUserProvider _userProvider;

        public BaseReadonlyRule(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == Field.ToLower();
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            if (!ChangeCheck(dto, entity))
            {
                var lang = _userProvider.GetCurrentUser()?.Language;
                return new DetailedValidationResult(Field.ToLowerFirstLetter(), GetMessage(lang), ValidationErrorType.ValueIsReadonly);
            }
            return null;
        }

        protected abstract string Field { get; }

        protected abstract string GetMessage(string lang);

        protected abstract bool ChangeCheck(OrderDto dto, Order entity);
    }
}
