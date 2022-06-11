using Domain.Extensions;
using Domain.Services.AppConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class AppConfigurationController : Controller
    {
        private readonly IAppConfigurationService appConfigurationService;

        public AppConfigurationController(IAppConfigurationService appConfigurationService)
        {
            this.appConfigurationService = appConfigurationService;
        }
        /// <summary>
        /// Получение конфигурации гридов и справочников
        /// </summary>
        [HttpGet("appConfiguration")] 
        public IActionResult Configuration()
        {
            try
            {
                var result = appConfigurationService.GetConfiguration();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get Application configuration");
                return StatusCode(500, ex.Message);
            }
        }
    }
}