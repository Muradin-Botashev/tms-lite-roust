using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class OpenRunResponse : DetailedValidationResult
    {
        public List<OpenRunShipping> Shippings { get; set; }
        public List<OpenRunSkippedWaybill> SkippedWaybills { get; set; }
    }
}
