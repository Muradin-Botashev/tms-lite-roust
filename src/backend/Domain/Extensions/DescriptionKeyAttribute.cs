using System;

namespace Domain.Extensions
{
    public class DescriptionKeyAttribute : Attribute
    {
        public string Key { get; set; }

        public DescriptionKeyAttribute(string key)
        {
            Key = key;
        }
    }
}
