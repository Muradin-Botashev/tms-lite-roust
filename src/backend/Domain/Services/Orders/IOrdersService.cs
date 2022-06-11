using Domain.Persistables;
using Domain.Shared;
using Domain.Shared.FormFilters;
using System.Collections.Generic;

namespace Domain.Services.Orders
{
    public interface IOrdersService : IGridService<Order, OrderDto, OrderFormDto, OrderSummaryDto, OrderFilterDto>
    {
        OrderFormDto GetFormByNumber(string orderNumber);

        IEnumerable<LookUpDto> FindByNumber(NumberSearchFormDto dto);
    }
}