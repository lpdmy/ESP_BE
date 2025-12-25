using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IClubMemberRepository
    {
        Task<IEnumerable<ClubMember>> GetAllAsync();
        Task<ClubMember?> GetByIdAsync(int id);
        Task AddAsync(ClubMember entity);
        Task AddRangeAsync(IEnumerable<ClubMember> entities);
        Task UpdateAsync(ClubMember entity);
        Task DeleteAsync(int id);
        Task<ClubMember> GetClubMemberByUserIdAndClubId(int userid, int clubid);
        Task<bool> IsInClub(int userid, int clubid);
        IQueryable<ClubMember> GetClubMemberByUser(User user);
        Task<bool> IsMentorAnyClub(int userId);
        IQueryable<ClubMember> GetQueryable();
    }
}
