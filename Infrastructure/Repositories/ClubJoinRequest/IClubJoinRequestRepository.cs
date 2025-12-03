using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IClubJoinRequestRepository
    {
        Task<IEnumerable<ClubJoinRequest>> GetAllAsync();
        Task<ClubJoinRequest?> GetByIdAsync(int id);
        Task AddAsync(ClubJoinRequest entity);
        Task AddRangeAsync(IEnumerable<ClubJoinRequest> entities);
        Task UpdateAsync(ClubJoinRequest entity);
        Task DeleteAsync(int id);
        IQueryable<ClubJoinRequest> GetAllWithIncludes();
        Task<ClubJoinRequest?> GetByIdWithIncludesAsync(int id);
        Task<ClubJoinRequest?> GetByUserIdAndClubId(int clubid, User user);
        IQueryable<ClubJoinRequest> GetAllWithIncludesByUser(int userid);
        Task<bool> IsAlreadyInvite(int userId, int clubId);
        Task<ClubJoinRequest> GetMentorInviationByClubId(int clubId);
        Task CancelMentorInvitaion(int clubJoinRequestId);
    }
}