using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class VehicleType : IPersistableWithName, ICompanyPersistable
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        [ReferenceType(typeof(Tonnage))]
        public Guid? TonnageId { get; set; }

        [SortKey(nameof(Persistables.Tonnage.Name))]
        public Tonnage Tonnage { get; set; }

        [ReferenceType(typeof(BodyType))]
        public Guid? BodyTypeId { get; set; }

        [SortKey(nameof(Persistables.BodyType.Name))]
        public BodyType BodyType { get; set; }

        public int? PalletsCount { get; set; }

        public bool? IsInterregion { get; set; }

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
