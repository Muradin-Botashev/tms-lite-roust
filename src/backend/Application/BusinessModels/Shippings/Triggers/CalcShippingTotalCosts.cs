using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.Calculation)]
    public class CalcShippingTotalCosts : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly IShippingCalculationService _calculationService;

        public CalcShippingTotalCosts(ICommonDataService dataService, IShippingCalculationService calculationService)
        {
            _dataService = dataService;
            _calculationService = calculationService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var shippingIds = changes.Select(x => x.Entity.Id).ToList();
            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                         .GroupBy(x => x.ShippingId)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var change in changes)
            {
                var shipping = change.Entity;

                List<Order> orders = null;
                ordersDict.TryGetValue(shipping.Id, out orders);

                _calculationService.RecalculateTotalCosts(shipping, orders);
                _calculationService.RecalculateShippingOrdersCosts(shipping, orders);
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(
                x => x.AdditionalCostsWithoutVAT,
                x => x.DowntimeRate,
                x => x.ExtraPointCostsWithoutVAT,
                x => x.OtherCosts,
                x => x.ReturnCostWithoutVAT);
        }
    }
}
