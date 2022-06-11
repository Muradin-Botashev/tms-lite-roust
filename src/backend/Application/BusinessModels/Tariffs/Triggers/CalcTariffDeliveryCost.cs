using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Tariffs.Triggers
{
    [TriggerCategory(TriggerCategory.Calculation)]
    public class CalcTariffDeliveryCost : ITrigger<Tariff>
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IDeliveryCostCalcService _calcService;
        private readonly IShippingCalculationService _shippingCalculationService;

        private List<Shipping> _shippingsCache = null;

        public CalcTariffDeliveryCost(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            IDeliveryCostCalcService calcService,
            IShippingCalculationService shippingCalculationService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _calcService = calcService;
            _shippingCalculationService = shippingCalculationService;
        }

        public void Execute(IEnumerable<EntityChanges<Tariff>> changes)
        {
            EnsureCache();

            foreach (var entity in changes.Select(x => x.Entity).GroupBy(x => new { x.CarrierId, x.TarifficationType }))
            {
                var shippings = _shippingsCache
                                    .Where(x => x.CarrierId == entity.Key.CarrierId
                                            && x.TarifficationType == entity.Key.TarifficationType)
                                    .ToList();

                var shippingIds = changes.Select(x => x.Entity).Select(x => x.Id).ToList();
                var ordersDict = _dataService.GetDbSet<Order>()
                                             .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                             .GroupBy(x => x.ShippingId)
                                             .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var shipping in shippings)
                {
                    List<Order> orders = null;
                    ordersDict.TryGetValue(shipping.Id, out orders);

                    _calcService.UpdateDeliveryCost(shipping, orders);
                    _shippingCalculationService.RecalculateDeliveryCosts(shipping, orders);
                }
            }
        }

        public IEnumerable<EntityChanges<Tariff>> FilterTriggered(IEnumerable<EntityChanges<Tariff>> changes)
        {
            return changes?.Where(x => x.FieldChanges?.Count > 0);
        }

        private void EnsureCache()
        {
            if (_shippingsCache == null)
            {
                var companyId = _userProvider.GetCurrentUser()?.CompanyId;
                _shippingsCache = _dataService.GetDbSet<Shipping>()
                                              .Where(x => (x.CompanyId == companyId || companyId == null)
                                                    && (x.Status == ShippingState.ShippingCreated
                                                        || x.Status == ShippingState.ShippingRequestSent
                                                        || x.Status == ShippingState.ShippingRejectedByTc
                                                        || x.Status == ShippingState.ShippingSlotBooked
                                                        || x.Status == ShippingState.ShippingSlotCancelled
                                                        || x.Status == ShippingState.ShippingChangesAgreeing))
                                              .ToList();
            }
        }
    }
}
