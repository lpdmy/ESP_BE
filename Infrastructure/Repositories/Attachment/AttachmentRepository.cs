using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public class AttachmentRepository : BaseRepository<Attachment>,IAttachmentRepository
    {
        public AttachmentRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task DeleteAttachmentByPostId(int postId)
        {
            var attachments = _context.Attachments.Where(a => a.PostId == postId);
            _context.Attachments.RemoveRange(attachments);
            await _context.SaveChangesAsync();
        }
    }
}
