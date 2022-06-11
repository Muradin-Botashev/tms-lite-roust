using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class AutogroupingCarrierDto : LookUpDto
    {
        public List<AlternativeCostDto> AlternativeCosts { get; set; }

        public AutogroupingCarrierDto() { }

        public AutogroupingCarrierDto(string value, string name) : base(value, name)
        {
        }
    }
}
