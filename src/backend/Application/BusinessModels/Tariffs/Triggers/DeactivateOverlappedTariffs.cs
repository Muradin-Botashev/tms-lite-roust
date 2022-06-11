using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Tariffs.Triggers
{
    [TriggerCategory(TriggerCategory.Preparation)]
    public class DeactivateOverlappedTariffs : ITrigger<Tariff>
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        private List<Tariff> _tariffsCache = null;

        public DeactivateOverlappedTariffs(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public void Execute(IEnumerable<EntityChanges<Tariff>> changes)
        {
            EnsureCache();

            foreach (var entity in changes.Select(x => x.Entity))
            {
                var sameTariffs = _tariffsCache
                        .Where(i =>
                            i.Id != entity.Id
                            && i.CarrierId == entity.CarrierId
                            && i.VehicleTypeId == entity.VehicleTypeId
                            && i.BodyTypeId == entity.BodyTypeId
                            && i.TarifficationType == entity.TarifficationType
                            && i.CompanyId == entity.CompanyId
                            && ((i.ShippingWarehouseId == null && entity.ShippingWarehouseId == null) || i.ShippingWarehouseId == entity.ShippingWarehouseId)
                            && ((i.DeliveryWarehouseId == null && entity.DeliveryWarehouseId == null) || i.DeliveryWarehouseId == entity.DeliveryWarehouseId)
                            && ((i.ShipmentRegion == null && entity.ShipmentRegion == null) || i.ShipmentRegion == entity.ShipmentRegion)
                            && ((i.DeliveryRegion == null && entity.DeliveryRegion == null) || i.DeliveryRegion == entity.DeliveryRegion)
                            && ((i.ShipmentCity == null && entity.ShipmentCity == null) || i.ShipmentCity == entity.ShipmentCity)
                            && ((i.DeliveryCity == null && entity.DeliveryCity == null) || i.DeliveryCity == entity.DeliveryCity))
                        .ToList();

                var fullyOverlapped = sameTariffs.Where(x => entity.EffectiveDate <= x.EffectiveDate && x.ExpirationDate <= entity.ExpirationDate).ToList();
                if (fullyOverlapped.Any())
                {
                    _dataService.GetDbSet<Tariff>().RemoveRange(fullyOverlapped);
                    foreach (var tariffToRemove in fullyOverlapped)
                    {
                        _tariffsCache.Remove(tariffToRemove);
                    }
                }

                foreach (var tariff in sameTariffs.Where(x => x.EffectiveDate < entity.EffectiveDate
                                                            && entity.EffectiveDate <= x.ExpirationDate
                                                            && x.ExpirationDate <= entity.ExpirationDate))
                {
                    tariff.ExpirationDate = entity.EffectiveDate?.AddDays(-1);
                }

                foreach (var tariff in sameTariffs.Where(x => entity.EffectiveDate <= x.EffectiveDate
                                                            && x.EffectiveDate <= entity.ExpirationDate
                                                            && entity.ExpirationDate < x.ExpirationDate))
                {
                    tariff.EffectiveDate = entity.ExpirationDate?.AddDays(1);
                }
            }
        }

        public IEnumerable<EntityChanges<Tariff>> FilterTriggered(IEnumerable<EntityChanges<Tariff>> changes)
        {
            return changes.FilterChanged(
                x => x.EffectiveDate, 
                x => x.ExpirationDate);
        }

        private void EnsureCache()
        {
            if (_tariffsCache == null)
            {
                var companyId = _userProvider.GetCurrentUser()?.CompanyId;
                _tariffsCache = _dataService.GetDbSet<Tariff>()
                                              .Where(x => x.CompanyId == null || companyId == null || x.CompanyId == companyId)
                                              .ToList();
            }
        }
    }
}
