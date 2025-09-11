
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
        Task<bool> FindUserByEmail(string email);
    }
}
