using Application.BusinessModels.Shared.Triggers;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateManualFieldFlags : ITrigger<Shipping>
    {
        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            foreach (var change in changes)
            {
                if (change.IsManuallyChanged(x => x.ActualPalletsCount))
                    change.Entity.ManualActualPalletsCount = true;

                if (change.IsManuallyChanged(x => x.ActualWeightKg))
                    change.Entity.ManualActualWeightKg = true;

                if (change.IsManuallyChanged(x => x.ConfirmedPalletsCount))
                    change.Entity.ManualConfirmedPalletsCount = true;

                if (change.IsManuallyChanged(x => x.PalletsCount))
                    change.Entity.ManualPalletsCount = true;

                if (change.IsManuallyChanged(x => x.TarifficationType))
                    change.Entity.ManualTarifficationType = true;

                if (change.IsManuallyChanged(x => x.TrucksDowntime))
                    change.Entity.ManualTrucksDowntime = true;

                if (change.IsManuallyChanged(x => x.WeightKg))
                    change.Entity.ManualWeightKg = true;
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(
                x => x.ActualPalletsCount,
                x => x.ActualWeightKg,
                x => x.ConfirmedPalletsCount,
                x => x.PalletsCount,
                x => x.TarifficationType,
                x => x.TrucksDowntime,
                x => x.WeightKg);
        }
    }
}
