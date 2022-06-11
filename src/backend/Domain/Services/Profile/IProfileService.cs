using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Profile
{
    public interface IProfileService : IService
    {
        ProfileDto GetProfile();
        ValidateResult Save(SaveProfileDto dto);
        IEnumerable<LookUpDto> GetAllNotifications();
    }
}