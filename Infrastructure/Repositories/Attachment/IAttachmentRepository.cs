using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IAttachmentRepository
    {
        Task DeleteAttachmentByPostId(int postId);
    }
}
