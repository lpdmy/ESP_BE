using EduShpere.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ContentModerationService _contentModerationService;
        public TestController(ContentModerationService contentModerationService)
        {
            _contentModerationService = contentModerationService;
        }
        [HttpPost("Test")]
        public IActionResult Test(string context)
        {
            var result = _contentModerationService.Check(context);

            if (result.Decision == "block")
            {
                return BadRequest(new
                {
                    message = "🚫 Nội dung vi phạm môi trường học đường.",
                    reason = result.Reason
                });
            }

            return Ok(new
            {
                message = "✅ Nội dung sạch.",
                reason = result.Reason
            });
        }

    }
}
