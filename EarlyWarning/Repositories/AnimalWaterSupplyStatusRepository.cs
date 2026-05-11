using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class AnimalWaterSupplyStatusRepository : IAnimalWaterSupplyStatusRepository
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalWaterSupplyStatusRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnimalWaterSupplyStatus>> GetAllAsync(params Expression<Func<AnimalWaterSupplyStatus, object>>[] includes)
        {
            IQueryable<AnimalWaterSupplyStatus> query = _context.AnimalWaterSupplyStatuses;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<AnimalWaterSupplyStatus?> GetByIdAsync(Guid id, params Expression<Func<AnimalWaterSupplyStatus, object>>[] includes)
        {
            IQueryable<AnimalWaterSupplyStatus> query = _context.AnimalWaterSupplyStatuses;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(AnimalWaterSupplyStatus entity) => await _context.AnimalWaterSupplyStatuses.AddAsync(entity);
        public void Update(AnimalWaterSupplyStatus entity) => _context.AnimalWaterSupplyStatuses.Update(entity);
        public void Delete(AnimalWaterSupplyStatus entity) => _context.AnimalWaterSupplyStatuses.Remove(entity);
        public async Task<bool> ExistsAsync(Guid id) => await _context.AnimalWaterSupplyStatuses.AnyAsync(r => r.Id == id);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}