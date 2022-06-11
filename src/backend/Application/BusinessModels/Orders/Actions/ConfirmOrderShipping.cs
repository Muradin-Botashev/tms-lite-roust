﻿using Application.BusinessModels.Shared.Actions;
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
    [ActionGroup(nameof(Shipping)), OrderNumber(20), ActionAccess(ActionAccess.GridOnly)]
    public class ConfirmOrderShipping : IAppAction<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly ConfirmShipping _shippingAction;

        public AppColor Color { get; set; }

        public ConfirmOrderShipping(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _shippingAction = new ConfirmShipping(dataService, historyService);
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