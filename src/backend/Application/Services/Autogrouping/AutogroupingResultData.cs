using Domain.Persistables;
using System.Collections.Generic;

namespace Application.Services.Autogrouping
{
    public class AutogroupingResultData
    {
        public List<AutogroupingOrder> Orders { get; set; }
        public List<AutogroupingShipping> Shippings { get; set; }
        public List<AutogroupingCost> Costs { get; set; }
    }
}
