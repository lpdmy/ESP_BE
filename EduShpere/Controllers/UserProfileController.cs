using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Application.Mappings;
using EduShpere.Domain.Models;

namespace EduShpere.Controllers
{
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserProfileController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [Authorize]
        [HttpPost(ApiEndpoints.UserProfile.StudentProfileUrl)]
        public async Task<IActionResult> CreateOrUpdateProfile([FromBody] CreateUpdateStudentProfileDto dto)
        {
            try
            {
                var user = await _authService.GetMe();
                dto.UserId = user.Id;
                dto.StudentNumber = user.Username; 
                var profile = await _userService.CreateOrUpdateStudentProfileAsync(dto);
                return Ok(new ResponseDto<StudentProfileDto>(profile, null));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.UpdateFailed, 400));
            }
        }
    }
}
