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
    /// Объеденить заказы
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(4)]
    public class UnionOrders : BaseOrderAction, IGroupAppAction<Order>
    {
        private readonly IShippingActionService _shippingActionService;

        public UnionOrders(ICommonDataService dataService, IShippingActionService shippingActionService)
            : base(dataService)
        {
            _shippingActionService = shippingActionService;
        }
        
        public AppColor Color { get; set; }

        public bool IsSingleAllowed => false;

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> orders)
        {
            var shipping = _shippingActionService.UnionOrders(orders);

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetCreated".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(IEnumerable<Order> target)
        {
            return target.All(order => IsConfirmedOrder(order) && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Delivery));
        }
    }
}