using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.SyncFields)]
    public class SyncVehicleType : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;

        public SyncVehicleType(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            foreach (var group in changes.Select(x => x.Entity).GroupBy(x => new { x.BodyTypeId, x.VehicleTypeId }))
            {
                var firstEntity = group.First();

                var currentVehicleType = firstEntity.VehicleTypeId == null ? null : _dataService.GetById<VehicleType>(firstEntity.VehicleTypeId.Value);
                VehicleType newVehicleType = null;
                if (currentVehicleType != null && firstEntity.BodyTypeId != null)
                {
                    newVehicleType = _dataService.GetDbSet<VehicleType>()
                                                 .FirstOrDefault(x => x.BodyTypeId == firstEntity.BodyTypeId
                                                                    && x.TonnageId == currentVehicleType.TonnageId
                                                                    && x.IsActive);
                }

                foreach (var entity in group)
                {
                    entity.VehicleTypeId = newVehicleType?.Id;
                }
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(x => x.BodyTypeId);
        }
    }
}
