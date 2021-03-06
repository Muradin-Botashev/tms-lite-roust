using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Orders.Validation
{
    public class WeightKgReadonlyRule : BaseReadonlyRule
    {
        private ICommonDataService _dataService;

        public WeightKgReadonlyRule(ICommonDataService dataService, IUserProvider userProvider) : base(userProvider)
        {
            _dataService = dataService;
        }

        protected override string Field => nameof(OrderDto.WeightKg);

        protected override bool ChangeCheck(OrderDto dto, Order entity)
        {
            var shipping = entity?.ShippingId == null ? null : _dataService.GetById<Shipping>(entity.ShippingId.Value);
            return entity == null
                || entity.OrderShippingStatus != ShippingState.ShippingSlotBooked
                || (entity.TarifficationType != TarifficationType.Pooling && entity.TarifficationType != TarifficationType.Milkrun)
                || dto.WeightKg == entity.WeightKg
                || (shipping?.AvailableUntil == null || shipping?.AvailableUntil >= DateTime.Now);
        }

        protected override string GetMessage(string lang)
        {
            return "valueIsUnavailableForPooling".Translate(lang, Field.ToLowerFirstLetter().Translate(lang));
        }
    }
}
