using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class AutogroupingTypesDto
    {
        public List<LookUpDto> All { get; set; }
        public List<LookUpDto> Selected { get; set; }
    }
}
