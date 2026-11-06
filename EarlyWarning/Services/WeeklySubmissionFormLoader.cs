using EarlyWarning.Data;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Services;

public interface IWeeklySubmissionFormLoader
{
    Task<WeeklySubmissionFormViewModel> LoadAsync(WeeklyReportSubmission submission);
}

/// <summary>Loads every domain record the wizard saved for a submission's woreda and reporting week.</summary>
public class WeeklySubmissionFormLoader(EarlyWarningDbContext db) : IWeeklySubmissionFormLoader
{
    public async Task<WeeklySubmissionFormViewModel> LoadAsync(WeeklyReportSubmission submission)
    {
        var woredaId = submission.WoredaId;
        var start = submission.StartDate.Date;
        var end = submission.EndDate.Date;
        var form = new WeeklySubmissionFormViewModel();

        form.Rainfall = await db.RainfallReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        if (form.Rainfall != null)
        {
            form.Rainfall.DeserializeKebeles();
            form.Rainfall.DeserializeDroughtKebeles();
            form.HighRainfallKebeles = await KebeleNames(form.Rainfall.SelectedKebeleIds);
            form.DroughtKebeles = await KebeleNames(form.Rainfall.SelectedDroughtKebeleIds);
        }
        form.Farming = await db.FarmingActivities.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.CropGrowth = await db.CropGrowths.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.CropPest = await db.CropPestAndDeseaseReports.AsNoTracking()
            .Include(x => x.ImpactDetails).ThenInclude(x => x.PestDisease)
            .Include(x => x.DefensiveActivities)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.Pasture = await db.PastureStatuses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.AnimalWater = await db.AnimalWaterSupplyStatuses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.AnimalHealth = await db.AnimalHealthStatuses.AsNoTracking()
            .Include(x => x.ImpactDetails).ThenInclude(x => x.Disease)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.HumanWater = await db.HumanDrinkWaterIssues.AsNoTracking()
            .Include(x => x.HumanDrinkWaterKebeles!).ThenInclude(x => x.Kebele)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.HumanWaterKebeles = form.HumanWater?.HumanDrinkWaterKebeles?
            .Select(x => x.Kebele?.LocationAmharicName ?? x.Kebele?.LocationName ?? "-").OrderBy(x => x).ToList() ?? [];
        form.Epidemic = await db.EpidemicDiseases.AsNoTracking()
            .Include(x => x.EpidemicDiseaseSelections!).ThenInclude(x => x.EpidemicDiseaseType)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.GrainSupply = await db.GrainSupplys.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.AnimalSupply = await db.AnimalSupplys.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.GrainPrice = await db.GrainPrices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.AnimalPrice = await db.AnimalPrice.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.GrainPriceRows = await db.GrainPricePerQuintals.AsNoTracking().Include(x => x.GrainType)
            .Where(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end).ToListAsync();
        form.AnimalPriceRows = await db.AnimalPricePerUnits.AsNoTracking().Include(x => x.AnimalType)
            .Where(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end).ToListAsync();
        form.Accidents = await db.WeeklyAccidents.AsNoTracking()
            .Include(x => x.AccidentDetails!).ThenInclude(x => x.AccidentType)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.Migration = await db.Migrations.AsNoTracking()
            .Include(x => x.MigrationDetails!).ThenInclude(x => x.OriginLocation)
            .Include(x => x.MigrationDetails!).ThenInclude(x => x.DestinationLocation)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.Death = await db.Deaths.AsNoTracking().Include(x => x.DeathDetails)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.AnimalMigrations = await db.AnimalMigrationRecords.AsNoTracking()
            .Include(x => x.OriginLocation).Include(x => x.DestinationLocation)
            .Where(x => x.WeeklyReportSubmissionId == submission.Id).ToListAsync();
        form.AssistanceRecipient = await db.AssistanceRecipients.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.Aid = await db.WeeklyProvideds.AsNoTracking()
            .Include(x => x.WeeklyProvidedDetail!).ThenInclude(x => x.SupplyType)
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.OtherProblem = await db.OtherProblems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        form.School = await db.SchoolReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == start && x.EndDate == end);
        return form;
    }

    private async Task<List<string>> KebeleNames(List<Guid> ids) => ids.Count == 0 ? [] :
        await db.Locations.AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(x => x.LocationAmharicName ?? x.LocationName ?? "-").OrderBy(x => x).ToListAsync();
}
