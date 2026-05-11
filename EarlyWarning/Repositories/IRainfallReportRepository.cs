using EarlyWarning.Models;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public interface IRainfallReportRepository
    {
        Task<IEnumerable<RainfallReport>> GetAllAsync(params Expression<Func<RainfallReport, object>>[] includes);
        Task<RainfallReport?> GetByIdAsync(Guid id, params Expression<Func<RainfallReport, object>>[] includes);
        Task AddAsync(RainfallReport report);
        void Update(RainfallReport report);
        void Delete(RainfallReport report);
        Task<bool> ExistsOverlappingReportAsync(Guid woredaId, DateTime startDate, DateTime endDate, Guid? excludeReportId = null);
        Task SaveChangesAsync();
    }

}