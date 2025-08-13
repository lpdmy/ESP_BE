using EduShpere.Application;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Shared;
using Microsoft.AspNetCore.Http;


namespace EduShpere.Application;
public class HttpContextService : IHttpContextService
{
    private User? _appUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public HttpContextService(IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<User?> GetAppUser()
    {
        if (_appUser != null)
            return _appUser;

        var userId = GetUserId();

        if (userId != null)
        {
            _appUser = await _userService.GetUserByIdAsync(userId.ToString());
        }

        return _appUser;
    }

    public async Task<User> GetAppUserAndThrow()
    {
        return await GetAppUser() ?? throw new UnauthorizedException();
    }
    public string GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return string.Empty;
        }

        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        }
        return ipAddress ?? string.Empty;
    }
    private string? GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

        return userId is not null
            ? new string(userId)
            : null;
    }
}
