using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class OpenRunRequest
    {
        public List<OpenRunWaybill> Waybills { get; set; }
        public List<string> AutogroupingTypes { get; set; }
    }
}
