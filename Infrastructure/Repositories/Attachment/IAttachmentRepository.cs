using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IAttachmentRepository : IRepository<Attachment>
    {
        Task DeleteAttachmentByPostId(int postId);
        Task<IEnumerable<Attachment>> GetAttachmentsByPostIdAsync(int postId);
        Task DeleteRangeAsync(IEnumerable<Attachment> attachments);
        Task DeleteAttachmentBySubmissionId(int submissionId);
        Task<IEnumerable<Attachment>> GetAttachmentsBySubmissionIdAsync(int submissionId);
    }
}
