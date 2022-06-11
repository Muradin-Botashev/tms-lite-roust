using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    public class SortKeyAttribute: Attribute
    {
        public string SortKey { get; set; }

        public SortKeyAttribute(string sortKey)
        {
            SortKey = sortKey;
        }
    }
}
