using Application.BusinessModels.Shared.Actions;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// Заказ отгружен
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(5)]
    public class OrderShipped : BaseOrderAction, IAppAction<Order>
    {
        private readonly IHistoryService _historyService;

        public OrderShipped(ICommonDataService dataService, IHistoryService historyService)
            : base(dataService)
        {
            _historyService = historyService;
            Color = AppColor.Orange;
        }

        public AppColor Color { get; set; }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            order.Status = OrderState.Shipped;
            order.IsNewForConfirmed = false;

            _historyService.Save(order.Id, "orderSetShipped", order.OrderNumber);
            
            return new AppResult
            {
                IsError = false,
                Message = "orderSetShipped".Translate(user.Language, order.OrderNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            return (order.Status == OrderState.InShipping && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Delivery)) ||
                   (IsConfirmedOrder(order) && (order.DeliveryType.HasValue && order.DeliveryType.Value == DeliveryType.SelfDelivery)) ||
                   (IsConfirmedOrder(order) && (order.DeliveryType.HasValue && order.DeliveryType.Value == DeliveryType.Courier));
        }
    }
}