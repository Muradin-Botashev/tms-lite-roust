using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers.Dictionary
{
    [Route("api/users")]
    public class UsersController : DictionaryController<IUsersService, User, UserDto, UserFilterDto>
    {
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

        [HttpPost("newOpen/{id}")]
        public IActionResult CreateOpenToken(Guid id)
        {
            try
            {
                var result = _service.CreateOpenToken(id);
                if (result.IsError)
                {
                    return BadRequest(result);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Create new Open token");
                return StatusCode(500, ex.Message);
            }
        }

        public UsersController(IUsersService usersService, IAppConfigurationService appConfigurationService) : base(usersService, appConfigurationService)
        {
        }
    }
}