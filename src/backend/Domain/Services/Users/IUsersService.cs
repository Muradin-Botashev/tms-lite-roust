using Domain.Persistables;
using Domain.Shared;
using System;

namespace Domain.Services.Users
{
    public interface IUsersService : IDictonaryService<User, UserDto, UserFilterDto>
    {
        ValidateResult SetActive(Guid id, bool active);
        OpenTokenResponseDto CreateOpenToken(Guid id);
    }
}
