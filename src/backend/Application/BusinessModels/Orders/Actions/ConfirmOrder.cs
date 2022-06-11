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
    [ActionGroup(nameof(Order)), OrderNumber(1)]
    public class ConfirmOrder : BaseOrderAction, IAppAction<Order>
    {
        private readonly IHistoryService _historyService;

        public ConfirmOrder(ICommonDataService dataService, IHistoryService historyService)
            : base(dataService)
        {
            _historyService = historyService;
            Color = AppColor.Green;
        }

        public AppColor Color { get; set; }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            order.Status = OrderState.Confirmed;

            order.IsNewForConfirmed = true;
            
            _historyService.Save(order.Id, "orderSetConfirmed", order.OrderNumber);

            return new AppResult
            {
                IsError = false,
                Message = "orderSetConfirmed".Translate(user.Language, order.OrderNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            var company = GetCompany(order);
            return company?.OrderRequiresConfirmation == true && order.Status == OrderState.Created;
        }
    }
}