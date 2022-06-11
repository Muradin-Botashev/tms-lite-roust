using API.Controllers.Shared;
using API.Extensions;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Documents;
using Domain.Services.Orders;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers.Grid
{
    [Route("api/orders")]
    [GridPermissions(Search = RolePermissions.OrdersView, SaveOrCreate = RolePermissions.OrdersCreate)]
    public class OrdersController : GridWithDocumentsController<IOrdersService, Order, OrderDto, OrderFormDto, OrderSummaryDto, OrderFilterDto> 
    {
        public OrdersController(IOrdersService ordersService, IDocumentService documentService) : base(ordersService, documentService)
        {
        }

        /// <summary>
        /// Поиск по номеру
        /// </summary>
        [HttpPost("findNumber")]
        public IActionResult Search([FromBody]NumberSearchFormDto form)
        {
            try
            {
                var result = service.FindByNumber(form);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Find Order number");
                return StatusCode(500, ex.Message);
            }
        }
    }
}