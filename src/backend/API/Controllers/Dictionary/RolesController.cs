using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Roles;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers.Dictionary
{
    [Route("api/roles")]
    public class RolesController : DictionaryController<IRolesService, Role, RoleDto, RoleFilterDto>
    {
        public RolesController(IRolesService rolesService, IAppConfigurationService appConfigurationService) : base(rolesService, appConfigurationService)
        {
        }

        /// <summary>
        /// ѕолучение данных дл€ выпадающего списка в 
        /// </summary>
        [HttpPost("setActive/{id}/{active}")]
        public IActionResult SetActive(Guid id, bool active)
        {
            try
            {
                var result = _service.SetActive(id, active);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to Change active for User");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// ѕолучение списка всех доступных разрешений
        /// </summary>
        [HttpGet("allPermissions")]
        public IActionResult GetAllPermissions()
        {
            try
            {
                var result = _service.GetAllPermissions();

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to get permissions list");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// ѕолучение списка всех доступных действий
        /// </summary>
        [HttpGet("allActions")]
        public IActionResult GetAllActions()
        {
            try
            {
                var result = _service.GetAllActions();

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to get actions list");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// ѕолучение списка всех доступных подсветок
        /// </summary>
        [HttpGet("allBacklights")]
        public IActionResult GetAllBacklights()
        {
            try
            {
                var result = _service.GetAllBacklights();

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to get backlights list");
                return StatusCode(500);
            }
        }
    }
}