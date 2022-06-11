using API.Controllers.Shared;
using API.Extensions;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Documents;
using Domain.Services.Shippings;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers.Grid
{
    [Route("api/shippings")]
    [GridPermissions(Search = RolePermissions.ShippingsView, SaveOrCreate = RolePermissions.ShippingsView)]
    public class ShippingsController : GridWithDocumentsController<IShippingsService, Shipping, ShippingDto, ShippingFormDto, ShippingSummaryDto, ShippingFilterDto> 
    {
        public ShippingsController(IShippingsService shippingsService, IDocumentService documentService) : base(shippingsService, documentService)
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
                Log.Error(ex, $"Failed to Find Shipping number");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Конфигурация полей формы создания новой перевозки вместе с накладными
        /// </summary>
        [HttpGet("create/appConfiguration")]
        public IActionResult GetCreateFormConfiguration()
        {
            var result = service.GetCreateFormConfiguration();
            return Ok(result);
        }

        /// <summary>
        /// Создание новой перевозки вместе с накладными
        /// </summary>
        [HttpGet("create/defaults")]
        public IActionResult DefaultCreateForm()
        {
            try
            {
                var result = service.DefaultCreateForm();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get default values for Create Shipping");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Создание новой перевозки вместе с накладными
        /// </summary>
        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateShippingDto dto)
        {
            try
            {
                var result = service.Create(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Create Shipping");
                return StatusCode(500, ex.Message);
            }
        }
    }
}