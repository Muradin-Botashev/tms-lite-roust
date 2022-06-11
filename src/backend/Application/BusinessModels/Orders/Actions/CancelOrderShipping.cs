using Application.BusinessModels.Shared.Actions;
using Application.BusinessModels.Shippings.Actions;
using Application.Shared.Notifications;
using Application.Shared.Shippings;
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
    /// Отменить перевозку
    /// </summary>
    [ActionGroup(nameof(Shipping)), OrderNumber(19), ActionAccess(ActionAccess.GridOnly)]
    public class CancelOrderShipping : IAppAction<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly CancelShipping _shippingAction;

        public AppColor Color { get; set; }

        public CancelOrderShipping(
            ICommonDataService dataService, 
            IHistoryService historyService, 
            IShippingCalculationService calculationService,
            INotificationService notificationService)
        {
            _dataService = dataService;
            _shippingAction = new CancelShipping(dataService, historyService, calculationService, notificationService);
            Color = _shippingAction.Color;
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

            return _shippingAction.Run(user, shipping);
        }

        public bool IsAvailable(Order order)
        {
            return _shippingAction.IsAvailable(order.OrderShippingStatus);
        }
    }
}