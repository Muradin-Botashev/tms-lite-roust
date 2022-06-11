using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.SyncFields)]
    public class SyncBodyType : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;

        public SyncBodyType(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            foreach (var group in changes.Select(x => x.Entity).GroupBy(x => x.VehicleTypeId))
            {
                var firstEntity = group.First();
                var vehicleType = firstEntity.VehicleTypeId == null ? null : _dataService.GetById<VehicleType>(firstEntity.VehicleTypeId.Value);
                foreach (var entity in group)
                {
                    entity.BodyTypeId = vehicleType?.BodyTypeId;
                }
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(x => x.VehicleTypeId);
        }
    }
}
