using EduShpere.Application;
using EduShpere.Infrastructure;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class UploadController : ControllerBase
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
}
