using System;

namespace Domain.Extensions
{
    public class ReferenceTypeAttribute : Attribute
    {
        public Type Type { get; set; }

        public ReferenceTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
