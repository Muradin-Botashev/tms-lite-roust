using API.Models;
using Domain.Extensions;
using Domain.Services.UserSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    /// <summary>
    /// Пользовательские настройки
    /// </summary>
    [Route("api/userSettings")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class UserSettingsController : Controller
    {
        private readonly IUserSettingsService _settingsService;

        public UserSettingsController(IUserSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Получение значения пользовательской настройки
        /// </summary>
        /// <param name="key">Ключ для настроек</param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public IActionResult GetValue(string key)
        {
            try
            {
                var result = _settingsService.GetValue(key);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get user settings");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Сохранение пользовательской настройки
        /// </summary>
        /// <param name="key">Ключ для настроек</param>
        /// <param name="value">Новое значение</param>
        /// <returns></returns>
        [HttpPost("{key}")]
        public IActionResult SetValue(string key, [FromBody]UserSettingValueDto value)
        {
            try
            {
                var result = _settingsService.SetValue(key, value?.Value);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to save user settings");
                return StatusCode(500);
            }
        }
    }
}
