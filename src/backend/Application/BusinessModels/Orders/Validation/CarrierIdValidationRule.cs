using Application.BusinessModels.Shared.Validation;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System.Linq;

namespace Application.BusinessModels.Orders.Validation
{
    public class CarrierIdValidationRule : IValidationRule<OrderDto, Order>
    {
        private ICommonDataService _dataService;
        private IUserProvider _userProvider;

        public CarrierIdValidationRule(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == nameof(OrderDto.CarrierId).ToLower();
        }

        public DetailedValidationResult Validate(OrderDto dto, Order entity)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            if (string.IsNullOrEmpty(dto.CarrierId?.Value)
                && entity != null
                && entity.CarrierId != null)
            {
                bool isAllowed = entity.Status != OrderState.Shipped
                                && entity.Status != OrderState.Delivered
                                && entity.Status != OrderState.Archive;


                if (isAllowed && entity.ShippingId != null)
                {
                    var shipping = _dataService.GetById<Shipping>(entity.ShippingId.Value);

                    isAllowed = shipping.Status != ShippingState.ShippingCompleted
                                && shipping.Status != ShippingState.ShippingBillSend
                                && shipping.Status != ShippingState.ShippingArhive;

                    if (isAllowed)
                    {
                        var otherOrderStatuses = _dataService.GetDbSet<Order>()
                                                             .Where(x => x.ShippingId == shipping.Id && x.Id != entity.Id)
                                                             .Select(x => x.Status)
                                                             .ToList();
                        isAllowed = otherOrderStatuses.All(x => x != OrderState.Shipped
                                                            && x != OrderState.Delivered
                                                            && x != OrderState.Archive);
                    }
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
