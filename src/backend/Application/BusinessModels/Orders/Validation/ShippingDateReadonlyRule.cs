using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Validation
{
    public class ShippingDateReadonlyRule : BaseReadonlyRule
    {
        public ShippingDateReadonlyRule(IUserProvider userProvider) : base(userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.ShippingDate);

        protected override bool ChangeCheck(OrderDto dto, Order entity)
        {
            return entity == null
                || entity.OrderShippingStatus != ShippingState.ShippingSlotBooked
                || (entity.TarifficationType != TarifficationType.Pooling && entity.TarifficationType != TarifficationType.Milkrun)
                || (dto.ShippingDate.ToDateTime() == entity.ShippingDate);
        }

        protected override string GetMessage(string lang)
        {
            return "valueIsReadonlyForPooling".Translate(lang, Field.ToLowerFirstLetter().Translate(lang));
        }
    }
}
