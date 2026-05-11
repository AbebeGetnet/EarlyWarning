using EarlyWarning.Models;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public interface ICropGrowthRepository
    {
        Task<IEnumerable<CropGrowth>> GetAllAsync(params Expression<Func<CropGrowth, object>>[] includes);
        Task<CropGrowth?> GetByIdAsync(Guid id, params Expression<Func<CropGrowth, object>>[] includes);
        Task AddAsync(CropGrowth cropGrowth);
        void Update(CropGrowth cropGrowth);
        void Delete(CropGrowth cropGrowth);
        Task SaveChangesAsync();
    }
}
