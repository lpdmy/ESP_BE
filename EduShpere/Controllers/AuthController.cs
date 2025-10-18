using EduShpere.Application;
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.UserDto;
using EduShpere.Infrastructure;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using EduShpere.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost(ApiEndpoints.Auth.Login)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
        {
            if (loginRequest == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Auth.InvalidCredentials, 400));

            var result = await _authService.Login(loginRequest);
            if (result == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Auth.InvalidCredentials, 400));

            return Ok(new ResponseDto<TokenModel>(result, "Đăng nhập thành công"));
        }

        [HttpGet(ApiEndpoints.Auth.GetMe)]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var user = await _authService.GetMe();
            if (user == null)
                return NotFound(new ResponseDto<string>(null, ErrorMessages.Auth.UserNotFound, 404));

            return Ok(new ResponseDto<UserDto>(user));
        }

        [HttpPost(ApiEndpoints.Auth.ImportFile)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Auth.InvalidFile, 400));

            var result = await _authService.ImportUsers(file);
            return Ok(new ResponseDto<object>(result, "Import file thành công"));
        }

        [HttpGet(ApiEndpoints.Auth.Test)]
        public async Task<IActionResult> Test()
        {
            await _emailService.SendEmailAsync("lpdmy15@gmail.com", "Hello", "Hi");
            return Ok(new ResponseDto<object>(new
            {
                Name = "EduShpere API is working fine",
                Description = "This is a test endpoint to verify that the API is operational."
            }, "API hoạt động bình thường"));
        }
        [HttpPost(ApiEndpoints.Auth.CreateUser)]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var result = await _authService.CreateUserAndGenerateOtlAsync(dto);
                return Ok(new ResponseDto<bool>(result, "Tạo người dùng thành công"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto<string>(null, e.Message, 400));
            }
        }

        [HttpPost("api/auth/create-user-dev")]
        public async Task<IActionResult> CreateUserDev([FromBody] CreateUserDto dto, [FromServices] IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                return NotFound(); // hoặc Forbidden
            }

            try
            {
                var result = await _authService.CreateUserAndReturnTokenAsync(dto);
                return Ok(new ResponseDto<TokenModel>(result, "Tạo người dùng thành công - Development Mode"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto<string>(null, e.Message, 400));
            }
        }

        [HttpGet(ApiEndpoints.User.Users)]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] UserPaginationRequestDto? paginationRequest = null,
            [FromQuery] int? status = null,
            [FromQuery] int? role = null,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                // Set default values if paginationRequest is null
                paginationRequest ??= new UserPaginationRequestDto
                {
                    PageNumber = 1,
                    PageSize = 10
                };

                // Override with query parameters if provided
                if (status.HasValue)
                    paginationRequest.Status = status;
                if (role.HasValue)
                    paginationRequest.Role = role;
                if (!string.IsNullOrEmpty(sortField))
                    paginationRequest.SortBy = sortField;
                if (!string.IsNullOrEmpty(sortDirection))
                    paginationRequest.SortDescending = sortDirection.ToLower() == "desc";

                var result = await _authService.GetAllUsersAsync(paginationRequest);

                return Ok(new ResponseDto<PaginationResponseDto<UserDto>>(result, "Lấy danh sách người dùng thành công",
                    (int)HttpStatusCode.OK
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, $"Internal Server Error: {ex.Message}", 500));
            }
        }


        [HttpPut(ApiEndpoints.User.Users)]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            try
            {
                var result = await _authService.UpdateUserAsync(dto);
                return Ok(new ResponseDto<bool>(result, "Chỉnh sửa thông tin người dùng thành công"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto<string>(null, e.Message, 400));
            }
        }


        [HttpGet(ApiEndpoints.Auth.OneTimeLogin)]
        public async Task<IActionResult> OneTimeLogin([FromQuery] string token)
        {
            try
            {
                var fullName = await _authService.OneTimeLoginAsync(token);
                return Ok(new ResponseDto<string>(fullName));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto<string>(null, e.Message, 400));
            }
        }

        [HttpPost(ApiEndpoints.Auth.ChangePasswordOtl)]
        public async Task<IActionResult> ChangePasswordOtl([FromBody] ChangePasswordRequest dto)
        {
            try
            {
                var tokenModel = await _authService.ChangePasswordWithOtlAsync(dto.Token, dto.NewPassword);
                return Ok(new ResponseDto<TokenModel>(tokenModel, "Đổi mật khẩu thành công"));
            }
            catch (UnauthorizedException)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Auth.InvalidToken, 400));
            }
            catch (NotFoundException)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Auth.UserNotFound, 404));
            }
            catch (Exception)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            }
        }

        [HttpPost(ApiEndpoints.Auth.ChangePassword)]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var result = await _authService.ChangePassword(dto);
                return Ok(new ResponseDto<bool>(result, "Đổi mật khẩu thành công"));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
            }

        }

        [HttpPost(ApiEndpoints.Auth.ForgotPassword)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                var tokenModel = await _authService.ForgotPassword(dto);
                return Ok(new ResponseDto<TokenModel>(tokenModel, "Vui lòng kiểm tra email để reset mật khẩu"));
            }
            catch (NotFoundException)
            {
                return NotFound(new ResponseDto<string>(null, ErrorMessages.Auth.UserNotFound, 404));
            }
            catch (Exception)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            }
        }

        [HttpDelete(ApiEndpoints.User.Users + "/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _authService.DeleteUserAsync(id);
                return Ok(new ResponseDto<bool>(result, "Xóa người dùng thành công"));
            }
            catch (NotFoundException)
            {
                return NotFound(new ResponseDto<string>(null, ErrorMessages.Auth.UserNotFound, 404));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto<string>(null, e.Message, 400));
            }
        }

        [HttpGet(ApiEndpoints.User.Statistics)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var statistics = await _authService.GetUserStatisticsAsync();
                return Ok(new ResponseDto<UserStatisticsDto>(statistics, "Lấy thống kê người dùng thành công"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto<string>(null, e.Message, 400));
            }
        }
    }
}
