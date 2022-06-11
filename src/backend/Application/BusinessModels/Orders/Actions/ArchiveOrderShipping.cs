using Application.BusinessModels.Shared.Actions;
using Application.BusinessModels.Shippings.Actions;
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
    [ActionGroup(nameof(Shipping)), OrderNumber(25), ActionAccess(ActionAccess.GridOnly), DescriptionKey("archiveOrderShippingDescription")]
    public class ArchiveOrderShipping : IAppAction<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly ArchiveShipping _shippingAction;

        public AppColor Color { get; set; }

        public ArchiveOrderShipping(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _shippingAction = new ArchiveShipping(dataService, historyService);
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