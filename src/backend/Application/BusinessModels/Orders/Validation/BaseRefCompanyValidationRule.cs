using Application.BusinessModels.Shared.Validation;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Orders.Validation
{
    public abstract class BaseRefCompanyValidationRule<TRefEntity> : IValidationRule<OrderDto, Order>
        where TRefEntity : class, IPersistable
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public BaseRefCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return Field?.ToLower() == fieldName?.ToLower();
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            var user = _userProvider.GetCurrentUser();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid() ?? user?.CompanyId;
            var refEntityId = GetRefId(dto);
            var refEntity = refEntityId == null ? null : _dataService.GetById<TRefEntity>(refEntityId.Value);
            var refCompanyId = refEntity == null ? null : GetRefCompanyId(refEntity);
            if (refCompanyId != null && refCompanyId != currentCompanyId)
            {
                var lang = user?.Language;
                return new DetailedValidationResult(Field.ToLowerFirstLetter(), GetMessage(lang), ValidationErrorType.InvalidDictionaryValue);
            }
            else
            {
                return null;
            }
        }

        protected abstract string Field { get; }

        protected abstract Guid? GetRefId(OrderDto dto);

        protected abstract Guid? GetRefCompanyId(TRefEntity entity);

        protected abstract string GetMessage(string lang);
    }
}
