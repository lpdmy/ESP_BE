using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;

namespace EduShpere.Application.Services
{
    public class ContestService : IContestService
    {
        private readonly IContestRepositories _repo;
        public ContestService(IContestRepositories repo)
        {
            _repo = repo;
        }
        
        public async Task<IEnumerable<Activity>> GetActivitiesAsync()
        {
            return await _repo.GetAllAsync();
        }
    }
}
