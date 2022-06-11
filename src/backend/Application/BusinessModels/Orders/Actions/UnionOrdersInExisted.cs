using Application.BusinessModels.Shared.Actions;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// Объеденить заказы в существующую
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(3)]
    public class UnionOrdersInExisted : BaseOrderAction, IGroupAppAction<Order>
    {
        private readonly IShippingActionService _shippingActionService;

        public UnionOrdersInExisted(ICommonDataService dataService, IShippingActionService shippingActionService)
            : base(dataService)
        {
            _shippingActionService = shippingActionService;
            Color = AppColor.Orange;
        }
        
        public AppColor Color { get; set; }

        public bool IsSingleAllowed => false;

        public AppResult Run(CurrentUserDto user, IEnumerable<Order> orders)
        {
            return _shippingActionService.UnionOrdersInExisted(orders);
        }

        public bool IsAvailable(IEnumerable<Order> target)
        {
            return target.Count() > 1 && 
                   target.Count(x => x.Status == OrderState.InShipping) == 1 &&
                   target.All(x => x.Status == OrderState.InShipping 
                                || (IsConfirmedOrder(x) && (!x.DeliveryType.HasValue || x.DeliveryType.Value == DeliveryType.Delivery)));
        }
    }
}