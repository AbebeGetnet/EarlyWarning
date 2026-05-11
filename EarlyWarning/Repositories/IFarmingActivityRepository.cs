using EarlyWarning.Models;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public interface IFarmingActivityRepository
    {
        Task<IEnumerable<FarmingActivity>> GetAllAsync(params Expression<Func<FarmingActivity, object>>[] includes);
        Task<FarmingActivity?> GetByIdAsync(Guid id, params Expression<Func<FarmingActivity, object>>[] includes);
        Task AddAsync(FarmingActivity activity);
        void Update(FarmingActivity activity);
        void Delete(FarmingActivity activity);
        Task SaveChangesAsync();
    }
}
