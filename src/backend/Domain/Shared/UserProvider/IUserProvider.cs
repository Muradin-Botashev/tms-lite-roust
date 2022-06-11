using System;

namespace Domain.Shared.UserProvider
{
    public interface IUserProvider
    {
        Guid? GetCurrentUserId();
        CurrentUserDto GetCurrentUser();
    }
}