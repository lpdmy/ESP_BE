using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ClubCreationRequestRepository : BaseRepository<ClubCreationRequest>, IClubCreationRepository
    {
        public ClubCreationRequestRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<bool> HasRecentSendCreationRequest(int userid,DateTime oneWeak)
        {
            return _context.ClubCreationRequest.Any(r => r.RequestedByUserId == userid && r.CreatedAt >= oneWeak && !r.IsDeleted);
        }
        public async Task<ClubCreationRequest?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.ClubCreationRequest
                .Where(c => c.Id == id && !c.IsDeleted)
                .Include(c => c.RequestedByUser)
                .Include(c => c.Category)
                .FirstOrDefaultAsync();
        }
        public IQueryable<ClubCreationRequest> GetAllWithIncludes()
        {
            return _context.ClubCreationRequest
                .Include(c => c.RequestedByUser)
                .Include(c=>c.Category)
                .Where(c => !c.IsDeleted).OrderByDescending(c=>c.CreatedAt)
                .OrderByDescending(c=>c.CreatedAt);
        }
        public IQueryable<ClubCreationRequest> GetAllWithIncludesByUser(User user)
        {
            return _context.ClubCreationRequest
                .Where(c=> c.RequestedByUserId==user.Id && !c.IsDeleted);
        }

    }
}
