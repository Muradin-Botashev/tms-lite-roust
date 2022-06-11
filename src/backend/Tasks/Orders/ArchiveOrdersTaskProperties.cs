using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Common;

namespace Tasks.Orders
{
    public class ArchiveOrdersTaskProperties : PropertiesBase
    {
        public string ExpirationPeriod { get; set; }
    }
}
