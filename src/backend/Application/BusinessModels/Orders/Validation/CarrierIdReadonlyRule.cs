using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Validation
{
    public class CarrierIdReadonlyRule : BaseReadonlyRule
    {
        public CarrierIdReadonlyRule(IUserProvider userProvider) : base(userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.CarrierId);

        protected override bool ChangeCheck(OrderDto dto, Order entity)
        {
            return entity == null
                || entity.OrderShippingStatus != ShippingState.ShippingSlotBooked
                || (entity.TarifficationType != TarifficationType.Pooling && entity.TarifficationType != TarifficationType.Milkrun)
                || (dto.CarrierId == null && entity.CarrierId == null)
                || (dto.CarrierId != null && dto.CarrierId.Value.ToGuid() == entity.CarrierId);
        }

        protected override string GetMessage(string lang)
        {
            return "valueIsReadonlyForPooling".Translate(lang, Field.ToLowerFirstLetter().Translate(lang));
        }
    }
}
