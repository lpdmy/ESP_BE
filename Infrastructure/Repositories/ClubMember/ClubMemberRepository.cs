using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ClubMemberRepository :  BaseRepository<ClubMember>,IClubMemberRepository
    {
        public ClubMemberRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<ClubMember> GetClubMemberByUserIdAndClubId(int userid,int clubid)
        {
            return await _context.ClubMembers
                .Where(cm => cm.UserId == userid && cm.ClubId == clubid && !cm.IsDeleted).Include(p => p.Club).ThenInclude(p => p.Category).Include(p=>p.User)
                .FirstOrDefaultAsync();
        } 
        public async Task<bool> IsInClub(int userid,int clubid)
        {
            return await _context.ClubMembers.AnyAsync(cm => cm.UserId == userid && cm.ClubId == clubid && !cm.IsDeleted);
        }
        public IQueryable<ClubMember> GetClubMemberByUser(User user)
        {
            return _context.ClubMembers.Where(p => p.UserId == user.Id && p.IsDeleted==false).Include(p=>p.Club).ThenInclude(p=>p.Category);
        }
        public async Task<bool> IsMentorAnyClub(int userId)
        {
            return await _context.ClubMembers.AnyAsync(cm=> cm.UserId == userId && cm.Role == "Mentor" && !cm.IsDeleted);
        }
        
    }
}
