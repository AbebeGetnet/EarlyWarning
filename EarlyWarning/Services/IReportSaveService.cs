using EarlyWarning.Data;
using EarlyWarning.Models;
using EarlyWarning.Models.Wizard;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Services
{
    public interface IReportSaveService
    {
        Task SaveAllReportsAsync(CombinedReportWizardModel model, string userId);
    }

    public class ReportSaveService : IReportSaveService
    {
        private readonly EarlyWarningDbContext _context;

        public ReportSaveService(EarlyWarningDbContext context)
        {
            _context = context;
        }

        public async Task SaveAllReportsAsync(CombinedReportWizardModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: RainfallReport
                model.Rainfall.WoredaId = model.WoredaId;
                model.Rainfall.SerializeKebeles();
                model.Rainfall.SerializeDroughtKebeles();
                await _context.RainfallReports.AddAsync(model.Rainfall);

                // Step 2: FarmingActivity
                model.Farming.WoredaId = model.WoredaId;
                await _context.FarmingActivities.AddAsync(model.Farming);

                // Step 3: CropGrowth
                model.Growth.WoredaId = model.WoredaId;
                await _context.CropGrowths.AddAsync(model.Growth);

                // Step 4: CropPestAndDiseaseReport
                model.PestDisease.WoredaId = model.WoredaId;
                model.PestDisease.SerializeCropDiseases();   // serialize SelectedDiseaseIds
                await _context.CropPestAndDeseaseReports.AddAsync(model.PestDisease);

                // Step 5: PastureStatus
                model.Pasture.WoredaId = model.WoredaId;
                await _context.PastureStatuses.AddAsync(model.Pasture);

                // Step 6: AnimalWaterSupplyStatus
                model.WaterSupply.WoredaId = model.WoredaId;
                await _context.AnimalWaterSupplyStatuses.AddAsync(model.WaterSupply);

                // Step 7: AnimalHealthStatus
                model.AnimalHealth.WoredaId = model.WoredaId;
                model.AnimalHealth.SerializeCropDiseases();  // animal diseases JSON
                await _context.AnimalHealthStatuses.AddAsync(model.AnimalHealth);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}