using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class CropGrowthRepository : ICropGrowthRepository
    {
        private readonly EarlyWarningDbContext _context;

        public CropGrowthRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CropGrowth>> GetAllAsync(params Expression<Func<CropGrowth, object>>[] includes)
        {
            IQueryable<CropGrowth> query = _context.CropGrowths;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<CropGrowth?> GetByIdAsync(Guid id, params Expression<Func<CropGrowth, object>>[] includes)
        {
            IQueryable<CropGrowth> query = _context.CropGrowths;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(CropGrowth cropGrowth) => await _context.CropGrowths.AddAsync(cropGrowth);
        public void Update(CropGrowth cropGrowth) => _context.CropGrowths.Update(cropGrowth);
        public void Delete(CropGrowth cropGrowth) => _context.CropGrowths.Remove(cropGrowth);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
