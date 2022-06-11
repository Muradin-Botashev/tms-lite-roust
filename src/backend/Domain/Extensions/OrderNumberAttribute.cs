using System;

namespace Domain.Extensions
{
    public class OrderNumberAttribute : Attribute
    {
        public int Value { get; set; }

        public OrderNumberAttribute(int orderNumber)
        {
            Value = orderNumber;
        }
    }
}
