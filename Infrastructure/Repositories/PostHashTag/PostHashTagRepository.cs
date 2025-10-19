using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public class PostHashTagRepository : BaseRepository<PostHashtag>, IPostHashTagRepository
    {
        public PostHashTagRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task DeleteByPostId(int id)
        {
            var postHashTags = _context.PostHashtags.Where(ph => ph.PostId == id);
            _context.PostHashtags.RemoveRange(postHashTags);
            await _context.SaveChangesAsync();
        }
    }
}
