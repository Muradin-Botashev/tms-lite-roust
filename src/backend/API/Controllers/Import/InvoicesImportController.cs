using Domain.Extensions;
using Domain.Services.Import;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace API.Controllers.Import
{
    [Route("api/import/invoices")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class InvoicesImportController : Controller
    {
        protected readonly IInvoicesImportService _service;

        public InvoicesImportController(IInvoicesImportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Сформировать Excel шаблон
        /// </summary>
        [HttpGet("excelTemplate"), DisableRequestSizeLimit]
        public IActionResult GenerateExcelTemplate()
        {
            var excelData = _service.GenerateExcelTemplate();
            var fileName = $"Import invoices template {DateTime.Now.FormatDateTime()}.xlsx";
            return File(excelData, "application/vnd.ms-excel", fileName);
        }

        /// <summary>
        /// Импортировать из excel
        /// </summary>
        [HttpPost("importFromExcel"), DisableRequestSizeLimit]
        public OperationDetailedResult ImportFromExcel()
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

            return _service.ImportFromExcel(file.OpenReadStream(), fileName);
        }
    }
}