using System.Linq.Expressions;
using EarlyWarning.Models;

namespace EarlyWarning.Repositories
{
    public interface IAnimalHealthStatusRepository
    {
        Task<IEnumerable<AnimalHealthStatus>> GetAllAsync(params Expression<Func<AnimalHealthStatus, object>>[] includes);
        Task<AnimalHealthStatus?> GetByIdAsync(Guid id, params Expression<Func<AnimalHealthStatus, object>>[] includes);
        Task AddAsync(AnimalHealthStatus entity);
        void Update(AnimalHealthStatus entity);
        void Delete(AnimalHealthStatus entity);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}