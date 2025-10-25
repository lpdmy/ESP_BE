using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain;

namespace EduShpere.Infrastructure.Repositories
{
    public class ClubCategoryRepository : BaseRepository<ClubCategory>, IClubCategoryRepository
    {
        public ClubCategoryRepository(EduShpereDbContext context) : base(context)
        {
        }
    }
}
