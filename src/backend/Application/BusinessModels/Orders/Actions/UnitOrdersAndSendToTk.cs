using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// Отправить перевозку в ТК
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(15), ActionAccess(ActionAccess.GridOnly), DescriptionKey("unitOrdersAndSendToTkDescription")]
    public class UnitOrdersAndSendToTk : BaseOrderAction, IGroupAppAction<Order>
    {
        private readonly IShippingActionService _shippingActionService;
        private readonly ISendShippingService _sendShippingService;

        public AppColor Color { get; set; }

        public bool IsSingleAllowed => true;

        public UnitOrdersAndSendToTk(IShippingActionService shippingActionService, ISendShippingService sendShippingService, ICommonDataService dataService)
            : base(dataService)
        {
            _shippingActionService = shippingActionService;
            _sendShippingService = sendShippingService;
            Color = AppColor.Blue;
        }

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> orders)
        {
            var shipping = _shippingActionService.UnionOrders(orders);

            _sendShippingService.SendShippingToTk(shipping, orders.ToList());

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetCreated".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(IEnumerable<Order> orders)
        {
            return orders.All(order => IsConfirmedOrder(order) 
                && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Delivery)
                && (order.CarrierId != null)
            );
        }
    }
}