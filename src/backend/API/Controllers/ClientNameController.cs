using API.Models;
using Domain.Extensions;
using Domain.Services.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace API.Controllers
{
    [Route("api/clientName")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class ClientNameController : Controller
    {
        /// <summary>
        /// Получение данных для выпадающего списка
        /// </summary>
        [HttpGet("forSelect")]
        public IActionResult ForSelect()
        {
            try
            {
                var result = _service.ForSelect().OrderBy(i => i.Name).ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get Client names");
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
                var result = _service.ForSelect(request?.CompanyId.ToGuid()).OrderBy(i => i.Name).ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get Client names");
                return StatusCode(500, ex.Message);
            }
        }

        public ClientNameController(IClientNameService service)
        {
            _service = service;
        }

        private readonly IClientNameService _service;
    }
}
