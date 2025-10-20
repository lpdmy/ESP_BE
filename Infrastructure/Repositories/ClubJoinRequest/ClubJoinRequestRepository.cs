using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ClubJoinRequestRepository : BaseRepository<ClubJoinRequest>, IClubJoinRequestRepository
    {
        public ClubJoinRequestRepository(EduShpereDbContext context) : base(context)
        {
        }

        public IQueryable<ClubJoinRequest> GetAllWithIncludes()
        {
           return _context.ClubJoinRequests
                .Where(r => !r.IsDeleted)
                .Include(r => r.User);
        }
        public async Task<ClubJoinRequest?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.ClubJoinRequests
                .Include(r => r.User)
                .Include(r => r.Club)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }
        public async Task<ClubJoinRequest?> GetByUserIdAndClubId(int clubid,User user)
        {
            return await _context.ClubJoinRequests
                .Include(r => r.User)
                .Include(r => r.Club)
                .FirstOrDefaultAsync(r => r.ClubId == clubid && r.UserId==user.Id && !r.IsDeleted);
        }
    }
}
