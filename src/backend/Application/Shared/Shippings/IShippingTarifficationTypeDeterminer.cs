using System.Collections.Generic;
using Domain.Enums;
using Domain.Persistables;

namespace Application.Shared.Shippings
{
    public interface IShippingTarifficationTypeDeterminer
    {
        TarifficationType GetTarifficationTypeForOrders(Shipping shipping, IEnumerable<Order> orders);
    }
}