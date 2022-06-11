using Application.BusinessModels.Shared.Validation;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Linq;

namespace Application.BusinessModels.Shippings.Validation
{
    public class PoolingRoutesValidationRule : IValidationRule<ShippingDto, Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly string _field = nameof(RoutePointDto.PlannedDate);

        public PoolingRoutesValidationRule(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == _field.ToLower();
        }

        public DetailedValidationResult Validate(ShippingDto dto, Shipping entity)
        {
            if (!ChangeCheck(dto, entity))
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

        private bool ChangeCheck(ShippingDto dto, Shipping entity)
        {
            if (entity.Status != ShippingState.ShippingSlotBooked) return true;

            var shippingForm = dto as ShippingFormDto;

            var orders = _dataService.GetDbSet<Order>().Where(i => i.ShippingId == entity.Id).ToList();

            foreach (var routePoint in shippingForm.RoutePoints)
            {
                var ordersIds = routePoint.OrderIds.Select(i => i.ToGuid());
                var routeOrders = orders.Where(i => ordersIds.Contains(i.Id)).ToList();

                var plannedDate = routePoint.PlannedDate.ToDateTime();

                if (routePoint.IsLoading && routeOrders.Any(i => i.ShippingDate != plannedDate))
                {
                    return false;
                }

                if (!routePoint.IsLoading && routeOrders.Any(i => i.DeliveryDate != plannedDate))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
