using EduShpere.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequest("Invalid login request.");
            }
            var result = await _authService.Login(loginRequest);
            if (result == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok(result);
        }
        [HttpGet("GetMe")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var user = await _authService.GetMe();
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }
        [HttpPost("ImportFile")]
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
           
            return Ok(await _authService.ImportUsers(file));
        }
        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {

            return Ok(new
            {
                Name = "EduShpere API is working fine",
                Description = "This is a test endpoint to verify that the API is operational."
            });
        }
    }
}
