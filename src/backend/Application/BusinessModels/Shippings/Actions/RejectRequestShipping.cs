using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Shippings.Actions
{
    /// <summary>
    /// Отменить заявку
    /// </summary>
    public class RejectRequestShipping : IAppAction<Shipping>
    {
        private readonly IShippingActionService _shippingActionService;

        public AppColor Color { get; set; }

        public RejectRequestShipping(IShippingActionService shippingActionService)
        {
            _shippingActionService = shippingActionService;
            Color = AppColor.Red;
        }
        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
            return _shippingActionService.RejectShippingRequest(shipping);
        }

        public bool IsAvailable(Shipping shipping)
        {
            return IsAvailable(shipping.Status);
        }

        public bool IsAvailable(ShippingState? shippingStatus)
        {
            return shippingStatus == ShippingState.ShippingRequestSent;
        }
    }
}