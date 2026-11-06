using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using GrainSupplyStatus = EarlyWarning.Models.SupplyofGrain.SupplyStatus;
using GrainPriceStatus = EarlyWarning.Models.PriceofGrain.SupplyStatus;
using AnimalSupplyStatus = EarlyWarning.Models.AnimalSupply.SupplyStatus;
using AnimalPriceStatus = EarlyWarning.Models.AnimalPrice.SupplyStatus;

namespace EarlyWarning.Services;

public interface IWeeklyFormReportService
{
    /// <summary>
    /// Builds the paper-form report (sections 1–11) for the given woreda submissions:
    /// numeric fields are totalled, yes/no and status answers are counted per woreda,
    /// and free text is concatenated per zone as "woreda፡ text".
    /// </summary>
    /// <param name="level">Woreda reports show plain text; zone and region reports group text by zone.</param>
    Task<List<FormReportSection>> BuildAsync(IReadOnlyCollection<WeeklyReportSubmission> reports, ReportScopeLevel level);
}

public sealed class WeeklyFormReportService(EarlyWarningDbContext db) : IWeeklyFormReportService
{
    private const string Woredas = "ወረዳ";

    public async Task<List<FormReportSection>> BuildAsync(IReadOnlyCollection<WeeklyReportSubmission> reports, ReportScopeLevel level)
    {
        var sections = new List<FormReportSection>();
        if (reports.Count == 0) return sections;

        var ids = reports.Select(x => x.WoredaId).Distinct().ToList();
        var keys = reports.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
        bool InWeek(Guid id, DateTime? start, DateTime? end) =>
            start.HasValue && end.HasValue && keys.Contains((id, start.Value.Date, end.Value.Date));
        var woredaNames = reports.GroupBy(x => x.WoredaId).ToDictionary(g => g.Key, g => Name(g.First().Woreda));
        var zoneNames = reports.GroupBy(x => x.WoredaId).ToDictionary(g => g.Key, g => g.First().Woreda?.Parent is { } zone ? Name(zone) : "-");

        var places = await AffectedLocations.LoadPlacesAsync(db, ids);

        // Woredas counted once each; region reports also count the zones those woredas belong to.
        string Places(IEnumerable<Guid> woredaIds)
        {
            var woredas = woredaIds.Distinct().ToList();
            if (level != ReportScopeLevel.Region) return Num(woredas.Count);
            var zones = woredas.Select(id => places.GetValueOrDefault(id)?.ZoneId).Where(z => z.HasValue).Distinct().Count();
            return $"{Num(woredas.Count)} {Woredas} / {Num(zones)} ዞን";
        }
        var occurredIn = level == ReportScopeLevel.Region ? "የተከሰተባቸው ወረዳዎች / ዞኖች" : "የተከሰተባቸው ወረዳዎች";
        FormReportItem CountWoredas(string label, IEnumerable<Guid> woredaIds, int indent = 1) =>
            level == ReportScopeLevel.Region ? LineText("•", label, Places(woredaIds), indent) : CountLine(label, woredaIds.Distinct().Count(), indent);

        // An event that affects kebeles: the kebele total, then (zone/region) the affected woredas,
        // (region) the affected zones, and the affected woredas listed under their zone.
        void Affected(List<FormReportItem> items, string number, string label, string eventName, IEnumerable<(Guid WoredaId, int Kebeles)> rows, int indent = 0)
        {
            var summary = AffectedLocations.Summarize(eventName, rows, places);
            items.Add(Line(number, label, summary.Kebeles, indent));
            if (level == ReportScopeLevel.Woreda) return;
            items.Add(LineText("", "የተጎዱ ወረዳዎች ብዛት", $"{Num(summary.Woredas)} {Woredas}", indent + 1));
            if (level == ReportScopeLevel.Region)
                items.Add(LineText("", "የተጎዱ ዞኖች ብዛት", $"{Num(summary.Zones)} ዞን", indent + 1));
            if (summary.Woredas == 0) return;
            // Region: "Zone A ዞን (3 ወረዳ)፣ …"; then under each zone "Woreda A (5 ቀበሌ)፣ … ድምር፡ …".
            if (level == ReportScopeLevel.Region)
                items.Add(LineText("", "የተጎዱ ዞኖች", AffectedLocations.ZoneList(summary), indent + 1));
            items.Add(new FormReportItem
            {
                Kind = FormReportItemKind.ZoneText,
                Label = "የተጎዱ ወረዳዎች",
                Indent = indent + 1,
                ZoneTexts = AffectedLocations.ByZone(summary)
                    .Select(g => new FormReportZoneText(
                        level == ReportScopeLevel.Region ? AffectedLocations.ZoneWithWoredas(g.Key, g.Count()) : $"{g.Key} ዞን",
                        $"{AffectedLocations.WoredaList(g)}። {AffectedLocations.Totals(g)}"))
                    .ToList()
            });
        }

        FormReportItem Texts(string label, IEnumerable<(Guid WoredaId, string? Text)> rows, int indent = 1) => new()
        {
            Kind = FormReportItemKind.ZoneText,
            Label = label,
            Indent = indent,
            ZoneTexts = level == ReportScopeLevel.Woreda
                // A woreda report is a single woreda: show its own text without zone/woreda prefixes.
                ? rows.Where(r => !string.IsNullOrWhiteSpace(r.Text)).Select(r => r.Text!.Trim()).Distinct()
                    .Select(text => new FormReportZoneText("", text)).ToList()
                : rows.Where(r => !string.IsNullOrWhiteSpace(r.Text))
                    .GroupBy(r => zoneNames.GetValueOrDefault(r.WoredaId, "-"))
                    .OrderBy(g => g.Key)
                    .Select(g => new FormReportZoneText($"{g.Key} ዞን", string.Join("፤ ",
                        g.Select(r => $"{woredaNames.GetValueOrDefault(r.WoredaId, "")}፡ {r.Text!.Trim()}").Distinct())))
                    .ToList()
        };

        // ------------------------------------------------------------------ 1. Rain
        var rain = (await db.RainfallReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        foreach (var report in rain) { report.DeserializeKebeles(); report.DeserializeDroughtKebeles(); }
        var s1 = Section(sections, "1", "የዝናብ ሁኔታ");
        s1.Items.Add(Line("1.1", "ዝናብ ሙሉ በሙሉ ያገኙ ቀበሌዎች ብዛት", rain.Sum(x => x.FullCoverageKebeles)));
        s1.Items.Add(Texts("ዝርዝር", rain.Select(x => (x.WoredaId, x.FullCoveredKebelieList))));
        s1.Items.Add(Line("1.2", "ዝናብ በከፊል ያገኙ ቀበሌዎች ብዛት", rain.Sum(x => x.PartialCoverageKebeles)));
        Affected(s1.Items, "1.3", "ዝናብ ያላገኙ ቀበሌዎች ብዛት", AffectedLocations.Events.NoRain, rain.Select(x => (x.WoredaId, x.NoRainKebeles)));
        s1.Items.Add(Texts("ዝርዝር", rain.Select(x => (x.WoredaId, x.NoRainKebelieList))));
        s1.Items.Add(Line("1.4", "መረጃ ያልተገኘላቸው ቀበሌዎች ብዛት", rain.Sum(x => x.NoDataKebeles)));
        s1.Items.Add(Heading("1.5", "የዝናቡ መጠንና ስርጭት"));
        s1.Items.Add(Line("•", "ከፍተኛ", rain.Sum(x => x.HighAmountKebeles), 1));
        s1.Items.Add(Texts("ዝርዝር", rain.Select(x => (x.WoredaId, x.HighRainKebelieList)), 2));
        Affected(s1.Items, "", "በከፍተኛ ዝናብ/ጎርፍ የተጎዱ ቀበሌዎች ብዛት", AffectedLocations.Events.Flood,
            rain.Select(x => (x.WoredaId, AffectedLocations.KebeleListCount(x.SelectedKebeleIds))), 2);
        s1.Items.Add(Line("•", "መካከለኛ", rain.Sum(x => x.MediumAmountKebeles), 1));
        s1.Items.Add(Line("•", "ከመካከለኛ - ከፍተኛ", rain.Sum(x => x.MediumHighAmountKebeles), 1));
        s1.Items.Add(Line("•", "ዝቅተኛ", rain.Sum(x => x.LowAmountKebeles), 1));
        s1.Items.Add(Texts("ዝርዝር", rain.Select(x => (x.WoredaId, x.LowRainKebeliesList)), 2));
        Affected(s1.Items, "", "በድርቅ የተጎዱ ቀበሌዎች ብዛት", AffectedLocations.Events.Drought,
            rain.Select(x => (x.WoredaId, AffectedLocations.KebeleListCount(x.SelectedDroughtKebeleIds))), 2);
        s1.Items.Add(Line("•", "ከዝቅተኛ - መካከለኛ", rain.Sum(x => x.LowMediumAmountKebeles), 1));
        s1.Items.Add(Line("•", "መረጃ ያልተገኘ", rain.Sum(x => x.NoDataForRainAmountKebeles), 1));
        s1.Items.Add(Texts("ማስታወሻ", rain.Select(x => (x.WoredaId, x.Remarks)), 0));

        // ------------------------------------------------------------------ 2. Farming
        var farming = (await db.FarmingActivities.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        var s2 = Section(sections, "2", "የመኸር የእርሻ ሥራ እንቅስቃሴ");
        s2.Items.Add(Line("2.1", "ዕቅድ በሄ/ር", (decimal)farming.Sum(x => x.MeherFarmPlan)));
        s2.Items.Add(Line("2.2", "የታረሰ በሄ/ር", (decimal)farming.Sum(x => x.MeherPloughed)));
        s2.Items.Add(Line("2.3", "የተዘራ በሄ/ር", (decimal)farming.Sum(x => x.MeherSown)));
        s2.Items.Add(Line("2.4", "የተሰበሰበ/አጨዳ በሄ/ር", (decimal)farming.Sum(x => x.MeherHarvestingHHarvesting)));
        s2.Items.Add(Line("2.5", "በቀሪ እርጥበት የተዘራ በሄ/ር", (decimal)farming.Sum(x => x.MeherSownWithResidualMoisture)));
        s2.Items.Add(Heading("", "የበልግ የእርሻ ሥራ እንቅስቃሴ"));
        s2.Items.Add(Line("", "ዕቅድ በሄ/ር", (decimal)farming.Sum(x => x.AutumnFarmPlan), 1));
        s2.Items.Add(Line("", "የታረሰ በሄ/ር", (decimal)farming.Sum(x => x.AutumnPloughed), 1));
        s2.Items.Add(Line("", "የተዘራ በሄ/ር", (decimal)farming.Sum(x => x.AutumnSown), 1));
        s2.Items.Add(Line("", "አጨዳና ስብሰባ በሄ/ር", (decimal)farming.Sum(x => x.AutumnHarvestingHHarvesting), 1));
        s2.Items.Add(Texts("ማስታወሻ", farming.Select(x => (x.WoredaId, x.Remarks)), 0));

        // ------------------------------------------------------------------ 3. Crops
        var growth = (await db.CropGrowths.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        var pests = (await db.CropPestAndDeseaseReports.AsNoTracking()
                .Include(x => x.ImpactDetails).ThenInclude(x => x.PestDisease)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        var s3 = Section(sections, "3", "የሰብል ሁኔታ");
        s3.Items.Add(Heading("3.1", "የሰብል እድገትና አቋም ሁኔታ"));
        s3.Items.Add(Line("ሀ.", "በዘር ደረጃ", (decimal)growth.Sum(x => x.AtSeedStage), 1));
        s3.Items.Add(Line("ለ.", "በብቅለት ደረጃ", (decimal)growth.Sum(x => x.AtGerminationStage), 1));
        s3.Items.Add(Line("ሐ.", "በእድገት ደረጃ", (decimal)growth.Sum(x => x.AtGrowthStage), 1));
        s3.Items.Add(Line("መ.", "ፍሬ በመሙላት ደረጃ", (decimal)growth.Sum(x => x.AtFruitStage), 1));
        s3.Items.Add(Line("ሠ.", "በመሰብሰብ ደረጃ", (decimal)growth.Sum(x => x.AtHarvestingStage), 1));
        s3.Items.Add(Heading("3.2", "የሰብል ተባይ ክስተት"));
        s3.Items.Add(CountWoredas("የለም", pests.Where(x => !x.HasPestAndDeseasOccured).Select(x => x.WoredaId)));
        s3.Items.Add(CountWoredas("አለ ከተባለ", pests.Where(x => x.HasPestAndDeseasOccured).Select(x => x.WoredaId)));
        var pestDetails = pests.SelectMany(p => p.ImpactDetails.Select(d => (p.WoredaId, Detail: d))).ToList();
        s3.Items.Add(Table(["የተባዩ/በሽታው አይነት", occurredIn, "የተጎዳ መሬት (ሄ/ር)"],
            pestDetails.GroupBy(x => x.Detail.PestDisease?.Name ?? "-").OrderBy(g => g.Key)
                .Select(g => new List<string> { g.Key, Places(g.Select(x => x.WoredaId)), Num(g.Sum(x => x.Detail.AffectedLandInHectares)) })));
        var cropPeople = new FormReportPeople();
        foreach (var p in pests)
        {
            if (p.ImpactDetails.Count > 0)
                foreach (var d in p.ImpactDetails)
                    cropPeople.Add(d.MaleHouseholdHeads, d.FemaleHouseholdHeads, d.MaleFamilyMembers, d.FemaleFamilyMembers, d.MaleChildren, d.FemaleChildren, d.MaleYouth, d.FemaleYouth, d.MaleElderly, d.FemaleElderly, d.MaleDisabled, d.FemaleDisabled);
            else
                cropPeople.Add(p.MaleHouseHold, p.FemaleHouseHold, p.MaleFamily, p.FemaleFamily, p.ChildhMale, p.ChildFemale, p.YouthMale, p.YouthFemale, p.ElderlyMale, p.ElderlyFemale, p.DisabledMale, p.DisabledFemale);
        }
        s3.Items.Add(People("በሰብል ተባይ የተጎዳ ማህበረሰብ", cropPeople));
        // Damaged land is entered per pest; older reports only have the report-level figure.
        s3.Items.Add(Line("", "በሰብል ተባይ የተጎዳ መሬት ሄ/ር", pests.Sum(x => x.ImpactDetails.Count > 0 ? x.ImpactDetails.Sum(d => d.AffectedLandInHectares) : (decimal)(x.AffectedLandInHectar ?? 0)), 1));
        s3.Items.Add(Texts("የሰብል አይነት", pests.Select(x => (x.WoredaId, x.TypeOfCropAffected))));
        s3.Items.Add(Texts("ማስታወሻ", pests.Select(x => (x.WoredaId, x.Remarks)), 0));

        // ------------------------------------------------------------------ 4. Livestock
        var pasture = (await db.PastureStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        var water = (await db.AnimalWaterSupplyStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        var health = (await db.AnimalHealthStatuses.AsNoTracking()
                .Include(x => x.ImpactDetails).ThenInclude(x => x.Disease)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.LasModifiedAt);
        var s4 = Section(sections, "4", "የእንስሳት ግጦሽና ውሀ አቅርቦት ሁኔታ");
        s4.Items.Add(Heading("4.1", "የእንስሳት ግጦሽ ሁኔታ"));
        s4.Items.Add(CountWoredas("በቂ ነው", pasture.Where(x => x.Enough).Select(x => x.WoredaId)));
        s4.Items.Add(CountWoredas("ዝቅተኛ", pasture.Where(x => !x.Enough).Select(x => x.WoredaId)));
        s4.Items.Add(Line("", "ዝቅተኛ ከሆነ የተጎዳ የእንስሳት ቁጥር", pasture.Sum(x => x.NumberOfAnimalsAffected), 1));
        Affected(s4.Items, "", "በሳምንቱ የግጦሽ ችግር ያለባቸው ቀበሌዎች በቁጥር", AffectedLocations.Events.PastureShortage, water.Select(x => (x.WoredaId, x.NoOfKebeliesWithPastureShortage)), 1);
        s4.Items.Add(People("በእንስሳት የመኖ እጥረት የተጎዳ ማህበረሰብ", Sum(pasture, x => (x.MaleHouseHold, x.FemaleHouseHold, x.MaleFamily, x.FemaleFamily, x.ChildhMale, x.ChildFemale, x.YouthMale, x.YouthFemale, x.ElderlyMale, x.ElderlyFemale, x.DisabledMale, x.DisabledFemale))));
        s4.Items.Add(Texts("ማስታወሻ", pasture.Select(x => (x.WoredaId, x.Remarks))));

        s4.Items.Add(Heading("4.2", "የእንስሳት ውሃ አቅርቦት ሁኔታ"));
        s4.Items.Add(CountWoredas("በቂ ነው", water.Where(x => x.Enough).Select(x => x.WoredaId)));
        s4.Items.Add(CountWoredas("ዝቅተኛ", water.Where(x => !x.Enough).Select(x => x.WoredaId)));
        Affected(s4.Items, "", "በሳምንቱ የውሃ ችግር ያለባቸው ቀበሌዎች በቁጥር", AffectedLocations.Events.AnimalWaterShortage, water.Select(x => (x.WoredaId, x.NoOfKebeliesWithWaterSupply)), 1);
        s4.Items.Add(People("በውሃ እጥረት የተጎዳ ማህበረሰብ", Sum(water, x => (x.MaleHouseHold, x.FemaleHouseHold, x.MaleFamily, x.FemaleFamily, x.ChildhMale, x.ChildFemale, x.YouthMale, x.YouthFemale, x.ElderlyMale, x.ElderlyFemale, x.DisabledMale, x.DisabledFemale))));
        s4.Items.Add(Texts("ማስታወሻ", water.Select(x => (x.WoredaId, x.Remarks))));

        s4.Items.Add(Heading("4.2", "የእንስሳት አቋም"));
        s4.Items.Add(CountWoredas("መካከለኛ", water.Where(x => x.AnimalBodyCondition == AnimalBodyCondition.መካከለኛ).Select(x => x.WoredaId)));
        s4.Items.Add(CountWoredas("ዝቅተኛ", water.Where(x => x.AnimalBodyCondition == AnimalBodyCondition.ዝቅተኛ).Select(x => x.WoredaId)));
        s4.Items.Add(Line("", "አቋማቸው ዝቅተኛ የሆነ የተጎዱ እንስሳት ብዛት", water.Sum(x => x.LowBodyConditionAnimalsAffected), 1));
        s4.Items.Add(Texts("ዝቅተኛ ከሆነ ምክንያት", water.Select(x => (x.WoredaId, x.LowBodyConditionReason))));

        // Animal health: Enough == false means a disease outbreak was reported (same convention as the wizard).
        s4.Items.Add(Heading("4.3", "የእንስሳት ጤና በወረርሽኝ መልክ የተከሰተ በሽታ"));
        s4.Items.Add(CountWoredas("አለ", health.Where(x => !x.Enough).Select(x => x.WoredaId)));
        s4.Items.Add(CountWoredas("የለም", health.Where(x => x.Enough).Select(x => x.WoredaId)));
        s4.Items.Add(Line("", "አለ ከተባለ የተጎዳ እንስሳት ቁጥር", health.Sum(x => x.NoOfKebeliesWithWaterSupply), 1));
        var healthDetails = health.SelectMany(h => h.ImpactDetails.Select(d => (h.WoredaId, Detail: d))).ToList();
        s4.Items.Add(Table(["የበሽታው አይነት", occurredIn, "የተጎዱ እንስሳት"],
            healthDetails.GroupBy(x => x.Detail.Disease?.Name ?? "-").OrderBy(g => g.Key)
                .Select(g => new List<string> { g.Key, Places(g.Select(x => x.WoredaId)), Num(g.Sum(x => x.Detail.NumberOfAffectedAnimals)) })));
        s4.Items.Add(Texts("የእንስሣት አይነት", health.Select(x => (x.WoredaId, x.DiseaseType))));
        var healthPeople = new FormReportPeople();
        foreach (var h in health)
        {
            if (h.ImpactDetails.Count > 0)
                foreach (var d in h.ImpactDetails)
                    healthPeople.Add(d.MaleHouseholdHeads, d.FemaleHouseholdHeads, d.MaleFamilyMembers, d.FemaleFamilyMembers, d.MaleChildren, d.FemaleChildren, d.MaleYouth, d.FemaleYouth, d.MaleElderly, d.FemaleElderly, d.MaleDisabled, d.FemaleDisabled);
            else
                healthPeople.Add(h.MaleHouseHold, h.FemaleHouseHold, h.MaleFamily, h.FemaleFamily, h.ChildhMale, h.ChildFemale, h.YouthMale, h.YouthFemale, h.ElderlyMale, h.ElderlyFemale, h.DisabledMale, h.DisabledFemale);
        }
        s4.Items.Add(People("በችግሩ የተጎዳ ማህበረሰብ", healthPeople));
        s4.Items.Add(Texts("ማስታወሻ", health.Select(x => (x.WoredaId, x.Remarks))));

        // ------------------------------------------------------------------ 5. Human drinking water
        var humanWater = (await db.HumanDrinkWaterIssues.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var s5 = Section(sections, "5", "የሰው መጠጥ ውሃ ሁኔታ");
        s5.Items.Add(CountWoredas("ችግር አለ", humanWater.Where(x => x.HasWaterProblem).Select(x => x.WoredaId), 0));
        s5.Items.Add(CountWoredas("ችግር የለም", humanWater.Where(x => !x.HasWaterProblem).Select(x => x.WoredaId), 0));
        Affected(s5.Items, "", "ችግር ያለባቸው ቀበሌዎች በቁጥር", AffectedLocations.Events.HumanWaterShortage, humanWater.Select(x => (x.WoredaId, x.NumberOfAffectedKebeles)));
        s5.Items.Add(People("በመጠጥ ውሃ የተጎዳ ማህበረሰብ", Sum(humanWater, x => (x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren, x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled)), 0));
        s5.Items.Add(Texts("ማስታወሻ", humanWater.Select(x => (x.WoredaId, x.AdditionalNotes)), 0));

        // ------------------------------------------------------------------ 6. Human epidemic
        var epidemics = (await db.EpidemicDiseases.AsNoTracking()
                .Include(x => x.EpidemicDiseaseSelections!).ThenInclude(x => x.EpidemicDiseaseType)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var s6 = Section(sections, "6", "የሰው ጤና በሳምንቱ በወረርሽኝ መልኩ የተከሰተ በሽታ");
        s6.Items.Add(CountWoredas("አለ", epidemics.Where(x => x.HasEpidemicDisease).Select(x => x.WoredaId), 0));
        s6.Items.Add(CountWoredas("የለም", epidemics.Where(x => !x.HasEpidemicDisease).Select(x => x.WoredaId), 0));
        var epidemicTypes = epidemics.SelectMany(e => (e.EpidemicDiseaseSelections ?? []).Select(s => (e.WoredaId, Name: s.EpidemicDiseaseType?.Name ?? "-"))).ToList();
        s6.Items.Add(Table(["ካለ በዝርዝር", occurredIn],
            epidemicTypes.GroupBy(x => x.Name).OrderBy(g => g.Key)
                .Select(g => new List<string> { g.Key, Places(g.Select(x => x.WoredaId)) })));
        Affected(s6.Items, "", "በወረርሽኝ መልኩ በተከሰተ በሽታ የተጎዱ ቀበሌዎች ብዛት በቁጥር", AffectedLocations.Events.Epidemic, epidemics.Select(x => (x.WoredaId, x.NumberOfAffectedKebeles)));
        var epidemicPeople = Sum(epidemics, x => (x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren, x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled));
        epidemicPeople.LactatingAndPregnant = epidemics.Sum(x => x.LactatingMothers + x.PregnantWomen);
        s6.Items.Add(People("በወረርሽኝ መልኩ በተከሰተ በሽታ የተጎዳ ማህበረሰብ", epidemicPeople, 0));
        s6.Items.Add(Texts("ማስታወሻ", epidemics.Select(x => (x.WoredaId, x.Notes)), 0));

        // ------------------------------------------------------------------ 7. Education
        var schools = (await db.SchoolReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), null);
        var s7 = Section(sections, "7", "የትምህርት ሁኔታ");
        s7.Items.Add(CountWoredas("በሳምንቱ ያጋጠመ ችግር አለ", schools.Where(x => x.HasTeachingLearningProblem).Select(x => x.WoredaId), 0));
        s7.Items.Add(CountWoredas("በሳምንቱ ያጋጠመ ችግር የለም", schools.Where(x => !x.HasTeachingLearningProblem).Select(x => x.WoredaId), 0));
        s7.Items.Add(Texts("ካለ የችግሩ አይነት", schools.Select(x => (x.WoredaId, x.Cause)), 0));
        s7.Items.Add(Line("", "የተዘጉ ትምህርት ቤቶች ብዛት", schools.Sum(x => x.ClosedSchools)));
        var maleDropouts = schools.Sum(x => x.MaleStudentDropouts);
        var femaleDropouts = schools.Sum(x => x.FemaleStudentDropouts);
        s7.Items.Add(LineText("", "ያቋረጡ ተማሪዎች", $"ወ {Num(maleDropouts)} — ሴ {Num(femaleDropouts)} — ድ {Num(maleDropouts + femaleDropouts)}"));
        var rates = schools.Where(x => x.DropoutRatePercent.HasValue).Select(x => x.DropoutRatePercent!.Value).ToList();
        s7.Items.Add(LineText("", "የማቋረጥ ጥምርታ/ምጣኔ በ% (የወረዳዎች አማካይ)", rates.Count == 0 ? "-" : Num(rates.Average()) + "%"));
        s7.Items.Add(Texts("የተወሰደ መፍትሄ", schools.Select(x => (x.WoredaId, (string?)string.Join(" | ",
            new[] { x.MajorSolutionOne, x.MajorSolutionTwo, x.MajorSolutionThree, x.OtherSolutionDescription }.Where(v => !string.IsNullOrWhiteSpace(v))))), 0));

        // ------------------------------------------------------------------ 8. Market
        var grainSupply = (await db.GrainSupplys.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var grainPrice = (await db.GrainPrices.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var grainRows = (await db.GrainPricePerQuintals.AsNoTracking().Include(x => x.GrainType).Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate), x.GrainTypeId), x => x.ReportDate);
        var animalSupply = (await db.AnimalSupplys.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var animalPrice = (await db.AnimalPrice.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var animalRows = (await db.AnimalPricePerUnits.AsNoTracking().Include(x => x.AnimalType).Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate), x.AnimalTypeId), x => x.ReportDate);
        var s8 = Section(sections, "8", "የገበያ ሁኔታ");
        s8.Items.Add(Heading("8.1", "የእህል አቅርቦት ከባለፈው ሳምንት ጋር ሲነጻጸር"));
        foreach (var status in Enum.GetValues<GrainSupplyStatus>())
            s8.Items.Add(CountWoredas(status.GetDisplayName(), grainSupply.Where(x => x.GrainSupplyStatus == status).Select(x => x.WoredaId)));
        s8.Items.Add(Texts("ምክንያት", grainSupply.Select(x => (x.WoredaId, x.AdditionalNotes))));
        s8.Items.Add(Heading("8.2", "የእህል ዋጋ ከባለፈው ሳምንት ጋር ሲነጻጸር"));
        foreach (var status in Enum.GetValues<GrainPriceStatus>())
            s8.Items.Add(CountWoredas(status.GetDisplayName(), grainPrice.Where(x => x.GrainSupplyStatus == status).Select(x => x.WoredaId)));
        s8.Items.Add(Texts("ምክንያት", grainPrice.Select(x => (x.WoredaId, x.AdditionalNotes))));
        s8.Items.Add(PriceTable("የእህል አይነት", grainRows.Select(x => (x.GrainType?.GrainName ?? "-", PreviousPrice(x.WeeklyPrice, x.PriceDifference), x.WeeklyPrice))));
        s8.Items.Add(Heading("8.3", "የእንስሳት አቅርቦት ከባለፈው ሳምንት ጋር ሲነጻጸር"));
        foreach (var status in Enum.GetValues<AnimalSupplyStatus>())
            s8.Items.Add(CountWoredas(status.GetDisplayName(), animalSupply.Where(x => x.GrainSupplyStatus == status).Select(x => x.WoredaId)));
        s8.Items.Add(Texts("ምክንያት", animalSupply.Select(x => (x.WoredaId, x.AdditionalNotes))));
        s8.Items.Add(Heading("8.4", "የእንስሳት ዋጋ ከባለፈው ሳምንት ጋር ሲነጻጸር"));
        foreach (var status in Enum.GetValues<AnimalPriceStatus>())
            s8.Items.Add(CountWoredas(status.GetDisplayName(), animalPrice.Where(x => x.GrainSupplyStatus == status).Select(x => x.WoredaId)));
        s8.Items.Add(Texts("ምክንያት", animalPrice.Select(x => (x.WoredaId, x.AdditionalNotes))));
        s8.Items.Add(PriceTable("የእንስሳት አይነት", animalRows.Select(x => (x.AnimalType?.AnimalTypeName ?? "-", PreviousPrice(x.WeeklyPrice, x.PriceDifference), x.WeeklyPrice))));

        // ------------------------------------------------------------------ 9. Hazards, migration, deaths
        var accidents = (await db.WeeklyAccidents.AsNoTracking()
                .Include(x => x.AccidentDetails!).ThenInclude(x => x.AccidentType)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var s9 = Section(sections, "9", "በሳምንቱ የታዩ ወቅታዊ አደጋዎች");
        s9.Items.Add(CountWoredas("አለ", accidents.Where(x => x.HasAccident).Select(x => x.WoredaId), 0));
        s9.Items.Add(CountWoredas("የለም", accidents.Where(x => !x.HasAccident).Select(x => x.WoredaId), 0));
        var accidentDetails = accidents.SelectMany(a => (a.AccidentDetails ?? []).Select(d => (a.WoredaId, Detail: d))).ToList();
        s9.Items.Add(Table(["የአደጋው አይነት", occurredIn, "የተጎዳ መሬት በሄ/ር", "የጉዳት መጠን በ% (አማካይ)"],
            accidentDetails.GroupBy(x => x.Detail.AccidentType?.AccidentName ?? "-").OrderBy(g => g.Key)
                .Select(g =>
                {
                    var damage = g.Where(x => x.Detail.DamageRateInPercent.HasValue).Select(x => x.Detail.DamageRateInPercent!.Value).ToList();
                    return new List<string> { g.Key, Places(g.Select(x => x.WoredaId)), Num(g.Sum(x => x.Detail.DamagedLandInHectares ?? 0)), damage.Count == 0 ? "-" : Num(damage.Average()) };
                })));
        var accidentPeople = new FormReportPeople { HasFamilyRow = false };
        foreach (var (_, d) in accidentDetails)
            accidentPeople.Add(d.AffectedHouseholdsMale, d.AffectedHouseholdsFemale, 0, 0, d.AffectedChildrenMale, d.AffectedChildrenFemale, d.AffectedYouthMale, d.AffectedYouthFemale, d.AffectedElderlyMale, d.AffectedElderlyFemale, d.AffectedDisabledMale, d.AffectedDisabledFemale);
        s9.Items.Add(People("በእያንዳንዱ ጉዳት የተጎዳ ማህበረሰብ", accidentPeople, 0));
        s9.Items.Add(Texts("ማስታወሻ", accidents.Select(x => (x.WoredaId, x.Notes)).Concat(accidentDetails.Select(x => (x.WoredaId, x.Detail.Notes))), 0));

        var migrations = (await db.Migrations.AsNoTracking().Include(x => x.MigrationDetails)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var migrationDetails = migrations.SelectMany(m => (m.MigrationDetails ?? []).Select(d => (m.WoredaId, Detail: d))).ToList();
        var locationIds = migrationDetails.SelectMany(x => new[] { x.Detail.OriginLocationId, x.Detail.DestinationLocationId })
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var locationNames = await db.Locations.AsNoTracking().Where(x => locationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.LocationAmharicName ?? x.LocationName ?? "");
        string Place(Guid? id, Guid woredaId) => id.HasValue ? locationNames.GetValueOrDefault(id.Value, "-") : woredaNames.GetValueOrDefault(woredaId, "-");
        s9.Items.Add(Heading("", "በተለያዩ ምክንያት ስደት"));
        s9.Items.Add(CountWoredas("አለ", migrations.Where(x => x.HasMigration).Select(x => x.WoredaId)));
        s9.Items.Add(CountWoredas("የለም", migrations.Where(x => !x.HasMigration).Select(x => x.WoredaId)));
        s9.Items.Add(Texts("ካለ ከየት ቦታ ተሰደዱ / ወደ የት ሄዱ", migrationDetails.Select(x => (x.WoredaId,
            (string?)$"{Place(x.Detail.OriginLocationId, x.WoredaId)} → {Place(x.Detail.DestinationLocationId, x.WoredaId)}" +
            (string.IsNullOrWhiteSpace(x.Detail.MigrationReason) ? "" : $" ({x.Detail.MigrationReason!.Trim()})")))));
        s9.Items.Add(People("የተሰደዱ", Sum(migrationDetails.Select(x => x.Detail), x => (x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren, x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled))));
        s9.Items.Add(Texts("ማስታወሻ", migrations.Select(x => (x.WoredaId, x.GeneralNotes))));

        var submissionIds = reports.Select(x => x.Id).ToList();
        var animalMoves = await db.AnimalMigrationRecords.AsNoTracking()
            .Include(x => x.OriginLocation).Include(x => x.DestinationLocation)
            .Where(x => submissionIds.Contains(x.WeeklyReportSubmissionId)).ToListAsync();
        string AnimalName(AnimalMigrationRecord x) => x.AnimalType == MigratingAnimalType.Other ? x.OtherAnimalType ?? "ሌላ" : x.AnimalType.ToString().Replace('_', ' ');
        s9.Items.Add(Heading("", "የእንስሳት ስደት"));
        s9.Items.Add(Table(["የእንስሳት አይነት", "ከወረዳው የወጡ", "ወደ ወረዳው የገቡ"],
            animalMoves.GroupBy(AnimalName).OrderBy(g => g.Key).Select(g => new List<string>
            {
                g.Key,
                Num(g.Where(x => x.Direction == AnimalMigrationDirection.Outward).Sum(x => (long)x.NumberOfAnimals)),
                Num(g.Where(x => x.Direction == AnimalMigrationDirection.Inward).Sum(x => (long)x.NumberOfAnimals))
            })));
        s9.Items.Add(Texts("የስደት መስመር", animalMoves.Select(x => (x.WoredaId,
            (string?)$"{AnimalName(x)} {Num(x.NumberOfAnimals)}፡ {Name(x.OriginLocation)} → {Name(x.DestinationLocation)}"))));

        var deaths = (await db.Deaths.AsNoTracking().Include(x => x.DeathDetails)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var deathDetails = deaths.SelectMany(d => (d.DeathDetails ?? []).Select(x => (d.WoredaId, Detail: x))).ToList();
        s9.Items.Add(Heading("", "የሞተ ካለ"));
        s9.Items.Add(CountWoredas("አለ", deaths.Where(x => x.HasDeath).Select(x => x.WoredaId)));
        s9.Items.Add(CountWoredas("የለም", deaths.Where(x => !x.HasDeath).Select(x => x.WoredaId)));
        s9.Items.Add(Texts("የሞቱ ምክንያት", deathDetails.Select(x => (x.WoredaId, x.Detail.DeathReason))));
        s9.Items.Add(People("የሞተ", Sum(deathDetails.Select(x => x.Detail), x => (x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren, x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled))));

        // ------------------------------------------------------------------ 10. Aid
        var recipients = (await db.AssistanceRecipients.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var provided = (await db.WeeklyProvideds.AsNoTracking()
                .Include(x => x.WeeklyProvidedDetail!).ThenInclude(x => x.SupplyType)
                .Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var s10 = Section(sections, "10", "የእርዳታ አቅርቦትና ስርጭት");
        s10.Items.Add(People("የዕለት እርዳታ ተጠቃሚ", Sum(recipients, x => (x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren, x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled)), 0));
        s10.Items.Add(Texts("ማስታወሻ", recipients.Select(x => (x.WoredaId, x.Notes)), 0));
        s10.Items.Add(Heading("10.1", "በሳምንቱ የቀረበ"));
        var aidDetails = provided.SelectMany(p => p.WeeklyProvidedDetail ?? []).ToList();
        s10.Items.Add(Table(["የእህል አይነት", "መለኪያ", "የቀረበ", "የተሰራጨ", "ከቀረበው የተሰራጨው በፐርሰንት", "እርዳታውን ያቀረበው አካል"],
            aidDetails.GroupBy(x => (Name: x.SupplyType?.SupplyTypeName ?? "-", x.Measurement)).OrderBy(g => g.Key.Name)
                .Select(g =>
                {
                    var given = g.Sum(x => x.ProvidedQuantity);
                    var distributed = g.Sum(x => x.DistributedQuantity);
                    return new List<string>
                    {
                        g.Key.Name, g.Key.Measurement.ToString(), Num(given), Num(distributed),
                        given == 0 ? "-" : Num(distributed / given * 100) + "%",
                        string.Join(", ", g.Select(x => x.Donor).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()).Distinct())
                    };
                })));
        s10.Items.Add(Heading("", "ችግር ካለ"));
        foreach (var problem in Enum.GetValues<Problem>().Where(x => x != Problem.No))
            s10.Items.Add(CountWoredas(problem.GetDisplayName(), provided.Where(x => x.IfProblem == problem).Select(x => x.WoredaId)));
        s10.Items.Add(Texts("ማስታወሻ", provided.Select(x => (x.WoredaId, x.Notes))));

        // ------------------------------------------------------------------ 11. Other problems
        var other = (await db.OtherProblems.AsNoTracking().Where(x => ids.Contains(x.WoredaId)).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, Day(x.StartDate), Day(x.EndDate)), x => x.ReportDate);
        var s11 = Section(sections, "11", "ሌላ ያልተገለፀ በሳምንቱ ያጋጠመ ችግር ካለ");
        s11.Items.Add(CountWoredas("ችግር የተመዘገበባቸው", other.Where(x => x.HasOtherProblem).Select(x => x.WoredaId), 0));
        s11.Items.Add(Texts("መግለጫ", other.Where(x => x.HasOtherProblem).Select(x => (x.WoredaId, (string?)x.ProblemName)), 0));
        s11.Items.Add(People("በጉዳቱ የተጎዳ", Sum(other, x => (x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren, x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled)), 0));
        s11.Items.Add(Texts("ማስታወሻ", other.Select(x => (x.WoredaId, x.GeneralNotes)), 0));

        return sections;
    }

    private static DateTime? Day(DateTime? value) => value?.Date;

    private static string Name(Locations? location) => location?.LocationAmharicName ?? location?.LocationName ?? "";

    private static string Num(decimal value) => value.ToString(value % 1 == 0 ? "#,0" : "#,0.##", CultureInfo.InvariantCulture);
    private static string Num(double value) => Num((decimal)value);
    private static string Num(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static FormReportSection Section(List<FormReportSection> sections, string number, string title)
    {
        var section = new FormReportSection { Number = number, Title = title };
        sections.Add(section);
        return section;
    }

    private static FormReportItem Heading(string number, string label) => new() { Kind = FormReportItemKind.Heading, Number = number, Label = label };
    private static FormReportItem Line(string number, string label, decimal value, int indent = 0) => LineText(number, label, Num(value), indent);
    private static FormReportItem LineText(string number, string label, string value, int indent = 0) =>
        new() { Kind = FormReportItemKind.Line, Number = number, Label = label, Value = value, Indent = indent };
    private static FormReportItem CountLine(string label, int woredas, int indent = 1) =>
        LineText("•", label, $"{Num(woredas)} {Woredas}", indent);
    private static FormReportItem People(string label, FormReportPeople people, int indent = 1) =>
        new() { Kind = FormReportItemKind.People, Label = label, People = people, Indent = indent };
    private static FormReportItem Table(List<string> headers, IEnumerable<List<string>> rows) =>
        new() { Kind = FormReportItemKind.Table, Indent = 1, TableHeaders = headers, TableRows = rows.ToList() };

    // Rows are one per woreda and item, so every average is over woreda observations (never an average of averages).
    // A missing or non-positive price is "no data", not a price of 0.
    private static FormReportItem PriceTable(string typeHeader, IEnumerable<(string Name, decimal? Previous, decimal Current)> rows) =>
        Table([typeHeader, "ያለፈው ሳምንት (አማካይ)", "በዚህ ሳምንት (አማካይ)", "በንፅፅር በመጨመር/በመቀነስ", "ዋጋ የላኩ ወረዳዎች"],
            rows.GroupBy(x => x.Name).OrderBy(g => g.Key).Select(g =>
            {
                var previous = g.Where(x => x.Previous > 0).Select(x => x.Previous!.Value).ToList();
                var current = g.Where(x => x.Current > 0).Select(x => x.Current).ToList();
                // The change compares like with like: only woredas with a valid price in both weeks.
                var paired = g.Where(x => x.Previous > 0 && x.Current > 0).ToList();
                var change = paired.Count == 0 ? "-"
                    : FormatChange(paired.Average(x => x.Current) - paired.Average(x => x.Previous!.Value), paired.Average(x => x.Previous!.Value));
                return new List<string>
                {
                    g.Key, previous.Count == 0 ? "-" : Num(previous.Average()), current.Count == 0 ? "-" : Num(current.Average()), change, Num(current.Count)
                };
            }));

    /// <summary>
    /// PreviousWeekPrice is not stored; the saved difference (this week − the immediately preceding week, null when
    /// there was no such week) gives it back.
    /// </summary>
    internal static decimal? PreviousPrice(decimal current, decimal? difference) => difference.HasValue ? current - difference.Value : null;

    private static string FormatChange(decimal difference, decimal previous)
    {
        var sign = difference > 0 ? "+" : "";
        var percent = previous == 0 ? "" : $" ({sign}{Num(Math.Round(difference / previous * 100, 2))}%)";
        return $"{sign}{Num(Math.Round(difference, 2))}{percent}";
    }

    private static FormReportPeople Sum<T>(IEnumerable<T> rows, Func<T, (int, int, int, int, int, int, int, int, int, int, int, int)> select)
    {
        var people = new FormReportPeople();
        foreach (var row in rows)
        {
            var p = select(row);
            people.Add(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, p.Item8, p.Item9, p.Item10, p.Item11, p.Item12);
        }
        return people;
    }
}

internal static class FormReportDedupe
{
    /// <summary>
    /// Keeps one row per key (woreda + week [+ type]) so a woreda that saved the same week twice is not counted twice.
    /// The most recent row by <paramref name="stamp"/> wins; without a stamp the last stored row wins.
    /// </summary>
    public static List<T> LatestPer<T, TKey>(this IEnumerable<T> rows, Func<T, TKey> key, Func<T, DateTime>? stamp)
        where TKey : notnull =>
        rows.GroupBy(key)
            .Select(g => stamp == null ? g.Last() : g.OrderByDescending(stamp).First())
            .ToList();
}
