using Domain.Extensions;
using Domain.Services.Reports.Registry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [Route("api/reports/registry")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class RegistryReportController : Controller
    {
        private readonly IRegistryReportService _reportService;

        public RegistryReportController(IRegistryReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Выгрузка реестра
        /// </summary>
        /// <returns></returns>
        [HttpPost("export")]
        public IActionResult ExportReport([FromBody] RegistryReportParameters filter)
        {
            var memoryStream = _reportService.ExportReport(filter);
            return File(memoryStream, "application/vnd.ms-excel", $"Registry Report_{DateTime.Now.FormatDateTime()}.xlsx");
        }
    }
}