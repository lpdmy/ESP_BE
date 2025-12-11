using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IPostHashTagRepository
    {
        Task DeleteByPostId(int id);
    }
}
