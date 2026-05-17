using System.Linq.Expressions;
using EarlyWarning.Models;

namespace EarlyWarning.Repositories
{
    public interface IPastureStatusRepository
    {
        Task<IEnumerable<PastureStatus>> GetAllAsync(params Expression<Func<PastureStatus, object>>[] includes);
        Task<PastureStatus?> GetByIdAsync(Guid id, params Expression<Func<PastureStatus, object>>[] includes);
        Task AddAsync(PastureStatus entity);
        void Update(PastureStatus entity);
        void Delete(PastureStatus entity);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}