using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class RunRequest
    {
        public List<string> Ids { get; set; }
        public List<LookUpDto> AutogroupingTypes { get; set; }
    }
}
