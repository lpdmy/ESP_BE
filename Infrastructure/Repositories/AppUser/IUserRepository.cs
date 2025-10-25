
using EduShpere.Domain;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure
{
    public interface IUserRepository 
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<User?> GetUserByUserName(string userName);
        Task AddRangeAsync(IEnumerable<User> users);
        Task UpdateRangeAsync(IEnumerable<User> users);
        Task<bool> FindUserByEmail(string email);
        Task<User?> GetUserByEmail(string email);
        Task<bool> FindUserByUsername(string username);
        IQueryable<User> GetQueryable();
        Task<IEnumerable<User>> SearchAsync(string query, int limit = 10);
        Task<User?> GetByIdIncludeAsync(int id);
        IQueryable<User> GetAllByStaffIncluding();
        Task<User> GetByStaffIdIncluding(int staffId);
    }
}
