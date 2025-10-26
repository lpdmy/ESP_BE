using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class UserRightRepository : BaseRepository<UserRight>, IUserRightRepository
    {
        public UserRightRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task DeleteByUserIdAsync(int userId)
        {
            var userRights = _context.UserRights.Where(ur => ur.UserId == userId);
            _context.UserRights.RemoveRange(userRights);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> HasRight(int userId)
        {
            return await _context.UserRights.AnyAsync(ur => ur.UserId == userId && !ur.IsDeleted);
        }
    }
}
