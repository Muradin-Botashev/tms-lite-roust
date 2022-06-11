using Domain.Extensions;
using Domain.Services.Autogrouping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api/open/autogrouping")]
    [Authorize(AuthenticationSchemes = ApiExtensions.OpenApiSchemes, Policy = ApiExtensions.OpenApiPolicy)]
    public class AutogroupingOpenController : Controller
    {
        private readonly IAutogroupingOpenService _service;

        public AutogroupingOpenController(IAutogroupingOpenService service)
        {
            _service = service;
        }

        /// <summary>
        /// Запуск автогруппировки
        /// </summary>
        [HttpPost("run")]
        public IActionResult Run([FromBody] OpenRunRequest request)
        {
            try
            {
                var result = _service.RunGrouping(request);
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
                Log.Error(ex, "Failed to run open Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
