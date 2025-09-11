

using System;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(EduShpereDbContext context) : base(context) { 
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Entity not found");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }


        public virtual async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<User?> GetUserByUserName(string userName)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Username == userName || p.Email.Equals(userName));
        }
        public async Task<bool> FindUserByEmail(string email)
        {
          return  await _dbSet.AnyAsync(p => p.Email == email);
        }
    }
}
