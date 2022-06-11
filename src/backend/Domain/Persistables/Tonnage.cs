using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class Tonnage: IPersistableWithName, ICompanyPersistable
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal? WeightKg { get; set; }

        public bool IsActive { get; set; }

        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
