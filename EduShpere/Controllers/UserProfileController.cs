using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Application.Mappings;
using EduShpere.Domain.Models;
using EduShpere.Middlewares;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class UserProfileController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserProfileController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }


        /// <summary>
        /// Cập nhật thông tin cá nhân của user hiện tại (số điện thoại, ngày sinh, avatar, bio)
        /// </summary>
        [Authorize]
        [HttpPut(ApiEndpoints.UserProfile.MyProfile)]
        public async Task<IActionResult> UpdateMyPersonalInfo([FromBody] UpdatePersonalInfoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return GetValidationErrorResponse();
                }

                var user = await _authService.GetMe();
                var result = await _userService.UpdatePersonalInfoAsync(user.Id, dto);
                
                if (result)
                {
                    return Ok(new ResponseDto<string>(null, "Cập nhật thông tin cá nhân thành công"));
                }
                else
                {
                    return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.UpdateFailed, 400));
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.UpdateFailed, 400));
            }
        }

        /// <summary>
        /// Lấy profile của user hiện tại
        /// </summary>
        [Authorize]
        [HttpGet(ApiEndpoints.UserProfile.MyProfile)]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var user = await _authService.GetMe();
                var profile = await _userService.GetStudentProfileByUserIdAsync(user.Id);
                
                if (profile == null)
                {
                    return NotFound(new ResponseDto<string>(null, ErrorMessages.UserProfile.ProfileNotFound, 404));
                }

                return Ok(new ResponseDto<GetStudentProfileDto>(profile, "Lấy thông tin profile thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.GetFailed, 400));
            }
        }

        /// <summary>
        /// Lấy tất cả student profiles (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet(ApiEndpoints.UserProfile.AllProfiles)]
        public async Task<IActionResult> GetAllStudentProfiles()
        {
            try
            {
                var profiles = await _userService.GetAllStudentProfilesAsync();
                return Ok(new ResponseDto<IEnumerable<GetStudentProfileDto>>(profiles, "Lấy danh sách profiles thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.GetFailed, 400));
            }
        }

        /// <summary>
        /// Lấy profile theo ID (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet(ApiEndpoints.UserProfile.ProfileById)]
        public async Task<IActionResult> GetStudentProfileById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.InvalidId, 400));
                }

                var profile = await _userService.GetStudentProfileByIdAsync(id);
                
                if (profile == null)
                {
                    return NotFound(new ResponseDto<string>(null, ErrorMessages.UserProfile.ProfileNotFound, 404));
                }

                return Ok(new ResponseDto<GetStudentProfileDto>(profile, "Lấy thông tin profile thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.GetFailed, 400));
            }
        }

        /// <summary>
        /// Tạo profile mới (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost(ApiEndpoints.UserProfile.CreateProfile)]
        public async Task<IActionResult> CreateStudentProfile([FromBody] CreateUpdateStudentProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return GetValidationErrorResponse();
                }

                var profile = await _userService.CreateStudentProfileAsync(dto);
                return CreatedAtAction(nameof(GetStudentProfileById), new { id = profile.Id }, 
                    new ResponseDto<StudentProfileDto>(profile, "Tạo profile thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.CreateFailed, 400));
            }
        }

        /// <summary>
        /// Cập nhật thông tin sinh viên theo ID (Admin only) - bao gồm mã số sinh viên, năm nhập học
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut(ApiEndpoints.UserProfile.UpdateProfile)]
        public async Task<IActionResult> UpdateStudentProfile(int id, [FromBody] UpdateStudentInfoDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.InvalidId, 400));
                }

                if (!ModelState.IsValid)
                {
                    return GetValidationErrorResponse();
                }

                var profile = await _userService.UpdateStudentInfoAsync(id, dto);
                return Ok(new ResponseDto<StudentProfileDto>(profile, "Cập nhật thông tin sinh viên thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDto<string>(null, ex.Message, 404));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.UpdateFailed, 400));
            }
        }

        /// <summary>
        /// Xóa profile theo ID (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete(ApiEndpoints.UserProfile.DeleteProfile)]
        public async Task<IActionResult> DeleteStudentProfile(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.InvalidId, 400));
                }

                var result = await _userService.DeleteStudentProfileAsync(id);
                
                if (!result)
                {
                    return NotFound(new ResponseDto<string>(null, ErrorMessages.UserProfile.ProfileNotFound, 404));
                }

                return Ok(new ResponseDto<string>(null, "Xóa profile thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.DeleteFailed, 400));
            }
        }

        /// <summary>
        /// Kiểm tra profile có tồn tại không
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet(ApiEndpoints.UserProfile.CheckProfileExists)]
        public async Task<IActionResult> CheckProfileExists(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.InvalidId, 400));
                }

                var exists = await _userService.StudentProfileExistsAsync(id);
                return Ok(new ResponseDto<bool>(exists, exists ? ErrorMessages.UserProfile.ProfileExists : ErrorMessages.UserProfile.ProfileNotExists));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.UserProfile.GetFailed, 400));
            }
        }
    }
}
