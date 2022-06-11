using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Validation
{
    public class DeliveryWarehouseIdReadonlyRule : BaseReadonlyRule
    {
        public DeliveryWarehouseIdReadonlyRule(IUserProvider userProvider) : base(userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.DeliveryWarehouseId);

        protected override bool ChangeCheck(OrderDto dto, Order entity)
        {
            return entity == null
                || entity.OrderShippingStatus != ShippingState.ShippingSlotBooked
                || (entity.TarifficationType != TarifficationType.Pooling && entity.TarifficationType != TarifficationType.Milkrun)
                || (dto.DeliveryWarehouseId == null && entity.DeliveryWarehouseId == null)
                || (dto.DeliveryWarehouseId != null && dto.DeliveryWarehouseId.Value.ToGuid() == entity.DeliveryWarehouseId);
        }

        protected override string GetMessage(string lang)
        {
            return "valueIsReadonlyForPooling".Translate(lang, Field.ToLowerFirstLetter().Translate(lang));
        }
    }
}
