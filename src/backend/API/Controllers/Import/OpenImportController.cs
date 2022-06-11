using Domain.Extensions;
using Domain.Services.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.IO;

namespace API.Controllers.Import
{
    [Route("api/open/import")]
    [Authorize(AuthenticationSchemes = ApiExtensions.OpenApiSchemes, Policy = ApiExtensions.OpenApiPolicy)]
    public class OpenImportController : Controller
    {
        private readonly IOpenImportService _importShippingsService;

        public OpenImportController(IOpenImportService importShippingsService)
        {
            _importShippingsService = importShippingsService;
        }

        [HttpPost("shippings")]
        public IActionResult ImportShippings()
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var requestData = reader.ReadToEnd();
                    _importShippingsService.ImportShippings(requestData);
                    return Ok();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Import shippings");
                return StatusCode(500, ex.Message);
            }
        }
    }
}