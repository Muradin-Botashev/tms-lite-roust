using System;

namespace Domain.Persistables
{
    public class FieldPropertyItemVisibility : FieldPropertyItemBase, IPersistable
    {
        public bool IsHidden { get; set; }
    }
}