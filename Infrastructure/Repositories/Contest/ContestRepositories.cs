


using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public class ContestRepositories : BaseRepository<Activity>,IContestRepositories
    {
        public ContestRepositories(EduShpereDbContext context) : base(context)
        {
        }
    }
}
