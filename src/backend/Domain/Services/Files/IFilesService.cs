using Domain.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Domain.Services.Files
{
    public interface IFilesService : IService
    {
        Task<ValidateResult> UploadAsync(string fileName, string body);
        Task<ValidateResult> UploadAsync(IFormFile formFile);
        FileDto Get(Guid id);
    }
}
