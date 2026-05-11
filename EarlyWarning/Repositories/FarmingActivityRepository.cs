using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class FarmingActivityRepository : IFarmingActivityRepository
    {
        private readonly EarlyWarningDbContext _context;

        public FarmingActivityRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FarmingActivity>> GetAllAsync(params Expression<Func<FarmingActivity, object>>[] includes)
        {
            IQueryable<FarmingActivity> query = _context.FarmingActivities;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<FarmingActivity?> GetByIdAsync(Guid id, params Expression<Func<FarmingActivity, object>>[] includes)
        {
            IQueryable<FarmingActivity> query = _context.FarmingActivities;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(FarmingActivity activity) => await _context.FarmingActivities.AddAsync(activity);
        public void Update(FarmingActivity activity) => _context.FarmingActivities.Update(activity);
        public void Delete(FarmingActivity activity) => _context.FarmingActivities.Remove(activity);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
