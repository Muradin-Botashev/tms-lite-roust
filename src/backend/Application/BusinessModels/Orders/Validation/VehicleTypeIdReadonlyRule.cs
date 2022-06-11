using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Validation
{
    public class VehicleTypeIdReadonlyRule : BaseReadonlyRule
    {
        private ICommonDataService _dataService;

        public VehicleTypeIdReadonlyRule(ICommonDataService dataService, IUserProvider userProvider) : base(userProvider)
        {
            _dataService = dataService;
        }

        protected override string Field => nameof(OrderDto.VehicleTypeId);

        protected override bool ChangeCheck(OrderDto dto, Order entity)
        {
            if (entity == null || dto == null
                || entity.OrderShippingStatus != ShippingState.ShippingSlotBooked
                || entity.TarifficationType != TarifficationType.Pooling
                || (dto.VehicleTypeId == null && entity.VehicleTypeId == null)
                || (dto.VehicleTypeId != null && dto.VehicleTypeId.Value.ToGuid() == entity.VehicleTypeId))
            {
                return true;
            }

            var dtoVehicleTypeId = dto.VehicleTypeId?.Value.ToGuid();
            var dtoVehicleType = dtoVehicleTypeId == null ? null : _dataService.GetById<VehicleType>(dtoVehicleTypeId.Value);
            var entityVehicleType = entity.VehicleTypeId == null ? null : _dataService.GetById<VehicleType>(entity.VehicleTypeId.Value);

            return (dtoVehicleType?.BodyTypeId == null && entityVehicleType?.BodyTypeId == null)
                || dtoVehicleType?.BodyTypeId == entityVehicleType?.BodyTypeId;
        }

        protected override string GetMessage(string lang)
        {
            return "bodyTypeIsReadonlyForPooling".Translate(lang, Field.ToLowerFirstLetter().Translate(lang));
        }
    }
}
