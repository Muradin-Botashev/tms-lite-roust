using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Shared.UserProvider;
using System.Linq;

namespace Application.BusinessModels.Shippings.Actions
{
    /// <summary>
    /// В архив
    /// </summary>
    [ActionGroup(nameof(Shipping)), OrderNumber(16)]
    public class SendToPooling : BaseShippingAction, IAppAction<Shipping>
    {
        private readonly ISendShippingService _sendShippingService;

        public SendToPooling(
            ICommonDataService dataService,
            ISendShippingService sendShippingService)
            : base(dataService)
        {
            _sendShippingService = sendShippingService;

            Color = AppColor.Blue;
        }

        public AppColor Color { get; set; }

        public bool IsSingleAllowed => true;

        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
            return _sendShippingService.SendShippingToPooling(user, shipping);
        }
        
        public bool IsAvailable(Shipping shipping)
        {
            var orders = _dataService.GetDbSet<Order>().Where(i => i.ShippingId == shipping.Id).ToList();

            return orders.All(i => i.ShippingId == shipping.Id)
                && orders.All(i => i.DeliveryType == DeliveryType.Delivery)
                && (orders.Any() && orders.All(i => GetOrderCompany(i)?.OrderRequiresConfirmation == true ? i.Status == OrderState.Confirmed : i.Status == OrderState.Created) 
                    || (shipping.Status == ShippingState.ShippingCreated)
                    || (shipping.Status == ShippingState.ShippingSlotCancelled)
                    )
                && orders.All(i => i.CarrierId.HasValue && i.CarrierId == orders.First().CarrierId);
        }
    }
}