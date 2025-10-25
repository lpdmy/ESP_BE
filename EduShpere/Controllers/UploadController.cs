using EduShpere.Application;
using EduShpere.Infrastructure;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Middlewares;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class UploadController : BaseController
    {
        private readonly CloudinaryService _cloudinaryService;

        public UploadController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost(ApiEndpoints.Upload.UploadUrl)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            var url = await _cloudinaryService.UploadImageAsync(file);
            if (url == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Upload.UploadFailed, 400));

            return Ok(new ResponseDto<string>(url, null));
        }

        [HttpPost("api/upload/file")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            var url = await _cloudinaryService.UploadFileAsync(file);
            if (url == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Upload.UploadFailed, 400));

            return Ok(new ResponseDto<string>(url, null));
        }
    }
}
