using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Profile
{
    public class SaveProfileDto
    {
        public string Email { get; set; }
        public string UserName { get; set; }

        [FieldType(FieldType.Text)]
        public string OldPassword { get; set; }

        [FieldType(FieldType.Password)]
        public string NewPassword { get; set; }

        public IEnumerable<LookUpDto> Notifications { get; set; }
    }
}