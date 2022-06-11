using Application.Shared.Pooling.Models;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Pooling.Models;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;

namespace Application.Shared.Orders
{
    public interface IOrderPoolingService
    {
        //IEnumerable<string> CheckRequiredFields(IEnumerable<Order> order);

        //IEnumerable<string> CheckRelatedValues(IEnumerable<Order> orders);

        //IEnumerable<string> CheckFieldsMatch(IEnumerable<Order> orders);

        HttpResult<SlotDto> GetSlot(Shipping shipping, Order order);

        HttpResult<SlotDto> GetSlot(string slotId, Guid? companyId);

        HttpResult<ReservationRequestDto> BookSlot(Shipping shipping, IEnumerable<Order> orders, SlotDto slot);

        HttpResult<ReservationRequestDto> UpdateSlot(Shipping shipping, IEnumerable<Order> orders = null);

        HttpResult CancelSlot(Shipping shipping);

        AppResult ValidateGetSlot(HttpResult<SlotDto> slot, CurrentUserDto user);

        AppResult ValidateBookedSlot(HttpResult<ReservationRequestDto> slot, CurrentUserDto user);

        AppResult ValidateCancelSlot(HttpResult result, Shipping shipping, CurrentUserDto user);

        AppResult ValidateOrders(IEnumerable<Order> orders, CurrentUserDto user);

        bool CheckConsolidationDate(Shipping shipping);

    }
}
