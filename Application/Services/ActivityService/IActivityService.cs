using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetAllAsync(int pageNumber, int pageSize, string? search = null);
        Task<Activity?> GetByIdAsync(int id);
    }
}
