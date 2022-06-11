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
    /// Создать перевозку
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(2)]
    public class CreateShipping : BaseOrderAction, IAppAction<Order>
    {
        private readonly IShippingActionService _shippingActionService;

        public CreateShipping(ICommonDataService dataService, IShippingActionService shippingActionService)
            : base(dataService)
        {
            _shippingActionService = shippingActionService;
            Color = AppColor.Blue;
        }

        public AppColor Color { get; set; }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            var shipping = _shippingActionService.UnionOrders(new[] { order });

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetCreated".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            return IsConfirmedOrder(order) && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Delivery);
        }
    }
}