using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.OneTimeLogin
{
    public class OneTimeLoginRepository : IOneTimeLoginRepository
    {
        private readonly EduShpereDbContext _context;
        public OneTimeLoginRepository(EduShpereDbContext context) => _context = context;

        public async Task<OneTimeLoginToken> CreateTokenAsync(User user)
        {
            try
            {
                var token = new OneTimeLoginToken
                {
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString("N"),
                    Expiry = DateTime.UtcNow.AddHours(24)
                };
                _context.Set<OneTimeLoginToken>().Add(token);
                await _context.SaveChangesAsync();
                return token;
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public async Task<OneTimeLoginToken?> GetValidTokenAsync(string token)
        {
            return await _context.Set<OneTimeLoginToken>()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.Expiry > DateTime.UtcNow);
        }

        public async Task MarkAsUsedAsync(OneTimeLoginToken token)
        {
            token.IsUsed = true;
            await _context.SaveChangesAsync();
        }
    }

}
