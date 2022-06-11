using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// Отправить перевозку в ТК
    /// </summary>
    [ActionGroup(nameof(Shipping)), OrderNumber(15), ActionAccess(ActionAccess.GridOnly), DescriptionKey("sendOrderShippingToTkDescription")]
    public class SendOrderShippingToTk : IAppAction<Order>
    {
        private readonly ISendShippingService _sendShippingService;
        private readonly ICommonDataService _dataService;

        public AppColor Color { get; set; }

        public SendOrderShippingToTk(ICommonDataService dataService, ISendShippingService sendShippingService)
        {
            _sendShippingService = sendShippingService;
            _dataService = dataService;
            Color = AppColor.Blue;
        }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            if (order?.ShippingId == null)
            {
                return new AppResult
                {
                    IsError = true,
                    Message = "orderShippingNotFound".Translate(user.Language)
                };
            }

            var shipping = _dataService.GetById<Shipping>(order.ShippingId.Value);

            _sendShippingService.SendShippingToTk(shipping);

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetRequestSent".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            return (order.OrderShippingStatus == ShippingState.ShippingCreated
                || order.OrderShippingStatus == ShippingState.ShippingRejectedByTc
                || order.OrderShippingStatus == ShippingState.ShippingSlotCancelled
                || order.OrderShippingStatus == ShippingState.ShippingChangesAgreeing)
                && order.CarrierId != null;
        }
    }
}