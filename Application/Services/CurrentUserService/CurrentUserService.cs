using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "Id")?.Value;

            if (int.TryParse(userId, out var id))
                return id;
            return null;
        }

        public async Task<int?> GetCurrentUserIdAsync()
        {
            return await Task.FromResult(GetCurrentUserId());
        }
    }
}
