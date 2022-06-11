using System.Collections.Generic;
using Domain.Persistables;

namespace Application.Shared.Shippings
{
    public interface IShippingCalculationService
    {
        void RecalculateDeliveryCosts(Shipping shipping, IEnumerable<Order> orders);
        void RecalculateTotalCosts(Shipping shipping, IEnumerable<Order> orders);
        void RecalculateShippingOrdersCosts(Shipping shipping, IEnumerable<Order> orders);
        void RecalculateTemperature(Shipping shipping, IEnumerable<Order> orders);
        void RecalculateShipping(Shipping shipping, IEnumerable<Order> orders);
        void SyncVehicleType(Shipping shipping, IEnumerable<Order> orders);
        void ClearShippingOrdersCosts(IEnumerable<Order> orders);
    }
}