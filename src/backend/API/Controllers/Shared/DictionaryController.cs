using API.Models;
using Domain.Extensions;
using Domain.Services;
using Domain.Services.AppConfiguration;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace API.Controllers.Shared
{
    /// <summary>
    /// Словарь
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TFilter"></typeparam>
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public abstract class DictionaryController<TService, TEntity, TDto, TFilter> : Controller 
        where TService: IDictonaryService<TEntity, TDto, TFilter>
    {
        protected readonly TService _service;

        protected IAppConfigurationService _appConfigurationService;

        /// <summary>
        /// Словарь
        /// </summary>
        /// <param name="service"></param>
        public DictionaryController(TService service, IAppConfigurationService appConfigurationService)
        {
            _service = service;
            _appConfigurationService = appConfigurationService;
        }

        /// <summary>
        /// Поиск по вхождению с пагинацией
        /// </summary>
        [HttpPost("search")]
        public IActionResult Search([FromBody]FilterFormDto<TFilter> form)
        {
            try
            {
                var result = _service.Search(form);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Search {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение данных для выпадающего списка в 
        /// </summary>
        [HttpGet("forSelect")]
        public IActionResult ForSelect()
        {
            try
            {
                var result = _service.ForSelect().OrderBy(i => i.Name).ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to ForSelect {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение данных для выпадающего списка в 
        /// </summary>
        [HttpPost("forSelect")]
        public IActionResult ForSelect([FromBody]CompanyFilter request)
        {
            try
            {
                var result = _service.ForSelect(request?.CompanyId.ToGuid()).OrderBy(i => i.Name).ToList();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to ForSelect {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение данных для выпадающего списка в 
        /// </summary>
        [HttpPost("forSelect/{field}")]
        public IActionResult ForSelect(string field, [FromBody]FilterFormDto<TFilter> filter)
        {
            try
            {
                var result = _service.ForSelect(field, filter)
                                     .OrderBy(x => x.Name);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to ForSelect (field) {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Данные по id
        /// </summary>
        [HttpGet("getById/{id}")]
        public IActionResult GetById(Guid id)
        {
            try
            {
                var user = _service.Get(id);
                return Ok(user);
            }
            catch (AccessDeniedException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to GetById {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Импортировать
        /// </summary>
        [HttpPost("import/{confirmed?}")]
        public IActionResult Import([FromBody] IEnumerable<TDto> form, [FromRoute] string confirmed)
        {
            try
            {
                var isConfirmed = (confirmed ?? string.Empty).ToLower() == "confirmed";
                var result = _service.Import(form, isConfirmed);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Import {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Импортировать из excel
        /// </summary>
        [HttpPost("importFromExcel/{confirmed?}")]
        public IActionResult ImportFromExcel([FromRoute] string confirmed)
        {
            try
            {
                var isConfirmed = (confirmed ?? string.Empty).ToLower() == "confirmed";
                var file = HttpContext.Request.Form.Files.FirstOrDefault();
                using (var stream = new FileStream(Path.GetTempFileName(), FileMode.Create))
                {
                    file.CopyTo(stream);
                    var result = _service.ImportFromExcel(stream, isConfirmed);
                    return Ok(result);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Import from Excel {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Экспортировать в excel
        /// </summary>
        [HttpPost("exportToExcel"), DisableRequestSizeLimit]
        public IActionResult ExportToExcel([FromBody]FilterFormDto<TFilter> form)
        {
            try
            {
                var memoryStream = _service.ExportToExcel(form);
                return File(memoryStream, "application/vnd.ms-excel", $"Export {EntityName.Pluralize()} {DateTime.Now.FormatDateTime()}.xlsx");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Export to Excel {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Сохранить или изменить
        /// </summary>
        [HttpPost("saveOrCreate/{confirmed?}")]
        public IActionResult SaveOrCreate([FromBody] TDto form, [FromRoute] string confirmed)
        {
            try
            {
                var isConfirmed = (confirmed ?? string.Empty).ToLower() == "confirmed";
                var result = _service.SaveOrCreate(form, isConfirmed);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Save {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Удалить
        /// </summary>
        [HttpDelete("delete")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var result = _service.Delete(id);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Delete {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получить значения по умолчанию
        /// </summary>
        [HttpGet("defaults")]
        public IActionResult GetDefaults()
        {
            try
            {
                var result = _service.GetDefaults();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get defaults {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получить значения по умолчанию
        /// </summary>
        [HttpGet("formConfiguration/{id}")]
        public IActionResult GetFormConfiguration(Guid id)
        {
            try
            {
                var defaultConfig = _appConfigurationService.GetDictionaryConfiguration(_service.GetType());

                var result = _service.GetFormConfiguration(id, defaultConfig);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to Get form configuration for {EntityName}");
                return StatusCode(500, ex.Message);
            }
        }

        private string EntityName => typeof(TEntity).Name;
    }
}