using Domain.Enums;
using System;

namespace Domain.Persistables
{
    public class Company : IPersistable, ICompanyPersistable
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public PoolingProductType? PoolingProductType { get; set; }

        public string PoolingToken { get; set; }

        public TarifficationType? NewShippingTarifficationType { get; set; }

        public bool? OrderRequiresConfirmation { get; set; }

        public bool IsActive { get; set; }

        public Guid? CompanyId => Id;

        public override string ToString()
        {
            return Name;
        }
    }
}
