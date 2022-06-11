using API.Models;
using Domain.Extensions;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class EnumController<T> :  Controller
    {
        /// <summary>
        /// Все доступные статусы
        /// </summary>
        [HttpGet("forSelect")]
        public IActionResult ForSelect()
        {
            try
            {
                var values = Domain.Extensions.Extensions.GetOrderedEnum<T>();
                var result = new List<LookUpDto>();
                foreach (var value in values)
                {
                    string name = value.FormatEnum();
                    result.Add(new LookUpDto
                    {
                        Name = name,
                        Value = name
                    });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to For select {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Все доступные статусы
        /// </summary>
        [HttpPost("forSelect")]
        public IActionResult ForSelect([FromBody] CompanyFilter request)
        {
            return ForSelect();
        }

        private string EntityName => typeof(T).Name;
    }
}