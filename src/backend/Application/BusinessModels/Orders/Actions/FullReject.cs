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
    /// Полный возврат
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(10)]
    public class FullReject : IAppAction<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;

        public FullReject(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
            Color = AppColor.Orange;
        }

        public AppColor Color { get; set; }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            order.Status = OrderState.FullReturn;

            _historyService.Save(order.Id, "orderSetFullReturn", order.OrderNumber);
            
            return new AppResult
            {
                IsError = false,
                Message = "orderSetFullReturn".Translate(user.Language, order.OrderNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            return order.Status == OrderState.Shipped && order.DeliveryType != DeliveryType.Courier
                || order.Status == OrderState.Delivered && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Courier);
        }
    }
}