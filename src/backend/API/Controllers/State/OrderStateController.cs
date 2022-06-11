using API.Models;
using Domain.Extensions;
using Domain.Services.OrderStates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers.State
{
    [Route("api/orderState")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class OrderStateController : Controller
    {
        private readonly IOrderStateService _stateService;

        public OrderStateController(IOrderStateService stateService)
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
                var result = _stateService.GetAll();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get all OrderState");
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
                var result = _stateService.ForSelect();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to For select OrderState");
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
    }
}