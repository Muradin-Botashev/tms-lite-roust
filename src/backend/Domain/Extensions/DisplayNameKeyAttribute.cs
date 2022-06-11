using System;

namespace Domain.Extensions
{
    public class DisplayNameKeyAttribute : Attribute
    {
        public string Key { get; set; }

        public DisplayNameKeyAttribute(string key)
        {
            Key = key;
        }
    }
}
