using EarlyWarning.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface ICropPestAndDeseasReportRepository
{
    Task<IEnumerable<CropPestAndDeseaseReport>> GetAllAsync(params Expression<Func<CropPestAndDeseaseReport, object>>[] includes);
    Task<CropPestAndDeseaseReport?> GetByIdAsync(Guid id, params Expression<Func<CropPestAndDeseaseReport, object>>[] includes);
    Task AddAsync(CropPestAndDeseaseReport report);
    void Update(CropPestAndDeseaseReport report);
    void Delete(CropPestAndDeseaseReport report);
    Task<bool> ExistsOverlappingReportAsync(Guid woredaId, DateTime startDate, DateTime endDate, Guid? excludeReportId = null);
    Task SaveChangesAsync();
}

