using System.Linq.Expressions;
using EarlyWarning.Models;

namespace EarlyWarning.Repositories
{
    public interface IAnimalWaterSupplyStatusRepository
    {
        Task<IEnumerable<AnimalWaterSupplyStatus>> GetAllAsync(params Expression<Func<AnimalWaterSupplyStatus, object>>[] includes);
        Task<AnimalWaterSupplyStatus?> GetByIdAsync(Guid id, params Expression<Func<AnimalWaterSupplyStatus, object>>[] includes);
        Task AddAsync(AnimalWaterSupplyStatus entity);
        void Update(AnimalWaterSupplyStatus entity);
        void Delete(AnimalWaterSupplyStatus entity);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}