using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Persistables;

namespace Application.Shared.Shippings
{
    public class ShippingTarifficationTypeDeterminer : IShippingTarifficationTypeDeterminer
    {
        public TarifficationType GetTarifficationTypeForOrders(Shipping shipping, IEnumerable<Order> orders)
        {
            if (shipping?.TarifficationType != null)
            {
                return shipping.TarifficationType.Value;
            }

            var orderTariffTypes = orders.Select(x => x.TarifficationType)
                                         .Where(x => x != null)
                                         .Distinct()
                                         .ToList();
            if (orderTariffTypes.Count == 1)
            {
                return orderTariffTypes.First().Value;
            }

            if (orders.Any(x => 
                    !string.IsNullOrEmpty(x.DeliveryRegion) && 
                    (x.DeliveryRegion.Contains("Москва г") ||
                     x.DeliveryRegion.Contains("Московская обл") ||
                     x.DeliveryRegion.Contains("Новосибирская обл"))
                ) || 
                orders.Sum(x => x.PalletsCount) > 24)
                return TarifficationType.Ftl;
            
            return TarifficationType.Ltl;
        }
    }
}