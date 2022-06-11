using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Roles
{
    public class RoleActionsDto
    {
        public IEnumerable<LookUpDto> OrderActions { get; set; }
        public IEnumerable<LookUpDto> ShippingActions { get; set; }
    }
}
