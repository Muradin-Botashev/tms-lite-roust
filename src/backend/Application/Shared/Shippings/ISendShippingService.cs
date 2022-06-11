using Domain.Persistables;
using Domain.Services;
using Domain.Shared.UserProvider;
using System.Collections.Generic;

namespace Application.Shared.Shippings
{
    public interface ISendShippingService
    {
        AppResult SendShippingToPooling(CurrentUserDto user, Shipping shipping, List<Order> orders = null);
        Shipping SendShippingToTk(Shipping shipping, List<Order> orders = null);
    }
}