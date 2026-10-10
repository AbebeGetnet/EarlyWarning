using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EarlyWarning.Services;

public interface IWeeklyAggregationService
{
    Task<WeeklyAggregateViewModel> BuildAsync(IReadOnlyCollection<WeeklyReportSubmission> reports);
    Task<WeeklyAggregateViewModel?> GetSnapshotAsync(Guid scopeId, LocationLevel level, DateTime start, DateTime end);
    Task SaveSnapshotAsync(Guid scopeId, LocationLevel level, DateTime start, DateTime end, WeeklySubmissionStatus status, string userId, IReadOnlyCollection<WeeklyReportSubmission> reports);
}

public sealed class WeeklyAggregationService(EarlyWarningDbContext db) : IWeeklyAggregationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<WeeklyAggregateViewModel> BuildAsync(IReadOnlyCollection<WeeklyReportSubmission> reports)
    {
        var result = new WeeklyAggregateViewModel { GeneratedAt = DateTime.UtcNow };
        if (reports.Count == 0) return result;
        var ids = reports.Select(x => x.WoredaId).Distinct().ToList();
        var keys = reports.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
        bool Match(Guid id, DateTime? start, DateTime? end) => start.HasValue && end.HasValue && keys.Contains((id, start.Value.Date, end.Value.Date));
        bool MatchRequired(Guid id, DateTime start, DateTime end) => keys.Contains((id, start.Date, end.Date));
        AggregateSectionViewModel Section(string name, params AggregateMetricViewModel[] metrics) => new() { Name = name, Metrics = metrics.ToList() };
        static AggregateMetricViewModel M(string name, decimal value, string unit = "", string method = "SUM") => new(name, value, unit, method);
        static decimal People(IEnumerable<(int mh, int fh, int mf, int ff, int cm, int cf, int ym, int yf, int em, int ef, int dm, int df)> rows) =>
            rows.Sum(x => (decimal)x.mh + x.fh + x.mf + x.ff + x.cm + x.cf + x.ym + x.yf + x.em + x.ef + x.dm + x.df);

        var rain = (await db.RainfallReports
            .Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .AsNoTracking()
            .Where(x => ids.Contains(x.WoredaId))
            .ToListAsync())
            .Where(x => Match(x.WoredaId, x.StartDate, x.EndDate))
            .ToList();
        result.FullRainCoverageKebeles = rain.Sum(x => x.FullCoverageKebeles); result.PartialRainCoverageKebeles = rain.Sum(x => x.PartialCoverageKebeles); result.NoRainKebeles = rain.Sum(x => x.NoRainKebeles);
        result.RainfallZones = rain
            .Where(x => x.Woreda.Parent != null)
            .GroupBy(x => new
            {
                x.Woreda.ParentId,
                Name = x.Woreda.Parent!.LocationAmharicName ?? x.Woreda.Parent.LocationName ?? ""
            })
            .Select(g => new RainfallZoneAggregateViewModel(
                g.Key.Name,
                g.Sum(x => x.FullCoverageKebeles),
                g.Sum(x => x.PartialCoverageKebeles),
                g.Sum(x => x.NoRainKebeles),
                g.Sum(x => x.NoDataKebeles)))
            .OrderBy(x => x.Zone)
            .ToList();
        result.Sections.Add(Section("1. የዝናብ ሁኔታ", M("ዝናብ ሙሉ በሙሉ ያገኙ ወረዳዎች ብዛት", result.FullRainCoverageKebeles), M("ዝናብ በከፊል ያገኙ ወረዳዎች ብዛት", result.PartialRainCoverageKebeles), M("ዝናብ ያላገኙ ወረዳዎች ብዛት", result.NoRainKebeles), M("መረጃ ያልተገኘላቸው ወረዳዎች ብዛትና ዝርዝር", rain.Sum(x => x.NoDataKebeles)), M("ዝቅተኛ", rain.Sum(x => x.LowAmountKebeles)), M("መካከለኛ", rain.Sum(x => x.MediumAmountKebeles)), M("ከፍተኛ", rain.Sum(x => x.HighAmountKebeles))));

        var farming = (await db.FarmingActivities
            .Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .AsNoTracking()
            .Where(x => ids.Contains(x.WoredaId))
            .ToListAsync())
            .Where(x => Match(x.WoredaId, x.StartDate, x.EndDate))
            .ToList();
        result.FarmingZones = farming
            .Where(x => x.Woreda.Parent != null)
            .GroupBy(x => new
            {
                x.Woreda.ParentId,
                Name = x.Woreda.Parent!.LocationAmharicName ?? x.Woreda.Parent.LocationName ?? ""
            })
            .Select(g => new FarmingZoneAggregateViewModel(
                g.Key.Name,
                (decimal)g.Sum(x => x.MeherFarmPlan),
                (decimal)g.Sum(x => x.MeherPloughed),
                (decimal)g.Sum(x => x.MeherSown),
                (decimal)g.Sum(x => x.MeherHarvestingHHarvesting),
                (decimal)g.Sum(x => x.MeherSownWithResidualMoisture),
                (decimal)g.Sum(x => x.AutumnFarmPlan),
                (decimal)g.Sum(x => x.AutumnSown)))
            .OrderBy(x => x.Zone)
            .ToList();
        result.Sections.Add(Section("2. የመኸር የእርሻ ሥራ እንቅስቃሴ", M("ዕቅድ በሄ/ር", (decimal)farming.Sum(x => x.MeherFarmPlan), "ሄ/ር"), M("የታረሰ በሄ/ር", (decimal)farming.Sum(x => x.MeherPloughed), "ሄ/ር"), M("የተዘራ በሄ/ር", (decimal)farming.Sum(x => x.MeherSown), "ሄ/ር"), M("አጨዳና ስብሰባ በሄ/ር", (decimal)farming.Sum(x => x.MeherHarvestingHHarvesting), "ሄ/ር"), M("በቀሪ እርጥበት የተዘራ በሄ/ር", (decimal)farming.Sum(x => x.MeherSownWithResidualMoisture), "ሄ/ር"), M("የበልግ ዕቅድ በሄ/ር", (decimal)farming.Sum(x => x.AutumnFarmPlan), "ሄ/ር"), M("የበልግ የተዘራ በሄ/ር", (decimal)farming.Sum(x => x.AutumnSown), "ሄ/ር")));

        var pests = (await db.CropPestAndDeseaseReports.Include(x => x.Woreda).AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => Match(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.AffectedCropLandHectares = pests.Sum(x => x.AffectedLandInHectar) ?? 0;
        result.CropAffectedPeople = (int)People(pests.Select(x => (x.MaleHouseHold,x.FemaleHouseHold,x.MaleFamily,x.FemaleFamily,x.ChildhMale,x.ChildFemale,x.YouthMale,x.YouthFemale,x.ElderlyMale,x.ElderlyFemale,x.DisabledMale,x.DisabledFemale)));
        result.Sections.Add(Section("3.2 የሰብል ተባይ ክስተት", M("በሰብል ተባይ የተጎዳ መሬት ሄ/ር", (decimal)result.AffectedCropLandHectares, "ሄ/ር"), M("በሰብል ተባይ የተጎዳ ማህበረሰብ", result.CropAffectedPeople)));

        var pasture = (await db.PastureStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => Match(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(Section("4.1 የእንስሳት ግጦሽ ሁኔታ", M("ዝቅተኛ ከሆነ የተጎዳ የእንስሳት ቁጥር", pasture.Sum(x => x.NumberOfAnimalsAffected)), M("የግጦሽ ሁኔታ ዝቅተኛ የሆነባቸው ወረዳዎች", pasture.Count(x => !x.Enough), "", "COUNT"), M("በእንስሳት የመኖ እጥረት የተጎዳ ማህበረሰብ", People(pasture.Select(x => (x.MaleHouseHold,x.FemaleHouseHold,x.MaleFamily,x.FemaleFamily,x.ChildhMale,x.ChildFemale,x.YouthMale,x.YouthFemale,x.ElderlyMale,x.ElderlyFemale,x.DisabledMale,x.DisabledFemale))))));

        var water = (await db.AnimalWaterSupplyStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => Match(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(Section("4.2 የእንስሳት ውሃ አቅርቦት ሁኔታ", M("በሳምንቱ የግጦሽ ችግር ያለባቸው ቀበሌዎች በቁጥር", water.Sum(x => x.NoOfKebeliesWithPastureShortage)), M("በሳምንቱ የውሃ ችግር ያለባቸው ቀበሌዎች በቁጥር", water.Sum(x => x.NoOfKebeliesWithWaterSupply)), M("የውሃ አቅርቦት ዝቅተኛ የሆነባቸው ወረዳዎች", water.Count(x => !x.Enough), "", "COUNT"), M("በውሃ እጥረት የተጎዳ ማህበረሰብ", People(water.Select(x => (x.MaleHouseHold,x.FemaleHouseHold,x.MaleFamily,x.FemaleFamily,x.ChildhMale,x.ChildFemale,x.YouthMale,x.YouthFemale,x.ElderlyMale,x.ElderlyFemale,x.DisabledMale,x.DisabledFemale))))));
        result.Sections.Add(Section("4.3 የእንስሳት አቋም", M("የእንስሳት አቋም መካከለኛ የሆነባቸው ወረዳዎች", water.Count(x => x.AnimalBodyCondition == AnimalBodyCondition.መካከለኛ), "", "COUNT"), M("የእንስሳት አቋም ዝቅተኛ የሆነባቸው ወረዳዎች", water.Count(x => x.AnimalBodyCondition == AnimalBodyCondition.ዝቅተኛ), "", "COUNT"), M("አቋማቸው ዝቅተኛ የሆነ የተጎዱ እንስሳት ብዛት", water.Sum(x => x.LowBodyConditionAnimalsAffected))));

        var health = (await db.AnimalHealthStatuses.Include(x => x.Woreda).AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => Match(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.AnimalDiseaseAffectedPeople = (int)People(health.Select(x => (x.MaleHouseHold,x.FemaleHouseHold,x.MaleFamily,x.FemaleFamily,x.ChildhMale,x.ChildFemale,x.YouthMale,x.YouthFemale,x.ElderlyMale,x.ElderlyFemale,x.DisabledMale,x.DisabledFemale)));
        result.Sections.Add(Section("4.3 የእንስሳት ጤና በወረርሽኝ መልክ የተከሰተ በሽታ", M("በችግሩ የተጎዳ ማህበረሰብ", result.AnimalDiseaseAffectedPeople), M("በሽታ የተከሰተባቸው ወረዳዎች", health.Count(x => !x.Enough), "", "COUNT")));

        var epidemics = (await db.EpidemicDiseases.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(Section("6. የሰው ጤና በሳምንቱ በወረርሽኝ መልኩ የተከሰተ በሽታ", M("የተጎዱ ቀበሌዎች ብዛት በቁጥር", epidemics.Sum(x => x.NumberOfAffectedKebeles)), M("በወረርሽኝ መልኩ በተከሰተ በሽታ የተጎዳ ማህበረሰብ", People(epidemics.Select(x => (x.MaleHouseholdHeads,x.FemaleHouseholdHeads,x.MaleFamilyMembers,x.FemaleFamilyMembers,x.MaleChildren,x.FemaleChildren,x.MaleYouth,x.FemaleYouth,x.MaleElderly,x.FemaleElderly,x.MaleDisabled,x.FemaleDisabled)))), M("ነፍሰ ጡር እናቶች በቁጥር", epidemics.Sum(x => x.PregnantWomen)), M("አጥቢ እናቶች በቁጥር", epidemics.Sum(x => x.LactatingMothers))));

        var humanWater = (await db.HumanDrinkWaterIssues.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(Section("5. የሰው መጠጥ ውሃ ሁኔታ", M("ችግር ያለባቸው ቀበሌዎች በቁጥር", humanWater.Sum(x => x.NumberOfAffectedKebeles)), M("በመጠጥ ውሃ የተጎዳ ማህበረሰብ", People(humanWater.Select(x => (x.MaleHouseholdHeads,x.FemaleHouseholdHeads,x.MaleFamilyMembers,x.FemaleFamilyMembers,x.MaleChildren,x.FemaleChildren,x.MaleYouth,x.FemaleYouth,x.MaleElderly,x.FemaleElderly,x.MaleDisabled,x.FemaleDisabled))))));

        var recipients = (await db.AssistanceRecipients.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(Section("10. የእርዳታ አቅርቦትና ስርጭት", M("የዕለት እርዳታ ተጠቃሚ", People(recipients.Select(x => (x.MaleHouseholdHeads,x.FemaleHouseholdHeads,x.MaleFamilyMembers,x.FemaleFamilyMembers,x.MaleChildren,x.FemaleChildren,x.MaleYouth,x.FemaleYouth,x.MaleElderly,x.FemaleElderly,x.MaleDisabled,x.FemaleDisabled))))));

        var migrations = (await db.Migrations.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var migrationIds = migrations.Select(x => x.MigrationId).ToList();
        var migrationPeople = await db.MigrationDetails.AsNoTracking().Where(x => migrationIds.Contains(x.MigrationId)).ToListAsync();
        result.Sections.Add(Section("በተለያዩ ምክንያት ስደት", M("የተሰደዱ ማህበረሰብ", People(migrationPeople.Select(x => (x.MaleHouseholdHeads,x.FemaleHouseholdHeads,x.MaleFamilyMembers,x.FemaleFamilyMembers,x.MaleChildren,x.FemaleChildren,x.MaleYouth,x.FemaleYouth,x.MaleElderly,x.FemaleElderly,x.MaleDisabled,x.FemaleDisabled))))));

        var deaths = (await db.Deaths.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var deathIds = deaths.Select(x => x.DeathId).ToList(); var deathRows = await db.DeathDetails.AsNoTracking().Where(x => deathIds.Contains(x.DeathId)).ToListAsync();
        result.Sections.Add(Section("የሞተ ካለ", M("የሞተ ማህበረሰብ", People(deathRows.Select(x => (x.MaleHouseholdHeads,x.FemaleHouseholdHeads,x.MaleFamilyMembers,x.FemaleFamilyMembers,x.MaleChildren,x.FemaleChildren,x.MaleYouth,x.FemaleYouth,x.MaleElderly,x.FemaleElderly,x.MaleDisabled,x.FemaleDisabled))))));

        var accidents = (await db.WeeklyAccidents.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var accidentIds = accidents.Select(x => x.WeeklyAccidentsId).ToList(); var accidentRows = await db.AccidentDetails.AsNoTracking().Where(x => accidentIds.Contains(x.WeeklyAccidentsId)).ToListAsync();
        result.Sections.Add(Section("9. በሳምንቱ የታዩ ወቅታዊ አደጋዎች", M("የተጎዳ መሬት በሄ/ር", accidentRows.Sum(x => x.DamagedLandInHectares) ?? 0, "ሄ/ር"), M("የተጎዳ ቤተሰብ", accidentRows.Sum(x => x.AffectedHouseholdsMale + x.AffectedHouseholdsFemale)), M("በእያንዳንዱ ጉዳት የተጎዳ ማህበረሰብ", accidentRows.Sum(x => x.AffectedChildrenMale+x.AffectedChildrenFemale+x.AffectedYouthMale+x.AffectedYouthFemale+x.AffectedElderlyMale+x.AffectedElderlyFemale+x.AffectedDisabledMale+x.AffectedDisabledFemale))));

        var other = (await db.OtherProblems.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(Section("11. ሌላ ያልተገለፀ በሳምንቱ ያጋጠመ ችግር ካለ", M("ችግር የተመዘገበባቸው ወረዳዎች", other.Count(x => x.HasOtherProblem), "", "COUNT"), M("በጉዳቱ የተጎዳ ማህበረሰብ", People(other.Select(x => (x.MaleHouseholdHeads,x.FemaleHouseholdHeads,x.MaleFamilyMembers,x.FemaleFamilyMembers,x.MaleChildren,x.FemaleChildren,x.MaleYouth,x.FemaleYouth,x.MaleElderly,x.FemaleElderly,x.MaleDisabled,x.FemaleDisabled))))));

        var aid = (await db.WeeklyProvideds.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var aidIds = aid.Select(x => x.WeeklyProvidedId).ToList(); var aidRows = await db.WeeklyProvidedDetails.AsNoTracking().Where(x => aidIds.Contains(x.WeeklyProvidedId)).ToListAsync();
        result.Sections.Add(Section("10.1 በሳምንቱ የቀረበ", M("የቀረበ", aidRows.Sum(x => x.ProvidedQuantity)), M("የተሰራጨ", aidRows.Sum(x => x.DistributedQuantity)), M("ከቀረበው የተሰራጨው በፐርሰንት", aidRows.Sum(x => x.ProvidedQuantity) == 0 ? 0 : aidRows.Sum(x => x.DistributedQuantity) / aidRows.Sum(x => x.ProvidedQuantity) * 100, "%", "RECALCULATED")));

        var grainPrices = (await db.GrainPricePerQuintals.Include(x => x.GrainType).AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var grainSupply = (await db.GrainSupplys.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(new AggregateSectionViewModel { Name = "8.1 የእህል አቅርቦት ከባለፈው ሳምንት ጋር ሲነጻጸር", Metrics = grainSupply.GroupBy(x => x.GrainSupplyStatus).Select(g => M(g.Key.ToString(), g.Count(), "ወረዳ", "COUNT")).ToList() });
        result.Sections.Add(new AggregateSectionViewModel { Name = "8.2 የእህል ዋጋ ከባለፈው ሳምንት ጋር ሲነጻጸር", Metrics = grainPrices.GroupBy(x => x.GrainType?.GrainName ?? x.GrainTypeId.ToString()).SelectMany(g => new[] { M($"{g.Key} — ያለፈው ሳምንት", g.Where(x => x.PreviousWeekPrice.HasValue).Select(x => x.PreviousWeekPrice!.Value).DefaultIfEmpty().Average(), "ብር", "AVERAGE"), M($"{g.Key} — በዚህ ሳምንት", g.Average(x => x.WeeklyPrice), "ብር", "AVERAGE") }).ToList() });
        var animalPrices = (await db.AnimalPricePerUnits.Include(x => x.AnimalType).AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var animalSupply = (await db.AnimalSupplys.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        result.Sections.Add(new AggregateSectionViewModel { Name = "8.3 የእንስሳት አቅርቦት ከባለፈው ሳምንት ጋር ሲነጻጸር", Metrics = animalSupply.GroupBy(x => x.GrainSupplyStatus).Select(g => M(g.Key.ToString(), g.Count(), "ወረዳ", "COUNT")).ToList() });
        result.Sections.Add(new AggregateSectionViewModel { Name = "8.4 የእንስሳት ዋጋ ከባለፈው ሳምንት ጋር ሲነጻጸር", Metrics = animalPrices.GroupBy(x => x.AnimalType?.AnimalTypeName ?? x.AnimalTypeId.ToString()).SelectMany(g => new[] { M($"{g.Key} — ያለፈው ሳምንት", g.Where(x => x.PreviousWeekPrice.HasValue).Select(x => x.PreviousWeekPrice!.Value).DefaultIfEmpty().Average(), "ብር", "AVERAGE"), M($"{g.Key} — በዚህ ሳምንት", g.Average(x => x.WeeklyPrice), "ብር", "AVERAGE") }).ToList() });

        var submissionIds = reports.Select(x => x.Id).ToList();
        var movements = await db.AnimalMigrationRecords.Include(x => x.OriginLocation).Include(x => x.DestinationLocation).AsNoTracking().Where(x => submissionIds.Contains(x.WeeklyReportSubmissionId)).ToListAsync();
        result.AnimalMigrationRoutes = movements.GroupBy(x => new { x.Direction,x.OriginLocationId,x.DestinationLocationId,x.AnimalType,x.OtherAnimalType }).Select(g => new AnimalMigrationAggregateViewModel { Direction=g.Key.Direction.ToString(), Origin=g.First().OriginLocation.LocationAmharicName??g.First().OriginLocation.LocationName??"", Destination=g.First().DestinationLocation.LocationAmharicName??g.First().DestinationLocation.LocationName??"", AnimalType=g.Key.AnimalType==MigratingAnimalType.Other?g.Key.OtherAnimalType??"Other":g.Key.AnimalType.ToString().Replace('_',' '), NumberOfAnimals=g.Sum(x=>(long)x.NumberOfAnimals)}).ToList();
        result.Sections.Add(Section("በተለያዩ ምክንያት ስደት — የእንስሳት ስደት", M("የተሰደዱ እንስሳት ብዛት", result.AnimalMigrationRoutes.Sum(x => x.NumberOfAnimals))));

        var schools = (await db.SchoolReports.Include(x => x.Woreda).AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync()).Where(x => MatchRequired(x.WoredaId, x.StartDate, x.EndDate)).ToList();
        var dropoutCount = schools.Sum(x => x.MaleStudentDropouts + x.FemaleStudentDropouts);
        var weightedAgePopulation = schools.Where(x => x.AverageDropoutAge.HasValue).Sum(x => x.MaleStudentDropouts + x.FemaleStudentDropouts);
        var weightedAverageAge = weightedAgePopulation == 0 ? 0 : schools.Where(x => x.AverageDropoutAge.HasValue)
            .Sum(x => x.AverageDropoutAge!.Value * (x.MaleStudentDropouts + x.FemaleStudentDropouts)) / weightedAgePopulation;
        result.Sections.Add(Section("7. የትምህርት ሁኔታ",
            M("በሳምንቱ ያጋጠመ ችግር አለ", schools.Count(x => x.HasTeachingLearningProblem), "ወረዳ", "COUNT"),
            M("ያቋረጡ ተማሪዎች — ወንድ", schools.Sum(x => x.MaleStudentDropouts), "ተማሪ"),
            M("ያቋረጡ ተማሪዎች — ሴት", schools.Sum(x => x.FemaleStudentDropouts), "ተማሪ"),
            M("ጠቅላላ ከትምህርት የወጡ", dropoutCount, "ተማሪ"),
            M("አማካይ የማቋረጥ ጥምርታ/ምጣኔ", schools.Any(x => x.DropoutRatePercent.HasValue) ? schools.Where(x => x.DropoutRatePercent.HasValue).Average(x => x.DropoutRatePercent!.Value) : 0, "%", "AVERAGE"),
            M("የወጡ ተማሪዎች የተመዘነ አማካይ ዕድሜ", weightedAverageAge, "ዓመት", "WEIGHTED AVERAGE"),
            M("የተዘጉ ትምህርት ቤቶች ብዛት", schools.Sum(x => x.ClosedSchools), "ትምህርት ቤት"),
            M("እርምጃው አጥጋቢ የነበረባቸው", schools.Count(x => x.WasActionSatisfactory == true), "ወረዳ", "COUNT"),
            M("የተማሪ ጉዳይ መፍትሄ", schools.Count(x => x.StudentRelatedSolution), "ወረዳ", "COUNT"),
            M("የመምህራን ወጪ መፍትሄ", schools.Count(x => x.TeacherExpenseSolution), "ወረዳ", "COUNT"),
            M("ክፍል/ቁሳቁስ መፍትሄ", schools.Count(x => x.ClassroomMaterialsSolution), "ወረዳ", "COUNT"),
            M("የሕንፃ አገልግሎት መፍትሄ", schools.Count(x => x.SchoolBuildingServiceSolution), "ወረዳ", "COUNT"),
            M("ሌላ መፍትሄ", schools.Count(x => x.OtherSolution), "ወረዳ", "COUNT")));

        var comments = await db.WeeklyReportWorkflowHistories.Include(x=>x.Submission).ThenInclude(x=>x.Woreda).AsNoTracking().Where(x=>submissionIds.Contains(x.WeeklyReportSubmissionId)&&x.Comment!=null&&x.Comment!="").ToListAsync();
        var schoolTexts = schools.Where(x => x.HasTeachingLearningProblem).SelectMany(x => new[]
        {
            new AggregatedTextViewModel("የትምህርት ችግር ምክንያት", x.Woreda?.LocationAmharicName ?? x.Woreda?.LocationName ?? "", x.Cause ?? "-"),
            new AggregatedTextViewModel("ሶስት ዋና መፍትሄዎች", x.Woreda?.LocationAmharicName ?? x.Woreda?.LocationName ?? "", string.Join(" | ", new[] { x.MajorSolutionOne, x.MajorSolutionTwo, x.MajorSolutionThree }.Where(v => !string.IsNullOrWhiteSpace(v))))
        }).Concat(schools.Where(x => x.OtherSolution && !string.IsNullOrWhiteSpace(x.OtherSolutionDescription)).Select(x => new AggregatedTextViewModel("ሌላ የትምህርት መፍትሄ", x.Woreda?.LocationAmharicName ?? x.Woreda?.LocationName ?? "", x.OtherSolutionDescription!)));
        result.TextEntries = pests.Where(x=>!string.IsNullOrWhiteSpace(x.Remarks)).Select(x=>new AggregatedTextViewModel("የሰብል ተባይ/በሽታ",x.Woreda?.LocationAmharicName??"",x.Remarks!)).Concat(health.Where(x=>!string.IsNullOrWhiteSpace(x.Remarks)).Select(x=>new AggregatedTextViewModel("የእንስሳት ጤና",x.Woreda?.LocationAmharicName??"",x.Remarks!))).Concat(schoolTexts).Concat(comments.Select(x=>new AggregatedTextViewModel("የፍቃድ/ውድቅ አስተያየት",x.Submission.Woreda.LocationAmharicName??x.Submission.Woreda.LocationName??"",x.Comment!))).ToList();
        result.WoredaReports=reports.Count; result.ApprovedReports=reports.Count(x=>x.Status is WeeklySubmissionStatus.ZoneApproved or WeeklySubmissionStatus.RegionApproved or WeeklySubmissionStatus.RegionRejectedToZone); result.PendingReports=reports.Count-result.ApprovedReports;
        return result;
    }

    public async Task SaveSnapshotAsync(Guid scopeId, LocationLevel level, DateTime start, DateTime end, WeeklySubmissionStatus status, string userId, IReadOnlyCollection<WeeklyReportSubmission> reports)
    {
        var aggregate = await BuildAsync(reports); aggregate.IsSnapshot = true;
        var existing = await db.WeeklyAggregateSnapshots.Where(x=>x.ScopeLocationId==scopeId&&x.ScopeLevel==level&&x.StartDate==start.Date&&x.EndDate==end.Date&&x.ApprovalStatus==status).ToListAsync();
        db.WeeklyAggregateSnapshots.RemoveRange(existing);
        db.WeeklyAggregateSnapshots.Add(new WeeklyAggregateSnapshot { ScopeLocationId=scopeId,ScopeLevel=level,StartDate=start.Date,EndDate=end.Date,ApprovalStatus=status,SourceReportCount=reports.Count,GeneratedAt=DateTime.UtcNow,GeneratedById=userId,AggregateJson=JsonSerializer.Serialize(aggregate,JsonOptions)});
        await db.SaveChangesAsync();
    }

    public async Task<WeeklyAggregateViewModel?> GetSnapshotAsync(Guid scopeId, LocationLevel level, DateTime start, DateTime end)
    {
        var row=await db.WeeklyAggregateSnapshots.AsNoTracking().Where(x=>x.ScopeLocationId==scopeId&&x.ScopeLevel==level&&x.StartDate==start.Date&&x.EndDate==end.Date).OrderByDescending(x=>x.GeneratedAt).FirstOrDefaultAsync();
        if(row==null)return null; var value=JsonSerializer.Deserialize<WeeklyAggregateViewModel>(row.AggregateJson,JsonOptions); if(value!=null){value.IsSnapshot=true;value.GeneratedAt=row.GeneratedAt;} return value;
    }
}
