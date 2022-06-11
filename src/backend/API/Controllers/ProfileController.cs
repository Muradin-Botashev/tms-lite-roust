using Domain.Extensions;
using Domain.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api/profile")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class ProfileController : Controller
    {
        private readonly IProfileService profileService;

        public ProfileController(IProfileService profileService)
        {
            this.profileService = profileService;
        }

        /// <summary>
        /// Получение профиля текущего пользователя
        /// </summary>
        [HttpGet("info")] 
        public IActionResult Info()
        {
            try
            {
                var result = profileService.GetProfile();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get profile info");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Сохранение новой информации о пользователе
        /// </summary>
        [HttpPost("save")] 
        public IActionResult Save([FromBody]SaveProfileDto dto)
        {
            try
            {
                var result = profileService.Save(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to save profile");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение списка всех доступных разрешений
        /// </summary>
        [HttpGet("allNotifications")]
        public IActionResult GetAllNotifications()
        {
            try
            {
                var result = profileService.GetAllNotifications();

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to get notifications list");
                return StatusCode(500);
            }
        }
    }
}