
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.UserDto;
using EduShpere.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application
{
    public interface IAuthService
    {
        Task<UserDto> GetMe();
        Task<TokenModel> Login(LoginDto dto);
        Task<object> ImportUsers(IFormFile request);
        Task<bool> CreateUserAndGenerateOtlAsync(CreateUserDto dto);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize, string? search = null);
        Task<bool> UpdateUserAsync(UpdateUserDto dto);
        Task<string> OneTimeLoginAsync(string token);
        Task<TokenModel> ChangePasswordWithOtlAsync(string token, string newPassword);
    }
}
