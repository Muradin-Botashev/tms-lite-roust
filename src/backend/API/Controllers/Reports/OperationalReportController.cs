using Domain.Extensions;
using Domain.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api/reports/operational")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class OperationalReportController : Controller
    {
        private readonly IReportService _reportService;

        public OperationalReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Получить операционный отчёт
        /// </summary>
        /// <returns></returns>
        [HttpPost("get")]
        public IActionResult GetReport([FromBody] ReportParametersDto filter)
        {
            try
            {
                var result = _reportService.GetReport(filter);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get Operational report preview");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получить операционный отчёт
        /// </summary>
        /// <returns></returns>
        [HttpPost("export")]
        public IActionResult ExportReport([FromBody]ReportParametersDto filter)
        {
            try
            {
                var memoryStream = _reportService.ExportReport(filter);
                return File(memoryStream, "application/vnd.ms-excel", $"Daily Report_{DateTime.Now.FormatDate()}.xlsx");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get Operational report Excel");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("reportConfiguration")]
        public IActionResult GetReportConfiguration()
        {
            try
            {
                var result = _reportService.GetReportConfiguration();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get Operational report configuration");
                return StatusCode(500, ex.Message);
            }
        }
    }
}