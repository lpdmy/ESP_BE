using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.OneTimeLogin
{
    public interface IOneTimeLoginRepository
    {
        Task<OneTimeLoginToken> CreateTokenAsync(User user);
        Task<OneTimeLoginToken?> GetValidTokenAsync(string token);
        Task MarkAsUsedAsync(OneTimeLoginToken token);
    }
}
