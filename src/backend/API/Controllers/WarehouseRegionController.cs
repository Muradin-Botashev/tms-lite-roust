using API.Models;
using Domain.Extensions;
using Domain.Services.WarehouseRegion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace API.Controllers
{
    [Route("api/warehouseRegion")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class WarehouseRegionController : Controller
    {
        private readonly IWarehouseRegionService _service;

        public WarehouseRegionController(IWarehouseRegionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Получение данных для выпадающего списка
        /// </summary>
        [HttpGet("forSelect")]
        public IActionResult ForSelect()
        {
            try
            {
                var result = _service.ForSelect().ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get Warehouse Regions");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение данных для выпадающего списка
        /// </summary>
        [HttpPost("forSelect")]
        public IActionResult ForSelect([FromBody] CompanyFilter request)
        {
            try
            {
                var result = _service.ForSelect(request?.CompanyId.ToGuid()).ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get Warehouse Regions");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
