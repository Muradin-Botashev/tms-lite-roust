using AutoMapper;
using DAL;
using DAL.Queries;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.Shared.UserProvider
{

    public class UserProvider : IUserProvider
    {
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppDbContext db;

        private CurrentUserDto _cachedUser = null;

        public UserProvider(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            this.httpContextAccessor = httpContextAccessor;
            db = dbContext;
            mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, CurrentUserDto>()).CreateMapper();
        }

        public Guid? GetCurrentUserId()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var userIdClaim = httpContext.User.FindFirst("userId");
            return userIdClaim != null
                ? Guid.Parse(userIdClaim.Value)
                : (Guid?)null;
        }

        public CurrentUserDto GetCurrentUser()
        {
            if (_cachedUser != null)
            {
                return _cachedUser;
            }

            User user = db.Users.GetById(EnsureCurrentUserId());
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException();
            }

            CurrentUserDto dto = mapper.Map<CurrentUserDto>(user);
            dto.Language = GetCurrentUserLanguage();

            _cachedUser = dto;
            return dto;
        }
        
        public Guid EnsureCurrentUserId()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                throw new UnauthorizedAccessException("Невозможно определить текущего пользователя");

            return userId.Value;
        }

        private string GetCurrentUserLanguage()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var langClaim = httpContext.User.FindFirst("lang");
            string lang = langClaim?.Value ?? "ru";
            return lang;
        }
    }
}