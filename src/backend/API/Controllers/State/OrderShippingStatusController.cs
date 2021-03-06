using API.Models;
using Domain.Extensions;
using Domain.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace API.Controllers.State
{
    [Route("api/orderShippingStatus")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class OrderShippingStatusController : Controller
    {
        private readonly IOrderShippingStatusService _stateService;

        public OrderShippingStatusController(IOrderShippingStatusService stateService)
        {
            _stateService = stateService;
        }

        /// <summary>
        /// ??? ????????? ??????? ? ???????
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
                Log.Error(ex, $"Failed to get all Order Shipping Statuses");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// ??? ????????? ???????
        /// </summary>
        [HttpGet("forSelect")]
        public IActionResult ForSelect()
        {
            try
            {
                var result = _stateService.ForSelect().ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get Order Shipping Statuses");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// ??? ????????? ???????
        /// </summary>
        [HttpPost("forSelect")]
        public IActionResult ForSelect([FromBody] CompanyFilter request)
        {
            return ForSelect();
        }
    }
}