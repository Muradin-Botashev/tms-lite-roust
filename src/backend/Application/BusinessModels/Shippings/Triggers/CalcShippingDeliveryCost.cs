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
    public class CalcShippingDeliveryCost : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly IDeliveryCostCalcService _calcService;
        private readonly IShippingCalculationService _shippingCalculationService;

        public CalcShippingDeliveryCost(
            ICommonDataService dataService, 
            IDeliveryCostCalcService calcService,
            IShippingCalculationService shippingCalculationService)
        {
            _dataService = dataService;
            _calcService = calcService;
            _shippingCalculationService = shippingCalculationService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var shippingIds = changes.Select(x => x.Entity.Id).ToList();
            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                         .GroupBy(x => x.ShippingId)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var entity in changes.Select(x => x.Entity))
            {
                List<Order> shippingOrders = null;
                ordersDict.TryGetValue(entity.Id, out shippingOrders);

                _calcService.UpdateDeliveryCost(entity, shippingOrders);
                _shippingCalculationService.RecalculateDeliveryCosts(entity, shippingOrders);
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(
                x => x.CarrierId,
                x => x.BodyTypeId,
                x => x.IsPooling,
                x => x.TarifficationType,
                x => x.VehicleTypeId);
        }
    }
}
