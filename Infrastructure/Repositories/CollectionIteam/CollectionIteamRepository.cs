using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public class CollectionIteamRepository : BaseRepository<CollectionItem> , ICollectionIteamRepository
    {
        public CollectionIteamRepository(EduShpereDbContext context) : base(context)
        {
        }
    }
}
