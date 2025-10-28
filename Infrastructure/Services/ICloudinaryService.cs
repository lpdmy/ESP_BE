using Microsoft.AspNetCore.Http;

namespace EduShpere.Infrastructure.Services;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile file);
    Task<string?> UploadFileAsync(IFormFile file);
}
