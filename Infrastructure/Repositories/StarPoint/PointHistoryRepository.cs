using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public class PointHistoryRepository : IPointHistoryRepository
    {
        private readonly EduShpereDbContext _db;
        public PointHistoryRepository(EduShpereDbContext db) => _db = db;

        public async Task<List<PointHistory>> GetByUserIdAsync(int userId) =>
            await _db.PointHistory
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<PointHistory> AddAsync(PointHistory entity)
        {
            _db.PointHistory.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
