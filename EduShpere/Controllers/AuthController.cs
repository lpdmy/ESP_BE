using EduShpere.Application;
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.UserDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
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
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Auth.InvalidFile, 400));

            var result = await _authService.ImportUsers(file);
            return Ok(result);
        }

        [HttpGet(ApiEndpoints.Auth.Test)]
        public async Task<IActionResult> Test()
        {
            await _emailService.SendEmailAsync("lpdmy15@gmail.com", "Hello", "Hi");
            return Ok(new
            {
                Name = "EduShpere API is working fine",
                Description = "This is a test endpoint to verify that the API is operational."
            });
        }
        [HttpGet(ApiEndpoints.Auth.TestInvitation)]
        public async Task<IActionResult> TestInvitation(string userFullName, string userEmail)
        {
            await _emailService.SendEmailInvitaion(userFullName, userEmail);
            return Ok(new
            {
                Name = "EduShpere API is working fine",
                Description = "This is a test endpoint to verify that the API is operational."
            });
        }


        [HttpPost(ApiEndpoints.Auth.CreateUser)]
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
    }
}
