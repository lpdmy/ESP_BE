using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<Attachment>> GetAttachmentsByPostIdAsync(int postId)
        {
            return await _dbSet.Where(a => a.PostId == postId && !a.IsDeleted).ToListAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<Attachment> attachments)
        {
            _dbSet.RemoveRange(attachments);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAttachmentBySubmissionId(int submissionId)
        {
            var attachments = _context.Attachments.Where(a => a.SubmissionId == submissionId);
            _context.Attachments.RemoveRange(attachments);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Attachment>> GetAttachmentsBySubmissionIdAsync(int submissionId)
        {
            return await _dbSet.Where(a => a.SubmissionId == submissionId && !a.IsDeleted).ToListAsync();
        }
    }
}
