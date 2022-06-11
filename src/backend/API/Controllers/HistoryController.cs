using Domain.Extensions;
using Domain.Services.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    /// <summary>
    /// История
    /// </summary>
    [Route("api/history")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class HistoryController : Controller
    {
        /// <summary>
        /// Получить историю сущности
        /// </summary>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <returns></returns>
        [Route("{entityId}")]
        [HttpGet]
        public IActionResult Get(Guid entityId)
        {
            try
            {
                HistoryDto dto = _historyService.Get(entityId);

                return Json(dto);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to Get history enties {entityId}");
                return StatusCode(500);
            }
        }

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        private readonly IHistoryService _historyService;
    }
}