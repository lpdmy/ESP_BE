
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IPostLikeRepository
    {
        Task<IEnumerable<PostLike>> GetAllAsync();
        Task<PostLike?> GetByIdAsync(int id);
        Task AddAsync(PostLike entity);
        Task AddRangeAsync(IEnumerable<PostLike> entities);
        Task UpdateAsync(PostLike entity);
        Task DeleteAsync(int id);
        Task<PostLike> CheckExist(int postId, int userId);
    }
}
