using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface ICollectionIteamRepository
    {
        Task<IEnumerable<CollectionItem>> GetAllAsync();
        Task<CollectionItem?> GetByIdAsync(int id);
        Task AddAsync(CollectionItem entity);
        Task AddRangeAsync(IEnumerable<CollectionItem> entities);
        Task UpdateAsync(CollectionItem entity);
        Task DeleteAsync(int id);   
    }
}
