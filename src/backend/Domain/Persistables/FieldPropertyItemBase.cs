using System;
using Domain.Enums;

namespace Domain.Persistables
{
    public class FieldPropertyItemBase : IComparable, ICompanyPersistable
    {
        public Guid Id { get; set; }
        
        public FieldPropertiesForEntityType ForEntity { get; set; }

        public Guid? CompanyId { get; set; }

        public Guid? RoleId { get; set; }

        public string FieldName { get; set; }
        
        public int CompareTo(object o)
        {
            var nextItem = o as FieldPropertyItemBase;
            if (nextItem != null)
                return GetPoints(nextItem) - GetPoints(this);
            else
                return -1;
        }

        private int GetPoints(FieldPropertyItemBase item)
        {
            int result = 0;
            if (item.RoleId != null) result += 2;
            if (item.CompanyId != null) result += 1;
            return result;
        }
    }
}