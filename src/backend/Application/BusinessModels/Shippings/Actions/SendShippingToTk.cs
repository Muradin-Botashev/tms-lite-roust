using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Shippings.Actions
{
    /// <summary>
    /// Отправить перевозку в ТК
    /// </summary>
    public class SendShippingToTk : IAppAction<Shipping>
    {
        private readonly ISendShippingService _sendShippingService;

        public AppColor Color { get; set; }

        public SendShippingToTk(ISendShippingService sendShippingService)
        {
            _sendShippingService = sendShippingService;
            Color = AppColor.Blue;
        }

        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
            _sendShippingService.SendShippingToTk(shipping);

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetRequestSent".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(Shipping shipping)
        {
            return IsAvailable(shipping.Status) && shipping.CarrierId != null;
        }

        public bool IsAvailable(ShippingState? shippingStatus)
        {
            return shippingStatus == ShippingState.ShippingCreated
                || shippingStatus == ShippingState.ShippingRejectedByTc
                || shippingStatus == ShippingState.ShippingSlotCancelled
                || shippingStatus == ShippingState.ShippingChangesAgreeing;
        }
    }
}