
using EduShpere.Domain;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure
{
    public interface IUserRepository 
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(string id);
        Task<User?> GetUserByUserName(string userName);
        Task AddRangeAsync(IEnumerable<User> users);
        Task<bool> FindUserByEmail(string email);
    }
}
