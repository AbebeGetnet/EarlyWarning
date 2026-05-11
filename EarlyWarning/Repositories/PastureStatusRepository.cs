using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class PastureStatusRepository : IPastureStatusRepository
    {
        private readonly EarlyWarningDbContext _context;

        public PastureStatusRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PastureStatus>> GetAllAsync(params Expression<Func<PastureStatus, object>>[] includes)
        {
            IQueryable<PastureStatus> query = _context.PastureStatuses;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<PastureStatus?> GetByIdAsync(Guid id, params Expression<Func<PastureStatus, object>>[] includes)
        {
            IQueryable<PastureStatus> query = _context.PastureStatuses;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(PastureStatus entity) => await _context.PastureStatuses.AddAsync(entity);
        public void Update(PastureStatus entity) => _context.PastureStatuses.Update(entity);
        public void Delete(PastureStatus entity) => _context.PastureStatuses.Remove(entity);
        public async Task<bool> ExistsAsync(Guid id) => await _context.PastureStatuses.AnyAsync(r => r.Id == id);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}