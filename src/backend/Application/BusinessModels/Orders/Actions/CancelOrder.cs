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
    /// Отменить заказ
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(12)]
    public class CancelOrder : IAppAction<Order>
    {
        private readonly IHistoryService _historyService;

        private readonly ICommonDataService _dataService;

        public CancelOrder(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
            Color = AppColor.Red;
        }

        public AppColor Color { get; set; }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            order.Status = OrderState.Canceled;
            order.IsNewForConfirmed = false;

            _historyService.Save(order.Id, "orderSetCancelled", order.OrderNumber);
            
            return new AppResult
            {
                IsError = false,
                Message = "orderSetCancelled".Translate(user.Language, order.OrderNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            return order.Status == OrderState.Confirmed 
                || order.Status == OrderState.Created 
                || order.Status == OrderState.Draft;
        }
    }
}