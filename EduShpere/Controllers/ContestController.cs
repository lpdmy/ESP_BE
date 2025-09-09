using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContestController : ControllerBase
    {
        private readonly IContestService _service;
        public ContestController(IContestService service)
        {
            _service = service;
        }
        [HttpPost(ApiEndpoints.Contest.GetAllContests)]
        public async Task<IActionResult> GetAllContests()
        {
            var result = await _service.GetActivitiesAsync();
            return Ok(result);
        }
    }
}
