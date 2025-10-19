using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public interface IPointHistoryRepository
    {
        Task<List<PointHistory>> GetByUserIdAsync(int userId);
        Task<PointHistory> AddAsync(PointHistory entity);
    }
}
