using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class AutogroupingSetting : IPersistable
    {
        public Guid Id { get; set; }

        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        public int? MaxUnloadingPoints { get; set; }

        public bool? CheckPoolingSlots { get; set; }

        public decimal? RegionOverrunCoefficient { get; set; }

        public decimal? InterregionOverrunCoefficient { get; set; }

        [ReferenceType(typeof(Tonnage))]
        public Guid? TonnageId { get; set; }

        [SortKey(nameof(Persistables.Tonnage.Name))]
        public Tonnage Tonnage { get; set; }
    }
}
