using Domain.Enums;
using Domain.Extensions;

namespace Domain.Services.Profile
{
    public class SetNewPasswordDto
    {
        [FieldType(FieldType.Text),  IsRequired]
        public string OldPassword { get; set; }

        [FieldType(FieldType.Password), IsRequired]
        public string NewPassword { get; set; }
    }
}