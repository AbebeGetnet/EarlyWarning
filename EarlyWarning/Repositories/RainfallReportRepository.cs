using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class RainfallReportRepository : IRainfallReportRepository
    {
        private readonly EarlyWarningDbContext _context;

        public RainfallReportRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RainfallReport>> GetAllAsync(params Expression<Func<RainfallReport, object>>[] includes)
        {
            IQueryable<RainfallReport> query = _context.RainfallReports;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<RainfallReport?> GetByIdAsync(Guid id, params Expression<Func<RainfallReport, object>>[] includes)
        {
            IQueryable<RainfallReport> query = _context.RainfallReports;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(RainfallReport report) => await _context.RainfallReports.AddAsync(report);
        public void Update(RainfallReport report) => _context.RainfallReports.Update(report);
        public void Delete(RainfallReport report) => _context.RainfallReports.Remove(report);
        public async Task<bool> ExistsOverlappingReportAsync(Guid woredaId, DateTime startDate, DateTime endDate, Guid? excludeReportId = null)
        {
            var query = _context.RainfallReports
                .Where(r => r.WoredaId == woredaId)
                .Where(r => r.StartDate <= endDate && r.EndDate >= startDate); // overlap check

            if (excludeReportId.HasValue)
                query = query.Where(r => r.Id != excludeReportId.Value);

            return await query.AnyAsync();
        }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}