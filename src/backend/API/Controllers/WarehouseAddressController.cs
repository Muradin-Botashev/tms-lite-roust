using Domain.Extensions;
using Domain.Services.WarehouseAddress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api/warehouseAddress")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class WarehouseAddressController : Controller
    {
        /// <summary>
        /// Получение данных для выпадающего списка
        /// </summary>
        [HttpPost("forSelect")]
        public IActionResult ForSelect([FromBody] WarehouseAddressFilter request)
        {
            try
            {
                var result = _service.ForSelect(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get Warehouse addresses");
                return StatusCode(500, ex.Message);
            }
        }

        public WarehouseAddressController(IWarehouseAddressService service)
        {
            _service = service;
        }

        private readonly IWarehouseAddressService _service;
    }
}