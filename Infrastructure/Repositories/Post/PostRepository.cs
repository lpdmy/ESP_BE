
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(EduShpereDbContext context) : base(context)
        {
        }
        public IQueryable<Post> GetAllPostIncluding()
        {
            return _dbSet
                .Include(p => p.Club)
                .Include(p => p.User)
                .Include(p => p.Attachments)
                .Include(p => p.PostLikes)
                .Include(p => p.PostReports)
                .Include(p => p.Comments)
                .Include(p => p.PostHashtags).ThenInclude(ph => ph.Hashtag)
                .Include(p => p.PostMentions).ThenInclude(pm => pm.MentionedUser);
        }public IQueryable<Post> GetAllPostIncludingByClubId(int clubid)
        {
            return _dbSet.Where(p=>p.ClubId==clubid)
                .Include(p => p.Club)
                .Include(p => p.User)
                .Include(p => p.Attachments)
                .Include(p => p.PostLikes)
                .Include(p => p.PostReports)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .Include(p => p.PostHashtags).ThenInclude(ph => ph.Hashtag)
                .Include(p => p.PostMentions).ThenInclude(pm => pm.MentionedUser);
        }
        public async Task DeleteSoft(int id)
        {
            var posts = _dbSet.Where(p => p.Id == id).FirstOrDefault();
            posts.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Post>> SearchAsync(string query, int limit = 10, bool includeClubMembers = false)
        {
            var searchQuery = query.ToLower().Trim();

            var queryable = _dbSet
                .Include(p => p.User)
                .Include(p => p.PostHashtags)
                    .ThenInclude(ph => ph.Hashtag)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments)
                .Include(p => p.Club)
                .Include(p => p.ClassGroup)
                .Where(p => !p.IsDeleted)
                .Where(p => 
                    (p.Title != null && p.Title.ToLower().Contains(searchQuery)) ||
                    (p.Body != null && p.Body.ToLower().Contains(searchQuery)) ||
                    p.PostHashtags.Any(ph => ph.Hashtag.Name.ToLower().Contains(searchQuery)) ||
                    (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(searchQuery)
                )
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit * 2); // fetch extra for permission filtering

            if (includeClubMembers)
            {
                queryable = queryable.Include(p => p.Club).ThenInclude(c => c.ClubMembers);
            }

            return await queryable.ToListAsync();
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

        public async Task<IEnumerable<Post>> SearchWithFiltersAsync(object request, bool includeClubMembers = false)
        {
            // For now, return basic search results
            // This method will be implemented when the DTO namespace issues are resolved
            return await SearchAsync("", 10, includeClubMembers);
        }

        public IQueryable<Post> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

    }
}
