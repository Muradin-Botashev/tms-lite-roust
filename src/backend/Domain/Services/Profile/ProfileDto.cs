using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Profile
{
    public class ProfileDto
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<LookUpDto> Notifications { get; set; }
    }
}