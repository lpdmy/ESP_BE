
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;

namespace EduShpere.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repo;
        public ActivityService(IActivityRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Activity>> GetAllAsync(int pageNumber, int pageSize, string? search =null)
        {
            var activities = await _repo.GetAllAsync();

            // Lọc theo Category
            var filteredActivities = activities
                .Where(c => c.Category == ActivityType.Event);

            // Nếu có search thì lọc thêm
            if (!string.IsNullOrEmpty(search))
            {
                filteredActivities = filteredActivities
                    .Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Áp dụng phân trang
            return filteredActivities
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

        }
        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
    }
}
