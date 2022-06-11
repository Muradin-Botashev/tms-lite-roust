using Domain.Extensions;
using Domain.Services.Autogrouping;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace API.Controllers
{
    [Route("api/autogrouping")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class AutogroupingController : Controller
    {
        private readonly IAutogroupingService _service;

        public AutogroupingController(IAutogroupingService service)
        {
            _service = service;
        }
        
        /// <summary>
        /// Запуск автогруппировки
        /// </summary>
        [HttpPost("run")]
        public IActionResult Run([FromBody]RunRequest request)
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
                Log.Error(ex, "Failed to run Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Изменение ТК в результате авторуппировки
        /// </summary>
        [HttpPost("{runId}/changeCarrier")]
        public IActionResult ChangeCarrier(Guid runId, [FromBody] ChangeCarrierRequest request)
        {
            try
            {
                _service.ChangeCarrier(runId, request);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to change carrier in Autogrouping result");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Перенос накладных в другую перевозку в результате авторуппировки
        /// </summary>
        [HttpPost("{runId}/moveOrders")]
        public IActionResult MoveOrders(Guid runId, [FromBody] MoveOrderRequest request)
        {
            try
            {
                var result = _service.MoveOrders(runId, request);
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
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to move orders in Autogrouping result");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Применение авторуппировки
        /// </summary>
        [HttpPost("{runId}/apply"), DisableRequestSizeLimit]
        public IActionResult Apply(Guid runId, [FromBody]ApplyRequest request)
        {
            try
            {
                var result = _service.Apply(runId, request?.RowIds?.Select(Guid.Parse).ToList());
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
                Log.Error(ex, "Failed to apply Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Применение авторуппировки и отправка в ТК
        /// </summary>
        [HttpPost("{runId}/applyAndSend"), DisableRequestSizeLimit]
        public IActionResult ApplyAndSend(Guid runId, [FromBody] ApplyRequest request)
        {
            try
            {
                var result = _service.ApplyAndSend(runId, request?.RowIds?.Select(Guid.Parse).ToList());
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
                Log.Error(ex, "Failed to apply Autogrouping and send");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Поиск по вхождению с пагинацией
        /// </summary>
        [HttpPost("{runId}/search")]
        public IActionResult Search(Guid runId, [FromBody]FilterFormDto<AutogroupingFilterDto> form)
        {
            try
            {
                var result = _service.Search(runId, form);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Search Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение Id сущностей, подходящих под фильтр
        /// </summary>
        [HttpPost("{runId}/ids")]
        public IActionResult SearchIds(Guid runId, [FromBody]FilterFormDto<AutogroupingFilterDto> form)
        {
            try
            {
                var result = _service.SearchIds(runId, form);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Load IDs of Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение данных для выпадающего списка в 
        /// </summary>
        [HttpPost("{runId}/forSelect/{field}")]
        public IActionResult ForSelect(Guid runId, string field, [FromBody] FilterFormDto<AutogroupingFilterDto> filter)
        {
            try
            {
                var result = _service.ForSelect(runId, field, filter);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to For Select for field of Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Экспортировать в excel
        /// </summary>
        [HttpPost("{runId}/exportToExcel"), DisableRequestSizeLimit]
        public IActionResult ExportToExcel(Guid runId, [FromBody]ExportExcelFormDto<AutogroupingFilterDto> dto)
        {
            try
            {
                var memoryStream = _service.ExportToExcel(runId, dto);
                return File(memoryStream, "application/vnd.ms-excel", $"Export Autogrouping {DateTime.Now.FormatDateTime()}.xlsx");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Export autogrouping to Excel");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение сводной информации по выделенным записям
        /// </summary>
        [HttpGet("{runId}/getSummary")]
        public IActionResult GetSummary(Guid runId)
        {
            try
            {
                var result = _service.GetSummary(runId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get summary for Autogrouping");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("previewConfiguration")]
        public IActionResult GetPreviewConfiguration()
        {
            var result = _service.GetPreviewConfiguration();
            return Ok(result);
        }

        [HttpGet("autogroupingTypes")]
        public IActionResult GetAutogroupingTypes()
        {
            var result = _service.GetAutogroupingTypes();
            return Ok(result);
        }
    }
}