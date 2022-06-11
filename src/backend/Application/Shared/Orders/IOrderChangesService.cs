using Domain.Persistables;
using Domain.Services.Orders;
using System.Collections.Generic;

namespace Application.Shared.Orders
{
    public interface IOrderChangesService
    {
        bool ClearBacklightFlags(IEnumerable<Order> entities, Role role);
        void MapFromDtoToEntity(Order entity, OrderDto dto);
    }
}