using API.Models;
using Domain.Extensions;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class StateController<T> :  Controller
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        /// <summary>
        /// Все доступные статусы с цветами
        /// </summary>
        [HttpPost("search")]
        public IActionResult GetAll()
        {
            try
            {
                var result = _stateService.GetAll<T>();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get all {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Все доступные статусы
        /// </summary>
        [HttpGet("forSelect")]
        public IActionResult ForSelect()
        {
            try
            {
                var result = _stateService.ForSelect<T>();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to For select {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Все доступные статусы
        /// </summary>
        [HttpPost("forSelect")]
        public IActionResult ForSelect([FromBody] CompanyFilter request)
        {
            return ForSelect();
        }

        private string EntityName => typeof(T).Name;
    }
}