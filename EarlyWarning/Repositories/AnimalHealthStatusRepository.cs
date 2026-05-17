using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class AnimalHealthStatusRepository : IAnimalHealthStatusRepository
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalHealthStatusRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnimalHealthStatus>> GetAllAsync(params Expression<Func<AnimalHealthStatus, object>>[] includes)
        {
            IQueryable<AnimalHealthStatus> query = _context.AnimalHealthStatuses;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<AnimalHealthStatus?> GetByIdAsync(Guid id, params Expression<Func<AnimalHealthStatus, object>>[] includes)
        {
            IQueryable<AnimalHealthStatus> query = _context.AnimalHealthStatuses;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(AnimalHealthStatus entity) => await _context.AnimalHealthStatuses.AddAsync(entity);
        public void Update(AnimalHealthStatus entity) => _context.AnimalHealthStatuses.Update(entity);
        public void Delete(AnimalHealthStatus entity) => _context.AnimalHealthStatuses.Remove(entity);
        public async Task<bool> ExistsAsync(Guid id) => await _context.AnimalHealthStatuses.AnyAsync(r => r.Id == id);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}