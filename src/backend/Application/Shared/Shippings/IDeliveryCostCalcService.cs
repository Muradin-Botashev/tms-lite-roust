using Domain.Enums;
using Domain.Persistables;
using System;
using System.Collections.Generic;

namespace Application.Shared.Shippings
{
    public interface IDeliveryCostCalcService
    {
        void UpdateDeliveryCost(Shipping shipping, IEnumerable<Order> orders = null, bool ignoreManualCost = false);

        Tariff FindTariff(
            Shipping shipping, 
            IEnumerable<Order> orders = null, 
            Guid? carrierId = null, 
            IEnumerable<Guid> vehicleTypeIds = null,
            TarifficationType? tarifficationType = null,
            IEnumerable<Guid> ignoredCarrierIds = null);

        void UpdateDeliveryCost(Tariff tariff, Shipping shipping, IEnumerable<Order> orders = null);

        decimal? GetDeliveryCost(Tariff tariff, Shipping shipping, IEnumerable<Order> orders, out decimal? extraPointCosts);

        decimal? GetBaseDeliveryCost(Tariff tariff, Shipping shipping, IEnumerable<Order> orders);
    }
}