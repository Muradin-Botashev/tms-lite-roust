using Domain.Extensions;
using Domain.Services.Warehouses.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace API.Controllers.Import
{
    [Route("api/import/poolingWarehouses")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class PoolingWarehousesImportController : Controller
    {
        protected readonly IPoolingWarehousesImportService _service;

        public PoolingWarehousesImportController(IPoolingWarehousesImportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Сформировать Excel шаблон
        /// </summary>
        [HttpGet("excelTemplate"), DisableRequestSizeLimit]
        public IActionResult GenerateExcelTemplate()
        {
            try
            {
                var memoryStream = _service.GenerateExcelTemplate();
                var fileName = $"Import Pooling Warehouses template {DateTime.Now.FormatDateTime()}.xlsx";
                return File(memoryStream, "application/vnd.ms-excel", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get Excel template for Pooling warehouses import");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Импортировать из excel
        /// </summary>
        [HttpPost("importFromExcel"), DisableRequestSizeLimit]
        public IActionResult ImportFromExcel()
        {
            try
            {
                var file = HttpContext.Request.Form.Files.ElementAt(0);

                string fileName = null;
                if (HttpContext.Request.Form.ContainsKey("FileName"))
                {
                    fileName = HttpContext.Request.Form["FileName"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = file.FileName;
                }

                var result = _service.ImportFromExcel(file.OpenReadStream(), fileName);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Import Pooling warehouses from Excel");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
