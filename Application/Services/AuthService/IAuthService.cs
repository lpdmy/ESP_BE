
using EduShpere.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application
{
    public interface IAuthService
    {
        Task<User> GetMe();
        Task<TokenModel> Login(LoginDto dto);
        Task<object> ImportUsers(IFormFile request);
    }
}
