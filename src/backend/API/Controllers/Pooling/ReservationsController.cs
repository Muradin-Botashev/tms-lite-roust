using Domain.Extensions;
using Domain.Services.Pooling;
using Domain.Services.Pooling.Models;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers.Pooling
{
    [Route("api/reservations")]
    [Authorize(AuthenticationSchemes = ApiExtensions.OpenApiSchemes, Policy = ApiExtensions.OpenApiPolicy)]
    public class ReservationsController : Controller
    {
        private readonly IInputReservationsService _reservationsService;

        public ReservationsController(IInputReservationsService reservationsService)
        {
            _reservationsService = reservationsService;
        }

        [HttpPut]
        public IActionResult SaveData([FromBody] ReservationDto dto)
        {
            try
            {
                _reservationsService.SaveData(dto);
                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Result?.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get Client names");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
