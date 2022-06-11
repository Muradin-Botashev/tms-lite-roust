using Domain.Extensions;
using Domain.Services.Autogrouping;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api/autogroupingOrders")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class AutogroupingOrdersController : Controller
    {
        private readonly IAutogroupingOrdersService _service;

        public AutogroupingOrdersController(IAutogroupingOrdersService service)
        {
            _service = service;
        }

        /// <summary>
        /// Поиск по вхождению с пагинацией
        /// </summary>
        [HttpPost("{runId}/{parentId}/search")]
        public IActionResult Search(Guid runId, string parentId, [FromBody] FilterFormDto<AutogroupingOrdersFilterDto> form)
        {
            try
            {
                var result = _service.Search(runId, parentId.ToGuid(), form);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Search Autogrouping Orders");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение данных для выпадающего списка в 
        /// </summary>
        [HttpPost("{runId}/{parentId}/forSelect/{field}")]
        public IActionResult ForSelect(Guid runId, string parentId, string field, [FromBody] FilterFormDto<AutogroupingOrdersFilterDto> filter)
        {
            try
            {
                var result = _service.ForSelect(runId, parentId.ToGuid(), field, filter);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to For Select for field of Autogrouping Orders");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("previewConfiguration")]
        public IActionResult GetPreviewConfiguration()
        {
            var result = _service.GetPreviewConfiguration();
            return Ok(result);
        }
    }
}