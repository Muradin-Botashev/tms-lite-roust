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
    /// Вернуть в предыдущий статус
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(14)]
    public class RollbackOrder : BaseOrderAction, IAppAction<Order>
    {
        private readonly IHistoryService _historyService;

        public RollbackOrder(ICommonDataService dataService, IHistoryService historyService)
            : base(dataService)
        {
            _historyService = historyService;
            Color = AppColor.Grey;
        }
        
        public AppColor Color { get; set; }
        public AppResult Run(CurrentUserDto user, Order order)
        {
            var newState = new OrderState?();

            var company = GetCompany(order);
            
            if (order.Status == OrderState.Canceled)
                newState = OrderState.Created;

            if (order.Status == OrderState.Confirmed)
                newState = OrderState.Created;

            if (order.Status == OrderState.Shipped)
            {
                if (!order.DeliveryType.HasValue || order.DeliveryType == DeliveryType.Delivery)
                    newState = OrderState.InShipping;
                else
                    newState = company?.OrderRequiresConfirmation == true ? OrderState.Confirmed : OrderState.Created;
            }
            
            
            if (order.Status == OrderState.Delivered)
                newState = OrderState.Shipped;

            if (order.Status == OrderState.Archive)
            {
                newState = order.DeliveryType == DeliveryType.SelfDelivery ? OrderState.Shipped : OrderState.Delivered;
            }
            
            if (newState.HasValue)
            {
                order.Status = newState.Value;
                order.IsNewForConfirmed = false;
                order.IsNewCarrierRequest = false;
                
                _historyService.Save(order.Id, "orderRollback", 
                    order.OrderNumber, 
                    newState.FormatEnum());
            }

            string newStateName = newState.FormatEnum().Translate(user.Language);
            return new AppResult
            {
                IsError = false,
                Message = "orderRollback".Translate(user.Language, 
                    order.OrderNumber,
                    newStateName)
            };
        }

        public bool IsAvailable(Order order)
        {
            return order.Status == OrderState.Confirmed ||
                   order.Status == OrderState.Shipped ||
                   order.Status == OrderState.Delivered ||
                   order.Status == OrderState.Canceled ||
                   order.Status == OrderState.Archive;
        }
    }
}