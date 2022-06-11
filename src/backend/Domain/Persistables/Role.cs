using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class Role : IPersistableWithName, ICompanyPersistable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public int[] Permissions { get; set; }
        public string[] Actions { get; set; }
        public int[] Backlights { get; set; }

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