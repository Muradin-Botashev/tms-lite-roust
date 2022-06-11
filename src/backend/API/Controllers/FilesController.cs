using API.Models;
using Domain.Extensions;
using Domain.Services.Files;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Serilog;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// Файлы
    /// </summary>
    [Route("api/files")]
    [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
    public class FilesController : Controller
    {
        private readonly IFilesService filesService;

        public FilesController(IFilesService filesService)
        {
            this.filesService = filesService;
        }

        /// <summary>
        /// Загрузка файла
        /// </summary>
        /// <param name="formFile">Файл</param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile formFile)
        {
            try
            {
                ValidateResult result = await filesService.UploadAsync(formFile);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to upload file");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Загрузка файла в base64 кодировке
        /// </summary>
        /// <param name="dto">Файл</param>
        /// <returns></returns>
        [Route("base64")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromBody]FileBase64Dto dto)
        {
            try
            {
                ValidateResult result = await filesService.UploadAsync(dto.Name, dto.Body);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to upload file");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Получить файл на просмотр
        /// </summary>
        /// <param name="id">Идентификатор файла</param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public IActionResult Get(Guid id)
        {
            try
            {
                FileDto dto = filesService.Get(id);

                return File(dto.Data, MimeTypes.GetMimeType(dto.Name));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to Get file {id}");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Скачать файл
        /// </summary>
        /// <param name="id">Идентификатор файла</param>
        /// <returns></returns>
        [Route("{id}/[action]")]
        [HttpGet]
        public IActionResult Download(Guid id)
        {
            try
            {
                FileDto dto = filesService.Get(id);

                return File(dto.Data, "application/octet-stream", dto.Name);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to Get file {id}");
                return StatusCode(500);
            }
        }
    }
}