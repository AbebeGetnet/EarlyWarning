using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EarlyWarning.Services;

public interface IWeeklyIndicatorService
{
    /// <summary>
    /// Reads every wizard topic of the given submissions (one row per woreda and week) and returns one
    /// <see cref="WoredaWeekFacts"/> per reporting woreda, with its alert flags already raised.
    /// Summarise any subset with <see cref="WeeklyIndicators.Summarize"/>.
    /// </summary>
    Task<List<WoredaWeekFacts>> LoadAsync(IReadOnlyCollection<WeeklyReportSubmission> reports);
}

public sealed class WeeklyIndicatorService(EarlyWarningDbContext db) : IWeeklyIndicatorService
{
    public async Task<List<WoredaWeekFacts>> LoadAsync(IReadOnlyCollection<WeeklyReportSubmission> reports)
    {
        if (reports.Count == 0) return [];
        var ids = reports.Select(x => x.WoredaId).Distinct().ToList();
        // Same week matching as the other reports: only the reported weeks are read, then each woreda's own week is kept.
        var from = reports.Min(x => x.StartDate.Date);
        var to = reports.Max(x => x.EndDate.Date).AddDays(1);
        var keys = reports.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
        bool InWeek(Guid id, DateTime? start, DateTime? end) =>
            start.HasValue && end.HasValue && keys.Contains((id, start.Value.Date, end.Value.Date));

        var places = await AffectedLocations.LoadPlacesAsync(db, ids);
        var facts = ids.ToDictionary(id => id, id => new WoredaWeekFacts
        {
            WoredaId = id,
            Woreda = places.TryGetValue(id, out var p) && !string.IsNullOrWhiteSpace(p.Woreda) ? p.Woreda : "-",
            Zone = places.TryGetValue(id, out var z) && !string.IsNullOrWhiteSpace(z.Zone) ? z.Zone : "-"
        });

        foreach (var r in (await db.RainfallReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt))
        {
            r.DeserializeKebeles(); r.DeserializeDroughtKebeles();
            var f = facts[r.WoredaId];
            f.RainReported = true;
            f.NoRain = WeeklyMatrixReportService.ClassifyRain(r) == WeeklyMatrixReportService.RainClass.None;
            f.NoRainKebeles = r.NoRainKebeles;
            f.DroughtKebeles = AffectedLocations.KebeleListCount(r.SelectedDroughtKebeleIds);
            f.FloodKebeles = AffectedLocations.KebeleListCount(r.SelectedKebeleIds);
        }
        foreach (var x in (await db.FarmingActivities.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt))
        {
            var f = facts[x.WoredaId];
            f.MeherPlan = (decimal)x.MeherFarmPlan; f.MeherPloughed = (decimal)x.MeherPloughed; f.MeherSown = (decimal)x.MeherSown;
            f.BelgPlan = (decimal)x.AutumnFarmPlan; f.BelgPloughed = (decimal)x.AutumnPloughed; f.BelgSown = (decimal)x.AutumnSown;
        }
        foreach (var x in (await db.CropPestAndDeseaseReports.AsNoTracking().Include(x => x.ImpactDetails).Include(x => x.DefensiveActivities)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt))
        {
            if (!x.HasPestAndDeseasOccured) continue;
            var f = facts[x.WoredaId];
            f.PestOutbreak = true;
            // Damaged land and people are entered per pest; older reports only have the report-level figures.
            f.PestHectares = x.ImpactDetails.Count > 0 ? x.ImpactDetails.Sum(d => d.AffectedLandInHectares) : (decimal)(x.AffectedLandInHectar ?? 0);
            f.PestControlledHectares = x.DefensiveActivities.Sum(d => d.AreaCoveredInHectares ?? 0);
            f.PestPeople = x.ImpactDetails.Count > 0 ? x.ImpactDetails.Sum(People) : People(x);
        }
        foreach (var x in (await db.PastureStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt))
        {
            if (x.Enough) continue;
            var f = facts[x.WoredaId];
            f.PastureShortage = true;
            f.AnimalsAffectedByFeedShortage += x.NumberOfAnimalsAffected;
            f.PasturePeople = People(x);
        }
        foreach (var x in (await db.AnimalWaterSupplyStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt))
        {
            var f = facts[x.WoredaId];
            if (!x.Enough) { f.AnimalWaterShortage = true; f.AnimalWaterPeople = People(x); }
            if (x.AnimalBodyCondition == AnimalBodyCondition.ዝቅተኛ)
            {
                f.LowBodyCondition = true;
                f.AnimalsAffectedByFeedShortage += x.LowBodyConditionAnimalsAffected;
            }
        }
        // Animal health: Enough == false means an outbreak was reported (wizard convention).
        foreach (var x in (await db.AnimalHealthStatuses.AsNoTracking().Include(x => x.ImpactDetails)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt))
        {
            if (x.Enough) continue;
            var f = facts[x.WoredaId];
            f.AnimalDiseaseOutbreak = true;
            // NoOfKebeliesWithWaterSupply holds the wizard's total of affected animals (see the model).
            f.AnimalsAffectedByDisease = x.ImpactDetails.Count > 0 ? x.ImpactDetails.Sum(d => (long)d.NumberOfAffectedAnimals) : x.NoOfKebeliesWithWaterSupply;
            f.AnimalHealthPeople = x.ImpactDetails.Count > 0 ? x.ImpactDetails.Sum(People) : People(x);
        }
        foreach (var x in (await db.HumanDrinkWaterIssues.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
        {
            if (!x.HasWaterProblem) continue;
            var f = facts[x.WoredaId];
            f.HumanWaterProblem = true; f.HumanWaterKebeles = x.NumberOfAffectedKebeles; f.HumanWaterPeople = People(x);
        }
        foreach (var x in (await db.EpidemicDiseases.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
        {
            if (!x.HasEpidemicDisease) continue;
            var f = facts[x.WoredaId];
            f.Epidemic = true; f.EpidemicKebeles = x.NumberOfAffectedKebeles; f.EpidemicPeople = People(x);
            f.EpidemicPregnantLactating = x.PregnantWomen + x.LactatingMothers;
        }
        foreach (var x in (await db.SchoolReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, null))
        {
            if (!x.HasTeachingLearningProblem) continue;
            var f = facts[x.WoredaId];
            f.SchoolProblem = true; f.ClosedSchools = x.ClosedSchools; f.Dropouts = x.MaleStudentDropouts + x.FemaleStudentDropouts;
        }
        foreach (var x in (await db.MalnutritionReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, null))
        {
            var f = facts[x.WoredaId];
            f.ChildrenScreened = x.ChildrenScreened; f.ChildrenSevere = x.ChildrenSevere; f.ChildrenModerate = x.ChildrenModerate;
            f.PlwScreened = x.PregnantLactatingScreened; f.PlwModerate = x.PregnantLactatingModerate;
        }
        foreach (var x in (await db.GrainPricePerQuintals.AsNoTracking().Include(x => x.GrainType)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, x.GrainTypeId), x => x.ReportDate))
            if (x.WeeklyPrice > 0)
                facts[x.WoredaId].GrainPrices.Add(new PricePoint((x.GrainType?.GrainName ?? "-").Trim(), x.WeeklyPrice, WeeklyFormReportService.PreviousPrice(x.WeeklyPrice, x.PriceDifference)));
        foreach (var x in (await db.AnimalPricePerUnits.AsNoTracking().Include(x => x.AnimalType)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, x.AnimalTypeId), x => x.ReportDate))
            if (x.WeeklyPrice > 0)
                facts[x.WoredaId].AnimalPrices.Add(new PricePoint((x.AnimalType?.AnimalTypeName ?? "-").Trim(), x.WeeklyPrice, WeeklyFormReportService.PreviousPrice(x.WeeklyPrice, x.PriceDifference)));
        foreach (var x in (await db.WeeklyAccidents.AsNoTracking().Include(x => x.AccidentDetails)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
        {
            if (!x.HasAccident) continue;
            var f = facts[x.WoredaId];
            f.Hazard = true;
            // Accidents count households only (no family row).
            f.HazardHouseholds = (x.AccidentDetails ?? []).Sum(d => (long)d.AffectedHouseholdsMale + d.AffectedHouseholdsFemale);
            f.HazardHectares = (x.AccidentDetails ?? []).Sum(d => d.DamagedLandInHectares ?? 0);
        }
        foreach (var x in (await db.Migrations.AsNoTracking().Include(x => x.MigrationDetails)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
            if (x.HasMigration) facts[x.WoredaId].MigratedPeople = (x.MigrationDetails ?? []).Sum(People);
        foreach (var x in (await db.Deaths.AsNoTracking().Include(x => x.DeathDetails)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
            if (x.HasDeath) facts[x.WoredaId].Deaths = (x.DeathDetails ?? []).Sum(People);
        foreach (var x in (await db.OtherProblems.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
            if (x.HasOtherProblem) { facts[x.WoredaId].OtherProblem = true; facts[x.WoredaId].OtherProblemPeople = People(x); }
        foreach (var x in (await db.AssistanceRecipients.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
            facts[x.WoredaId].AidRecipients = People(x);
        foreach (var x in (await db.WeeklyProvideds.AsNoTracking().Include(x => x.WeeklyProvidedDetail!).ThenInclude(x => x.SupplyType)
                         .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
                     .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate))
        {
            var f = facts[x.WoredaId];
            if (x.IfProblem.HasValue && x.IfProblem != Problem.No) { f.AidProblem = true; f.AidProblemText = x.IfProblem.Value.GetDisplayName(); }
            f.Aid.AddRange((x.WeeklyProvidedDetail ?? [])
                .Where(d => d.RequestedQuantity > 0 || d.ProvidedQuantity > 0 || d.DistributedQuantity > 0)
                .Select(d => new AidLine(d.SupplyType?.SupplyTypeName ?? "-", d.Measurement.ToString(), d.RequestedQuantity, d.ProvidedQuantity, d.DistributedQuantity)));
        }
        var submissionIds = reports.Select(x => x.Id).ToList();
        foreach (var x in await db.AnimalMigrationRecords.AsNoTracking().Where(x => submissionIds.Contains(x.WeeklyReportSubmissionId)).ToListAsync())
        {
            if (!facts.TryGetValue(x.WoredaId, out var f)) continue;
            if (x.Direction == AnimalMigrationDirection.Outward) f.AnimalsMovedOut += x.NumberOfAnimals; else f.AnimalsMovedIn += x.NumberOfAnimals;
        }

        foreach (var f in facts.Values)
        {
            f.StaplePriceChange = WeeklyIndicators.AverageChange(f.GrainPrices);
            WeeklyIndicators.RaiseFlags(f);
        }
        return facts.Values.OrderBy(x => x.Zone).ThenBy(x => x.Woreda).ToList();
    }

    // Beneficiaries = household heads + family members (Extensions/Beneficiaries).
    private static long People(object row) => Beneficiaries.From(row)?.Total ?? 0;
}

/// <summary>
/// Key indicators, alert levels and recommended actions from <see cref="WoredaWeekFacts"/>.
/// Pure functions, so the per-zone and per-woreda reports summarise the same facts without new queries.
/// </summary>
public static class WeeklyIndicators
{
    // Staple grains and livestock used for the terms of trade, in order of preference.
    private static readonly string[] StapleGrains = ["በቆሎ", "ማሽላ", "ስንዴ", "ጤፍ", "ገብስ"];
    private static readonly string[] TradeAnimals = ["ፍየል", "በግ", "በሬ"];

    /// <summary>
    /// Flags for one woreda. Weights: 3 = life-threatening (human epidemic, deaths, GAM ≥ 15%), 2 = serious
    /// (no rain or drought, flood, livestock condition or disease, drinking water, hazard with households hit,
    /// displacement, GAM 10–15%), 1 = watch (pests, pasture or water shortage, price rise ≥ 10%, aid problem, …).
    /// </summary>
    public static void RaiseFlags(WoredaWeekFacts f)
    {
        void Flag(string code, int weight, string text) => f.Flags.Add(new IndicatorFlag(code, weight, text));
        if (f.NoRain) Flag("rain", 2, "ዝናብ አላገኘም");
        if (f.DroughtKebeles > 0) Flag("drought", 2, $"ድርቅ ({N(f.DroughtKebeles)} ቀበሌ)");
        if (f.FloodKebeles > 0) Flag("flood", 2, $"ከፍተኛ ዝናብ/ጎርፍ ({N(f.FloodKebeles)} ቀበሌ)");
        if (f.PestOutbreak)
        {
            var poorlyControlled = f.PestHectares > 0 && f.PestControlledHectares < f.PestHectares / 2;
            Flag("pest", poorlyControlled ? 2 : 1, "የሰብል ተባይ/በሽታ" + (f.PestHectares > 0 ? $" ({N(f.PestHectares)} ሄ/ር፣ የተከላከሉት {N(f.PestControlledHectares)} ሄ/ር)" : ""));
        }
        if (f.PastureShortage) Flag("pasture", 1, "የግጦሽ/መኖ እጥረት");
        if (f.AnimalWaterShortage) Flag("animal-water", 1, "የእንስሳት ውሃ እጥረት");
        if (f.LowBodyCondition) Flag("body", 2, "የእንስሳት አቋም ዝቅተኛ");
        if (f.AnimalDiseaseOutbreak) Flag("animal-disease", 2, "የእንስሳት በሽታ ወረርሽኝ" + (f.AnimalsAffectedByDisease > 0 ? $" ({N(f.AnimalsAffectedByDisease)} እንስሳት)" : ""));
        if (f.AnimalsMovedOut > 0) Flag("animal-out", 1, $"{N(f.AnimalsMovedOut)} እንስሳት ከወረዳው ወጥተዋል");
        if (f.HumanWaterProblem) Flag("human-water", 2, "የመጠጥ ውሃ እጥረት" + (f.HumanWaterPeople > 0 ? $" ({N(f.HumanWaterPeople)} ሰው)" : ""));
        if (f.Epidemic) Flag("epidemic", 3, "የሰው ወረርሽኝ" + (f.EpidemicPeople > 0 ? $" ({N(f.EpidemicPeople)} ሰው)" : ""));
        if (f.GamRate is { } gam && gam >= 5)
            Flag("gam", gam >= 15 ? 3 : gam >= 10 ? 2 : 1, $"የህጻናት የምግብ እጥረት GAM {N(gam)}%");
        if (f.ChildrenSevere > 0) Flag("sam", 1, $"{N(f.ChildrenSevere)} ህጻናት በከፍተኛ ደረጃ የምግብ እጥረት");
        if (f.PlwMamRate is { } plw && plw >= 10) Flag("plw", 1, $"የእናቶች የምግብ እጥረት {N(plw)}%");
        if (f.Hazard) Flag("hazard", f.HazardHouseholds > 0 ? 2 : 1, "አደጋ" + (f.HazardHouseholds > 0 ? $" ({N(f.HazardHouseholds)} አባወራ/እማወራ)" : ""));
        if (f.MigratedPeople > 0) Flag("displacement", 2, $"ስደት/መፈናቀል ({N(f.MigratedPeople)} ሰው)");
        if (f.Deaths > 0) Flag("deaths", 3, $"ሞት ({N(f.Deaths)} ሰው)");
        if (f.SchoolProblem && (f.ClosedSchools > 0 || f.Dropouts > 0))
            Flag("school", 1, $"ትምህርት ({N(f.ClosedSchools)} ትምህርት ቤት ተዘግቷል፣ {N(f.Dropouts)} ተማሪ አቋርጧል)");
        if (f.StaplePriceChange is { } price && price >= 10) Flag("price", 1, $"የእህል ዋጋ ጨምሯል (+{N(price)}%)");
        if (f.AidProblem) Flag("aid", 1, $"የእርዳታ ችግር ({f.AidProblemText})");
        if (f.OtherProblem) Flag("other", 1, "ሌላ ችግር");
    }

    /// <summary>Score ≥ 7 → emergency; ≥ 4, or any life-threatening flag → alarm; ≥ 1 → watch.</summary>
    public static AlertLevel LevelFor(IReadOnlyCollection<IndicatorFlag> flags)
    {
        var score = flags.Sum(x => x.Weight);
        if (score >= 7) return AlertLevel.Emergency;
        if (score >= 4 || flags.Any(x => x.Weight >= 3)) return AlertLevel.Alarm;
        return score >= 1 ? AlertLevel.Watch : AlertLevel.Normal;
    }

    public static string LevelName(AlertLevel level) => level switch
    {
        AlertLevel.Emergency => "አስቸኳይ ምላሽ",
        AlertLevel.Alarm => "ከፍተኛ ስጋት",
        AlertLevel.Watch => "ክትትል",
        _ => "መደበኛ"
    };

    /// <summary>The WHO prevalence bands for global acute malnutrition (screening results are indicative only).</summary>
    public static (AlertLevel Level, string Name) GamBand(decimal rate) => rate switch
    {
        >= 15 => (AlertLevel.Emergency, "በጣም ከፍተኛ (≥15%)"),
        >= 10 => (AlertLevel.Alarm, "ከፍተኛ (10–15%)"),
        >= 5 => (AlertLevel.Watch, "መካከለኛ (5–10%)"),
        _ => (AlertLevel.Normal, "ዝቅተኛ (<5%)")
    };

    /// <summary>Average % change of the price types that have a price in both weeks.</summary>
    public static decimal? AverageChange(IEnumerable<PricePoint> prices)
    {
        var changes = prices.Where(x => x.Current > 0 && x.Previous > 0).Select(x => (x.Current - x.Previous!.Value) / x.Previous.Value * 100).ToList();
        return changes.Count == 0 ? null : Math.Round(changes.Average(), 1);
    }

    // What to do about each flag. Texts are short and practical; each applies to the woredas that raised the flag.
    private static readonly Dictionary<string, (string Sector, string Action)> Responses = new()
    {
        ["epidemic"] = ("ጤና", "የጤና ባለሙያዎችንና መድሃኒት ወደ ተጎዱ ቀበሌዎች ማሰማራት፣ የበሽታ ቅኝትንና የማህበረሰብ ግንዛቤን ማጠናከር"),
        ["deaths"] = ("ጤና / አደጋ", "የሞቱን ምክንያት በአስቸኳይ ማጣራትና ተመሳሳይ ጉዳት እንዳይደርስ ተገቢ ምላሽ መስጠት"),
        ["gam"] = ("ስነ-ምግብ", "በከፍተኛ የምግብ እጥረት ለተጎዱ ህጻናት ህክምና (OTP/SC) ማረጋገጥ፣ የተጨማሪ ምግብ (TSFP) ድጋፍ ማስፋትና ልየታውን ማስቀጠል"),
        ["sam"] = ("ስነ-ምግብ", "በከፍተኛ ደረጃ የምግብ እጥረት የተገኘባቸውን ህጻናት ወደ ህክምና ማስተላለፍና መከታተል"),
        ["plw"] = ("ስነ-ምግብ", "ለእርጉዝና የሚያጠቡ እናቶች የተጨማሪ ምግብ ድጋፍ ማቅረብ"),
        ["human-water"] = ("ውሃ", "የመጠጥ ውሃ በቦቴ ማቅረብ፣ የውሃ ማከሚያ ኬሚካል ማሰራጨትና የተበላሹ የውሃ ተቋማትን መጠገን"),
        ["rain"] = ("ግብርና / ውሃ", "የዝናብ እጥረቱን መከታተል፣ የሰውና የእንስሳት ውሃ አቅርቦትን ማዘጋጀትና ድርቅ ተቋቋሚ ዘር ማቅረብ"),
        ["drought"] = ("ግብርና / ውሃ", "ለድርቅ ተጎጂ ቀበሌዎች የውሃ (በቦቴ) እና የእንስሳት መኖ አቅርቦት ማመቻቸት፣ የምግብ ድጋፍ ፍላጎትን መገምገም"),
        ["flood"] = ("አደጋ", "ለጎርፍ ተጎጂዎች የመጠለያ፣ የምግብና የምግብ ያልሆኑ ቁሳቁሶች ድጋፍ ማድረስ፤ የውሃ መውረጃዎችን ማጽዳት"),
        ["hazard"] = ("አደጋ", "የጉዳት ዳሰሳ ማካሄድና ለተጎዱ አባወራዎች አስቸኳይ የምግብና የምግብ ያልሆኑ ቁሳቁሶች ድጋፍ ማድረስ"),
        ["displacement"] = ("አደጋ / ስደት", "ለተሰደዱ/ለተፈናቀሉ ሰዎች መጠለያ፣ ምግብና ውሃ ማቅረብ፤ የመፈናቀሉን ምክንያት መከታተል"),
        ["animal-disease"] = ("እንስሳት", "የእንስሳት ክትባትና ህክምና ቡድን ማሰማራትና የእንስሳት እንቅስቃሴን መቆጣጠር"),
        ["body"] = ("እንስሳት", "ተጨማሪ መኖ ማቅረብና አቋማቸው የወረደ እንስሳትን ቀድሞ ለገበያ ማቅረብ (destocking) ማበረታታት"),
        ["pasture"] = ("እንስሳት", "የእንስሳት መኖ ማቅረብና የመኖ ክምችትን መጠበቅ"),
        ["animal-water"] = ("እንስሳት", "ለእንስሳት የውሃ ነጥቦችን መጠገንና የውሃ አቅርቦት ማመቻቸት"),
        ["animal-out"] = ("እንስሳት", "የእንስሳት ስደቱን ምክንያት (መኖ/ውሃ) መከታተልና በመዳረሻ አካባቢዎች ግጭት እንዳይፈጠር ማስተባበር"),
        ["pest"] = ("ግብርና", "የመከላከያ ኬሚካልና ባለሙያ ማሰማራት፣ ያልተሸፈነውን መሬት በመከላከል መሸፈንና ቅኝቱን ማጠናከር"),
        ["school"] = ("ትምህርት", "የተዘጉ ትምህርት ቤቶችን ለማስከፈትና ያቋረጡ ተማሪዎችን ለመመለስ የትምህርት ቤት ምገባና የውሃ ድጋፍ ማመቻቸት"),
        ["price"] = ("ገበያ", "የገበያ ዋጋ ክትትልን ማጠናከርና ለተጎጂ ቤተሰቦች የምግብ ወይም የገንዘብ ድጋፍ ማቀድ"),
        ["aid"] = ("እርዳታ", "በእርዳታ አቅርቦትና ስርጭት ላይ የታየውን ችግር መፍታትና ስርጭቱን ማፋጠን"),
        ["other"] = ("ሌላ", "የተገለጸውን ችግር በባለድርሻ አካላት ማጣራትና ምላሽ መስጠት")
    };

    /// <summary>The key indicators, priorities and recommended actions for a set of woredas.</summary>
    /// <param name="woredasInScope">Woredas expected to report (for the reporting coverage); 0 to leave it out.</param>
    public static WeeklyIndicatorSummary Summarize(IReadOnlyCollection<WoredaWeekFacts> facts, int woredasInScope)
    {
        var s = new WeeklyIndicatorSummary { WoredasReported = facts.Count };
        if (facts.Count == 0) return s;
        void Kpi(string group, string label, string value, string note = "", AlertLevel level = AlertLevel.Normal) =>
            s.Kpis.Add(new IndicatorKpi(group, label, value, note, level));
        int Count(Func<WoredaWeekFacts, bool> test) => facts.Count(test);
        string Woredas(int n) => $"{N(n)} ወረዳ";

        // ---- Coverage
        if (woredasInScope > 0)
        {
            var coverage = Math.Round(facts.Count * 100m / woredasInScope, 1);
            Kpi("ሪፖርት", "የሪፖርት ሽፋን", $"{N(facts.Count)} ከ {N(woredasInScope)} ({N(coverage)}%)",
                coverage < 100 ? $"{N(woredasInScope - facts.Count)} ወረዳዎች ሪፖርት አልላኩም" : "ሁሉም ወረዳዎች ሪፖርት ልከዋል",
                coverage < 50 ? AlertLevel.Alarm : coverage < 100 ? AlertLevel.Watch : AlertLevel.Normal);
        }

        // ---- Rain and farming
        var rainReported = Count(x => x.RainReported);
        var noRain = Count(x => x.NoRain);
        if (rainReported > 0)
            Kpi("ዝናብና እርሻ", "ዝናብ ያላገኙ ወረዳዎች", $"{N(noRain)} ከ {N(rainReported)}",
                noRain == 0 ? "" : $"{N(Math.Round(noRain * 100m / rainReported, 1))}% የሪፖርት ወረዳዎች",
                noRain == 0 ? AlertLevel.Normal : noRain * 4 >= rainReported ? AlertLevel.Alarm : AlertLevel.Watch);
        var drought = facts.Sum(x => x.DroughtKebeles);
        var flood = facts.Sum(x => x.FloodKebeles);
        if (drought > 0) Kpi("ዝናብና እርሻ", "በድርቅ የተጎዱ ቀበሌዎች", $"{N(drought)} ቀበሌ", Woredas(Count(x => x.DroughtKebeles > 0)), AlertLevel.Alarm);
        if (flood > 0) Kpi("ዝናብና እርሻ", "በከፍተኛ ዝናብ/ጎርፍ የተጎዱ ቀበሌዎች", $"{N(flood)} ቀበሌ", Woredas(Count(x => x.FloodKebeles > 0)), AlertLevel.Alarm);
        void Season(string name, decimal plan, decimal ploughed, decimal sown)
        {
            if (plan <= 0) return;
            Kpi("ዝናብና እርሻ", $"የ{name} እርሻ ዕቅድ አፈጻጸም", $"የታረሰ {N(Math.Round(ploughed / plan * 100, 1))}%፣ የተዘራ {N(Math.Round(sown / plan * 100, 1))}%",
                $"ዕቅድ {N(plan)} ሄ/ር፣ የታረሰ {N(ploughed)} ሄ/ር፣ የተዘራ {N(sown)} ሄ/ር");
        }
        Season("መኸር", facts.Sum(x => x.MeherPlan), facts.Sum(x => x.MeherPloughed), facts.Sum(x => x.MeherSown));
        Season("በልግ", facts.Sum(x => x.BelgPlan), facts.Sum(x => x.BelgPloughed), facts.Sum(x => x.BelgSown));
        var pestLand = facts.Sum(x => x.PestHectares);
        var pestWoredas = Count(x => x.PestOutbreak);
        if (pestWoredas > 0)
        {
            var controlled = facts.Sum(x => x.PestControlledHectares);
            var share = pestLand > 0 ? Math.Min(100, Math.Round(controlled / pestLand * 100, 1)) : (decimal?)null;
            Kpi("ዝናብና እርሻ", "በሰብል ተባይ/በሽታ የተጎዳ መሬት", $"{N(pestLand)} ሄ/ር",
                $"{Woredas(pestWoredas)}" + (share.HasValue ? $"፤ በመከላከል የተሸፈነ {N(controlled)} ሄ/ር ({N(share.Value)}%)" : ""),
                share is < 50 ? AlertLevel.Alarm : AlertLevel.Watch);
        }

        // ---- Livestock
        var feed = Count(x => x.PastureShortage || x.AnimalWaterShortage);
        if (feed > 0) Kpi("እንስሳት", "የግጦሽ ወይም የውሃ እጥረት ያለባቸው ወረዳዎች", Woredas(feed),
            $"ግጦሽ {N(Count(x => x.PastureShortage))}፣ ውሃ {N(Count(x => x.AnimalWaterShortage))}", AlertLevel.Watch);
        var low = Count(x => x.LowBodyCondition);
        if (low > 0) Kpi("እንስሳት", "የእንስሳት አቋም ዝቅተኛ የሆነባቸው ወረዳዎች", Woredas(low),
            facts.Sum(x => x.AnimalsAffectedByFeedShortage) is > 0 and var hungry ? $"በመኖ/አቋም የተጎዱ እንስሳት {N(hungry)}" : "", AlertLevel.Alarm);
        var sick = Count(x => x.AnimalDiseaseOutbreak);
        if (sick > 0) Kpi("እንስሳት", "በወረርሽኝ የተጠቁ እንስሳት", N(facts.Sum(x => x.AnimalsAffectedByDisease)), Woredas(sick), AlertLevel.Alarm);
        s.AnimalsMovedOut = facts.Sum(x => x.AnimalsMovedOut);
        s.AnimalsMovedIn = facts.Sum(x => x.AnimalsMovedIn);
        if (s.AnimalsMovedOut + s.AnimalsMovedIn > 0)
            Kpi("እንስሳት", "የእንስሳት ስደት", $"የወጡ {N(s.AnimalsMovedOut)}፣ የገቡ {N(s.AnimalsMovedIn)}", "", s.AnimalsMovedOut > 0 ? AlertLevel.Watch : AlertLevel.Normal);

        // ---- People
        void PeopleKpi(string label, Func<WoredaWeekFacts, bool> has, Func<WoredaWeekFacts, long> people, AlertLevel level, string unit = "ሰው")
        {
            var n = Count(has);
            if (n == 0) return;
            var total = facts.Where(has).Sum(people);
            Kpi("ሰዎች", label, total > 0 ? $"{N(total)} {unit}" : "-", Woredas(n), level);
        }
        PeopleKpi("በሰው ወረርሽኝ የተጠቁ", x => x.Epidemic, x => x.EpidemicPeople, AlertLevel.Emergency);
        PeopleKpi("በመጠጥ ውሃ እጥረት የተጎዱ", x => x.HumanWaterProblem, x => x.HumanWaterPeople, AlertLevel.Alarm);
        PeopleKpi("በአደጋ የተጎዱ", x => x.Hazard, x => x.HazardHouseholds, AlertLevel.Alarm, "አባወራ/እማወራ");
        PeopleKpi("የተሰደዱ/የተፈናቀሉ", x => x.MigratedPeople > 0, x => x.MigratedPeople, AlertLevel.Alarm);
        PeopleKpi("የሞቱ", x => x.Deaths > 0, x => x.Deaths, AlertLevel.Emergency);

        // ---- Nutrition
        var screened = facts.Sum(x => x.ChildrenScreened);
        var severe = facts.Sum(x => x.ChildrenSevere);
        var moderate = facts.Sum(x => x.ChildrenModerate);
        var plwScreened = facts.Sum(x => x.PlwScreened);
        var plwModerate = facts.Sum(x => x.PlwModerate);
        foreach (var f in facts.Where(x => x.ChildrenScreened > 0 || x.PlwScreened > 0))
            s.Malnutrition.Add(new MalnutritionRow(f.Woreda, f.Zone, f.ChildrenScreened, f.ChildrenSevere, f.ChildrenModerate, f.GamRate, f.PlwScreened, f.PlwModerate, f.PlwMamRate));
        if (screened > 0 || plwScreened > 0)
        {
            var gam = MalnutritionReport.Rate(severe + moderate, screened);
            var plw = MalnutritionReport.Rate(plwModerate, plwScreened);
            s.MalnutritionTotal = new MalnutritionRow("ድምር", "", screened, severe, moderate, gam, plwScreened, plwModerate, plw);
            if (gam is { } g)
            {
                var band = GamBand(g);
                Kpi("ስነ-ምግብ", "የህጻናት አጣዳፊ የምግብ እጥረት (GAM)", $"{N(g)}%",
                    $"{band.Name}፤ ከ{N(screened)} ህጻናት ከፍተኛ {N(severe)}፣ መካከለኛ {N(moderate)} (ከልየታ የተገኘ)", band.Level);
            }
            if (plw is { } m)
                Kpi("ስነ-ምግብ", "የእርጉዝና የሚያጠቡ እናቶች የምግብ እጥረት (MAM)", $"{N(m)}%", $"ከ{N(plwScreened)} እናቶች {N(plwModerate)}",
                    m >= 20 ? AlertLevel.Alarm : m >= 10 ? AlertLevel.Watch : AlertLevel.Normal);
        }

        // ---- Education
        var closed = facts.Sum(x => x.ClosedSchools);
        var dropouts = facts.Sum(x => x.Dropouts);
        if (closed + dropouts > 0)
            Kpi("ትምህርት", "የተዘጉ ትምህርት ቤቶች / ያቋረጡ ተማሪዎች", $"{N(closed)} / {N(dropouts)}", Woredas(Count(x => x.ClosedSchools + x.Dropouts > 0)),
                closed > 0 ? AlertLevel.Alarm : AlertLevel.Watch);

        // ---- Market
        if (AverageChange(facts.SelectMany(x => x.GrainPrices)) is { } grainChange)
            Kpi("ገበያ", "የእህል ዋጋ ለውጥ (አማካይ)", Signed(grainChange) + "%", "ከባለፈው ሳምንት ጋር ሲነጻጸር",
                grainChange >= 25 ? AlertLevel.Alarm : grainChange >= 10 ? AlertLevel.Watch : AlertLevel.Normal);
        if (AverageChange(facts.SelectMany(x => x.AnimalPrices)) is { } animalChange)
            Kpi("ገበያ", "የእንስሳት ዋጋ ለውጥ (አማካይ)", Signed(animalChange) + "%", "ከባለፈው ሳምንት ጋር ሲነጻጸር",
                animalChange <= -25 ? AlertLevel.Alarm : animalChange <= -10 ? AlertLevel.Watch : AlertLevel.Normal);
        s.TermsOfTrade.AddRange(Trade(facts));
        foreach (var t in s.TermsOfTrade)
            Kpi("ገበያ", $"የእንስሳትና የእህል ልውውጥ ({t.Animal} → {t.Grain})", $"1 {t.Animal} = {N(t.Current)} ኪ.ግ {t.Grain}",
                t.Previous.HasValue ? $"ባለፈው ሳምንት {N(t.Previous.Value)} ኪ.ግ ({Signed(t.Change ?? 0)}%)" : "",
                t.Change <= -25 ? AlertLevel.Alarm : t.Change <= -10 ? AlertLevel.Watch : AlertLevel.Normal);

        // ---- Aid
        s.AidRecipients = facts.Sum(x => x.AidRecipients);
        s.Aid.AddRange(facts.SelectMany(x => x.Aid).GroupBy(x => (x.Item, x.Unit)).OrderBy(g => g.Key.Item)
            .Select(g => new AidLine(g.Key.Item, g.Key.Unit, g.Sum(x => x.Requested), g.Sum(x => x.Provided), g.Sum(x => x.Distributed))));
        if (s.AidRecipients > 0) Kpi("እርዳታ", "የዕለት እርዳታ ተጠቃሚዎች", $"{N(s.AidRecipients)} ሰው", Woredas(Count(x => x.AidRecipients > 0)));
        // Quantities are in different units, so coverage is the average over items (each item counts once).
        var coverages = s.Aid.Where(x => x.Coverage.HasValue).Select(x => Math.Min(100, x.Coverage!.Value)).ToList();
        if (coverages.Count > 0)
        {
            var avg = Math.Round(coverages.Average(), 1);
            var short_ = s.Aid.Where(x => x.Gap > 0).Select(x => x.Item).ToList();
            Kpi("እርዳታ", "ከተጠየቀው የቀረበ (አማካይ በአይነት)", $"{N(avg)}%",
                short_.Count == 0 ? "ፍላጎቱ ሙሉ በሙሉ ተሸፍኗል" : $"ክፍተት ያለባቸው፡ {string.Join("፣ ", short_)}",
                avg < 50 ? AlertLevel.Alarm : avg < 80 ? AlertLevel.Watch : AlertLevel.Normal);
        }
        var rates = s.Aid.Where(x => x.DistributionRate.HasValue).Select(x => Math.Min(100, x.DistributionRate!.Value)).ToList();
        if (rates.Count > 0)
        {
            var avg = Math.Round(rates.Average(), 1);
            Kpi("እርዳታ", "ከቀረበው የተሰራጨ (አማካይ በአይነት)", $"{N(avg)}%", "", avg < 50 ? AlertLevel.Alarm : avg < 80 ? AlertLevel.Watch : AlertLevel.Normal);
        }
        var aidProblems = Count(x => x.AidProblem);
        if (aidProblems > 0) Kpi("እርዳታ", "የእርዳታ አቅርቦት/ስርጭት ችግር የገጠማቸው", Woredas(aidProblems), "", AlertLevel.Watch);

        // ---- People affected by cause (one person may appear under more than one cause)
        void Cause(string name, Func<WoredaWeekFacts, long> people)
        {
            var total = facts.Sum(people);
            if (total > 0) s.PeopleByCause.Add(new AffectedByCause(name, total, facts.Count(x => people(x) > 0)));
        }
        Cause("የሰው ወረርሽኝ", x => x.EpidemicPeople);
        Cause("የሰው መጠጥ ውሃ እጥረት", x => x.HumanWaterPeople);
        Cause("የሰብል ተባይ/በሽታ", x => x.PestPeople);
        Cause("የእንስሳት መኖ እጥረት", x => x.PasturePeople);
        Cause("የእንስሳት ውሃ እጥረት", x => x.AnimalWaterPeople);
        Cause("የእንስሳት በሽታ", x => x.AnimalHealthPeople);
        Cause("አደጋ (አባወራ/እማወራ)", x => x.HazardHouseholds);
        Cause("ስደት/መፈናቀል", x => x.MigratedPeople);
        Cause("ሞት", x => x.Deaths);
        Cause("ሌላ ችግር", x => x.OtherProblemPeople);

        // ---- Priorities and actions
        s.Priorities.AddRange(facts.Where(x => x.Flags.Count > 0)
            .OrderByDescending(x => x.Level).ThenByDescending(x => x.Score).ThenBy(x => x.Zone).ThenBy(x => x.Woreda)
            .Select(x => new WoredaPriority(x.Woreda, x.Zone, x.Score, x.Level, x.Flags.OrderByDescending(f => f.Weight).Select(f => f.Text).ToList())));
        var emergency = s.Priorities.Count(x => x.Level == AlertLevel.Emergency);
        var alarm = s.Priorities.Count(x => x.Level == AlertLevel.Alarm);
        Kpi("ቅድሚያ", "ቅድሚያ ትኩረት የሚያስፈልጋቸው ወረዳዎች", $"{N(emergency)} አስቸኳይ፣ {N(alarm)} ከፍተኛ ስጋት",
            $"ክትትል የሚያስፈልጋቸው {N(s.Priorities.Count(x => x.Level == AlertLevel.Watch))}",
            emergency > 0 ? AlertLevel.Emergency : alarm > 0 ? AlertLevel.Alarm : AlertLevel.Normal);
        // The overview line goes first.
        s.Kpis.Insert(0, s.Kpis[^1]);
        s.Kpis.RemoveAt(s.Kpis.Count - 1);

        var multipleZones = facts.Select(x => x.Zone).Distinct().Count() > 1;
        string Place(WoredaWeekFacts f) => multipleZones ? $"{f.Woreda} ({f.Zone})" : f.Woreda;
        s.Actions.AddRange(facts.SelectMany(f => f.Flags.Select(flag => (Flag: flag, Facts: f)))
            .Where(x => Responses.ContainsKey(x.Flag.Code))
            .GroupBy(x => x.Flag.Code)
            .Select(g => new RecommendedAction(Responses[g.Key].Sector, Responses[g.Key].Action,
                (AlertLevel)g.Max(x => x.Flag.Weight),
                g.Select(x => Place(x.Facts)).Distinct().Order().ToList()))
            .OrderByDescending(x => x.Level).ThenByDescending(x => x.Woredas.Count).ThenBy(x => x.Sector));
        // Scope-wide actions that are not tied to one woreda's flag.
        var gaps = s.Aid.Where(x => x.Gap > 0).ToList();
        if (gaps.Count > 0)
            s.Actions.Add(new RecommendedAction("እርዳታ",
                $"ያልተሸፈነውን የእርዳታ ፍላጎት ለመሸፈን ጥያቄ ማቅረብ ({string.Join("፣ ", gaps.Select(x => $"{x.Item} {N(x.Gap)} {x.Unit}"))})",
                AlertLevel.Watch, []));
        if (s.TermsOfTrade.Any(x => x.Change <= -10))
            s.Actions.Add(new RecommendedAction("ገበያ", "የእንስሳት አርቢዎች የመግዛት አቅም ቀንሷል፤ የገበያ ክትትልን ማጠናከርና የእንስሳት ግብይት ድጋፍ ማቀድ", AlertLevel.Watch, []));
        if (woredasInScope > facts.Count)
            s.Actions.Add(new RecommendedAction("ሪፖርት", $"ሪፖርት ያልላኩ {N(woredasInScope - facts.Count)} ወረዳዎች በወቅቱ እንዲልኩ ማሳሰብ", AlertLevel.Watch, []));
        return s;
    }

    /// <summary>Kilograms of the first staple grain with prices that one goat, sheep or ox buys, this week and last week.</summary>
    internal static IEnumerable<TermsOfTrade> Trade(IReadOnlyCollection<WoredaWeekFacts> facts)
    {
        static (decimal? Current, decimal? Previous) Average(IEnumerable<PricePoint> rows)
        {
            var list = rows.ToList();
            var current = list.Where(x => x.Current > 0).Select(x => x.Current).ToList();
            var previous = list.Where(x => x.Previous > 0).Select(x => x.Previous!.Value).ToList();
            return (current.Count == 0 ? null : current.Average(), previous.Count == 0 ? null : previous.Average());
        }
        var grain = StapleGrains.Select(name => (Name: name, Price: Average(facts.SelectMany(x => x.GrainPrices).Where(x => x.Type == name))))
            .FirstOrDefault(x => x.Price.Current > 0);
        if (grain.Name == null) yield break;
        foreach (var animal in TradeAnimals)
        {
            var price = Average(facts.SelectMany(x => x.AnimalPrices).Where(x => x.Type == animal));
            if (price.Current is not > 0) continue;
            var current = Math.Round(price.Current.Value / (grain.Price.Current!.Value / 100), 1);
            decimal? previous = price.Previous > 0 && grain.Price.Previous > 0
                ? Math.Round(price.Previous.Value / (grain.Price.Previous.Value / 100), 1) : null;
            yield return new TermsOfTrade(animal, grain.Name, current, previous);
        }
    }

    internal static string N(decimal value) => value.ToString(value % 1 == 0 ? "#,0" : "#,0.#", CultureInfo.InvariantCulture);
    internal static string N(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);
    private static string Signed(decimal value) => (value > 0 ? "+" : "") + N(value);
}
