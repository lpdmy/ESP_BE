namespace EduShpere.Application.Services
{
    public interface ICurrentUserService
    {
        int? GetCurrentUserId();
        Task<int?> GetCurrentUserIdAsync();
    }
}
