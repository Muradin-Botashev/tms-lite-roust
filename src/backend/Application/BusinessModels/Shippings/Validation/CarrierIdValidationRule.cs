using Application.BusinessModels.Shared.Validation;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System.Linq;

namespace Application.BusinessModels.Shippings.Validation
{
    public class CarrierIdValidationRule : IValidationRule<ShippingDto, Shipping>
    {
        private ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public CarrierIdValidationRule(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == nameof(ShippingDto.CarrierId).ToLower();
        }

        public DetailedValidationResult Validate(ShippingDto dto, Shipping entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            if (string.IsNullOrEmpty(dto.CarrierId?.Value)
                && entity != null
                && entity.CarrierId != null)
            {
                bool isAllowed = entity.Status != ShippingState.ShippingCompleted
                                && entity.Status != ShippingState.ShippingBillSend
                                && entity.Status != ShippingState.ShippingArhive;

                if (isAllowed)
                {
                    var orderStatuses = _dataService.GetDbSet<Order>()
                                                    .Where(x => x.ShippingId == entity.Id)
                                                    .Select(x => x.Status)
                                                    .ToList();
                    isAllowed = orderStatuses.All(x => x != OrderState.Shipped
                                                    && x != OrderState.Delivered
                                                    && x != OrderState.Archive);
                }

                if (!isAllowed)
                {
                    return new DetailedValidationResult
                    (
                        nameof(dto.CarrierId).ToLowerFirstLetter(),
                        "carrierIdCannotBeEmpty".Translate(lang),
                        ValidationErrorType.ValueIsRequired
                    );
                }
            }

            return null;
        }
    }
}
