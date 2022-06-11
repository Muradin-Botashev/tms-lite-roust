using Domain.Extensions;
using Domain.Services.FieldProperties;
using Domain.Shared.UserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace API.Controllers
{
    /// <summary>
    /// Настройка полей
    /// </summary>    
    [Route("api/fieldProperties")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class FieldPropertiesController : Controller
    {
        private readonly IFieldPropertiesService _fieldPropertiesService;
        private readonly IUserProvider _userProvider;

        public FieldPropertiesController(IFieldPropertiesService fieldPropertiesService, IUserProvider userProvider)
        {
            _fieldPropertiesService = fieldPropertiesService;
            _userProvider = userProvider;
        }

        /// <summary>
        /// Получить список полей и отображения по статусам
        /// </summary>
        [HttpPost("get")]
        public IActionResult GetFor([FromBody] FieldPropertiesGetForParams getForParams)
        {
            try
            {
                var currentUser = _userProvider.GetCurrentUser();
                var companyId = getForParams.CompanyId.ToGuid() ?? currentUser.CompanyId;
                var roleId = getForParams.RoleId.ToGuid() ?? currentUser.RoleId;
                var result = _fieldPropertiesService.GetFor(getForParams.ForEntity, companyId, roleId, currentUser.Id);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to GetFor field properties");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получить права доступа для поля
        /// </summary>
        [HttpPost("getField")]
        public IActionResult GetForField([FromBody] GetForFieldPropertyParams dto)
        {
            try
            {
                var accessType = _fieldPropertiesService.GetAccessTypeForField(dto);
                return Ok(new { accessType });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to GetForField field properties");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Сохранить
        /// </summary>
        [HttpPost("save")]
        public IActionResult Save([FromBody] FieldPropertyDto fieldPropertiesDto)
        {
            try
            {
                var result = _fieldPropertiesService.Save(fieldPropertiesDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Save field properties");
                return StatusCode(500, ex.Message);
            }
        }        

        /// <summary>
        /// Переключить "Скрыто" у поля
        /// </summary>
        [HttpPost("toggleHiddenState")]
        public IActionResult ToggleHiddenState([FromBody] ToggleHiddenStateDto dto)
        {
            try
            {
                var result = _fieldPropertiesService.ToggleHiddenState(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Toggle field properties");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение списка ролей
        /// </summary>
        [HttpGet("companies")]
        public IActionResult GetCompanies()
        {
            try
            {
                var result = _fieldPropertiesService.GetCompanies().ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get companies list");
                return StatusCode(500, ex.Message);
            }
        }
    }
}