using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using Application.Shared.TransportCompanies;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateBasicDeliveryCost : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly ICarrierSelectionService _carrierSelectionService;
        private readonly IDeliveryCostCalcService _deliveryCostCalcService;
        private readonly IShippingCalculationService _shippingCalculationService;

        public UpdateBasicDeliveryCost(
            ICommonDataService dataService, 
            ICarrierSelectionService carrierSelectionService,
            IDeliveryCostCalcService deliveryCostCalcService,
            IShippingCalculationService shippingCalculationService)
        {
            _dataService = dataService;
            _carrierSelectionService = carrierSelectionService;
            _deliveryCostCalcService = deliveryCostCalcService;
            _shippingCalculationService = shippingCalculationService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var shippings = changes.Where(x => IsNewCostBigger(x) && x.Entity.CarrierId != null).Select(x => x.Entity).ToList();
            var shippingIds = shippings.Select(x => x.Id).ToList();
            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                         .GroupBy(x => x.ShippingId.Value)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var shipping in shippings)
            {
                ordersDict.TryGetValue(shipping.Id, out List<Order> orders);

                var carrierId = _carrierSelectionService.FindCarrier(shipping, orders, out Tariff tariff, out CarrierSelectionType selectionType, shipping.CarrierId);
                var deliveryCost = _deliveryCostCalcService.GetBaseDeliveryCost(tariff, shipping, orders);
                var isBetterCost = deliveryCost != null && deliveryCost < shipping.BasicDeliveryCostWithoutVAT;
                if (carrierId != null && (selectionType == CarrierSelectionType.FixedDirection || isBetterCost))
                {
                    var shippingAction = new CarrierShippingAction
                    {
                        Id = Guid.NewGuid(),
                        ShippingId = shipping.Id,
                        CarrierId = shipping.CarrierId,
                        ActionName = "Изменение суммы тарифа",
                        ActionTime = DateTime.Now
                    };
                    _dataService.GetDbSet<CarrierShippingAction>().Add(shippingAction);

                    _carrierSelectionService.UpdateCarrier(shipping, orders, carrierId.Value, tariff);
                }
                else
                {
                    shipping.Status = ShippingState.ShippingChangesAgreeing;
                    _shippingCalculationService.RecalculateTotalCosts(shipping, orders);
                }
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(
                x => x.BasicDeliveryCostWithoutVAT);
        }

        private bool IsNewCostBigger(EntityChanges<Shipping> changes)
        {
            var isNewCarrier = changes.IsChanged(x => x.CarrierId);
            var change = changes.FieldChanges.FirstOrDefault(x => x.FieldName.ToLower() == nameof(Shipping.BasicDeliveryCostWithoutVAT).ToLower());
            var oldValue = change?.OldValue as decimal?;
            var newValue = change?.NewValue as decimal?;
            return !isNewCarrier && newValue != null && oldValue != null && oldValue > 0 && newValue > oldValue;
        }
    }
}
