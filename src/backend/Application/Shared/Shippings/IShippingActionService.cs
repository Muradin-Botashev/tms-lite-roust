using Domain.Persistables;
using Domain.Services;
using System.Collections.Generic;

namespace Application.Shared.Shippings
{
    public interface IShippingActionService
    {
        AppResult RejectShippingRequest(Shipping shipping, List<Order> orders = null);
        AppResult BaseRejectShippingRequest(Shipping shipping, List<Order> orders = null);

        Shipping UnionOrders(IEnumerable<Order> orders, string shippingNumber = null, bool forRegrounpping = false);

        AppResult UnionOrdersInExisted(IEnumerable<Order> orders);

        AppResult UnionOrdersInExisted(Shipping shipping, IEnumerable<Order> orders);
    }
}
