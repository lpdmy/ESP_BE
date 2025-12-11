using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure
{
    public interface IPostRepository 
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task AddAsync(Post entity);
        Task AddRangeAsync(IEnumerable<Post> entities);
        Task UpdateAsync(Post entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Post>> SearchAsync(string query, int limit = 10, bool includeClubMembers = false);
        Task<IEnumerable<Post>> SearchWithFiltersAsync(object request, bool includeClubMembers = false);
        Task<Dictionary<int, (int LikesCount, int CommentsCount)>> GetPostEngagementStatsAsync(IEnumerable<int> postIds);
        IQueryable<Post> GetAllPostIncluding();
        Task DeleteSoft(int id);
        IQueryable<Post> GetAllPostIncludingByClubId(int clubid);
        IQueryable<Post> GetQueryable();
    }
}
