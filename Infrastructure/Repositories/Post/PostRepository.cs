
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Post>> getAllPostIncluding()
        {
            return await _dbSet
                .Include(p => p.Club)
                .Include(p => p.User)
                .Include(p => p.Attachments)
                .Include(p => p.PostLikes)
                .Include(p => p.PostReports)
                .Include(p => p.Comments)
                .Include(p => p.PostHashtags)
                    .ThenInclude(ph => ph.Hashtag)
                .Include(p => p.PostMentions)
                    .ThenInclude(pm => pm.MentionedUser)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> SearchAsync(string query, int limit = 10)
        {
            var searchQuery = query.ToLower().Trim();

            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.PostHashtags)
                    .ThenInclude(ph => ph.Hashtag)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments)
                .Include(p => p.Club)
                .Include(p => p.ClassGroup)
                .Where(p => !p.IsDeleted)
                .Where(p => p.PrivacyLevel != 0) // Exclude private posts from search
                .Where(p => 
                    (p.Title != null && p.Title.ToLower().Contains(searchQuery)) ||
                    (p.Body != null && p.Body.ToLower().Contains(searchQuery)) ||
                    p.PostHashtags.Any(ph => ph.Hashtag.Name.ToLower().Contains(searchQuery)) ||
                    (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(searchQuery)
                )
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Dictionary<int, (int LikesCount, int CommentsCount)>> GetPostEngagementStatsAsync(IEnumerable<int> postIds)
        {
            var stats = await _dbSet
                .Where(p => postIds.Contains(p.Id))
                .Select(p => new
                {
                    PostId = p.Id,
                    LikesCount = p.PostLikes.Count(pl => !pl.IsDeleted),
                    CommentsCount = p.Comments.Count(c => !c.IsDeleted)
                })
                .ToListAsync();

            return stats.ToDictionary(
                s => s.PostId,
                s => (s.LikesCount, s.CommentsCount)
            );
        }

        public async Task<IEnumerable<Post>> SearchWithFiltersAsync(object request)
        {
            // For now, return basic search results
            // This method will be implemented when the DTO namespace issues are resolved
            return await SearchAsync("", 10);
        }

    }
}
