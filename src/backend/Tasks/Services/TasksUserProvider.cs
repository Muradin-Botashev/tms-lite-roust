using Domain.Shared.UserProvider;
using System;

namespace Tasks.Services
{
    public class TasksUserProvider : IUserProvider
    {
        public CurrentUserDto GetCurrentUser()
        {
            return new CurrentUserDto
            {
                Id = null,
                RoleId = null,
                CompanyId = null,
                Name = "System",
                Language = "ru"
            };
        }

        public Guid? GetCurrentUserId()
        {
            return null;
        }
    }
}
