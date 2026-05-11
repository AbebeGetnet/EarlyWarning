using EarlyWarning.Data;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Linq.Expressions;

namespace EarlyWarning.Repositories
{
    public class CropPestAndDeseasReportRepository : ICropPestAndDeseasReportRepository
    {
        private readonly EarlyWarningDbContext _context;

        public CropPestAndDeseasReportRepository(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CropPestAndDeseaseReport>> GetAllAsync(params Expression<Func<CropPestAndDeseaseReport, object>>[] includes)
        {
            IQueryable<CropPestAndDeseaseReport> query = _context.CropPestAndDeseaseReports;

            // Fetch all diseases once
            var allDiseases = await _context.CropPestAndDesease.ToListAsync();

            // Enrich reports with disease names
            foreach (var report in query)
            {
                if (!string.IsNullOrEmpty(report.CropDiseasesJson))
                {
                    report.DeserializeCropDiseases(); // fills SelectedDiseaseIds
                    report.DiseaseNames = allDiseases
                        .Where(d => report.SelectedDiseaseIds.Contains(d.Id))
                        .Select(d => d.Name)
                        .ToList();
                }
            }
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }
        

        
        public async Task<CropPestAndDeseaseReport?> GetByIdAsync(Guid id, params Expression<Func<CropPestAndDeseaseReport, object>>[] includes)
        {
            IQueryable<CropPestAndDeseaseReport> query = _context.CropPestAndDeseaseReports;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(CropPestAndDeseaseReport report) => await _context.CropPestAndDeseaseReports.AddAsync(report);
        public void Update(CropPestAndDeseaseReport report) => _context.CropPestAndDeseaseReports.Update(report);
        public void Delete(CropPestAndDeseaseReport report) => _context.CropPestAndDeseaseReports.Remove(report);
        public async Task<bool> ExistsOverlappingReportAsync(Guid woredaId, DateTime startDate, DateTime endDate, Guid? excludeReportId = null)
        {
            var query = _context.CropPestAndDeseaseReports
                .Where(r => r.WoredaId == woredaId)
                .Where(r => r.StartDate <= endDate && r.EndDate >= startDate); // overlap check

            if (excludeReportId.HasValue)
                query = query.Where(r => r.Id != excludeReportId.Value);

            return await query.AnyAsync();
        }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}