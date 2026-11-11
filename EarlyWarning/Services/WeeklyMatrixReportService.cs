using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EarlyWarning.Services;

public interface IWeeklyMatrixReportService
{
    /// <summary>
    /// Builds the weekly report as a topic × zone grid (region users) or topic × woreda grid (zone and woreda
    /// users), in the layout of the regional Excel summary. Only the given submissions are read, so the caller's
    /// scope and status rules (see <see cref="IReportScopeService"/>) decide what is included.
    /// </summary>
    /// <param name="woredaColumns">One column per woreda even for a region (used to write one narrative per zone).</param>
    Task<WeeklyMatrixReport> BuildAsync(ReportScope scope, IReadOnlyCollection<WeeklyReportSubmission> reports, DateTime startDate, DateTime endDate,
        bool woredaColumns = false);
}

public sealed class WeeklyMatrixReportService(EarlyWarningDbContext db) : IWeeklyMatrixReportService
{
    internal const string None = "የለም";
    internal const string NoProblem = "ችግር የለም";
    internal const string NotReported = "ሪፖርት አልተላከም";
    internal const string NoData = "መረጃ የለም";
    private const string ListSeparator = "፣ ";
    private const string PartSeparator = "፤ ";

    // Row order of the paper report; types not listed follow alphabetically.
    private static readonly string[] GrainOrder = ["ጤፍ", "ገብስ", "ማሽላ", "ስንዴ", "በቆሎ", "ባቄላ", "አተር", "ሽንብራ", "ሰሊጥ", "ምስር"];
    private static readonly string[] AnimalOrder = ["በሬ", "ላም", "በግ", "ፍየል", "ዶሮ", "ደሮ"];

    private sealed record Unit(string Name, HashSet<Guid> Woredas, HashSet<Guid> Reported, bool IsTotal = false, string Group = "")
    {
        public bool Contains(Guid woredaId) => Woredas.Contains(woredaId);
    }

    public async Task<WeeklyMatrixReport> BuildAsync(ReportScope scope, IReadOnlyCollection<WeeklyReportSubmission> reports, DateTime startDate, DateTime endDate,
        bool woredaColumns = false)
    {
        var places = await AffectedLocations.LoadPlacesAsync(db, scope.WoredaIds);
        string WoredaName(Guid id) => places.TryGetValue(id, out var p) && !string.IsNullOrWhiteSpace(p.Woreda) ? p.Woreda : "-";

        var reported = reports.Select(x => x.WoredaId).ToHashSet();
        var units = BuildUnits(scope, places, reported, woredaColumns);
        if (units.Count > 1)
            units.Add(new Unit("ጠቅላላ ድምር", units.SelectMany(x => x.Woredas).ToHashSet(), reported, IsTotal: true));

        var report = new WeeklyMatrixReport
        {
            StartDate = startDate,
            EndDate = endDate,
            ScopeName = scope.LocationName,
            ScopeLevel = scope.Level,
            UnitWord = scope.Level == ReportScopeLevel.Region && !woredaColumns ? "ዞን" : "ወረዳ",
            WoredasInScope = scope.WoredaIds.Count,
            WoredasReported = scope.WoredaIds.Count(reported.Contains),
            Title = $"የቅድሚያ ማስጠንቀቂያና ምላሽ ሳምንታዊ ሪፖርት ከ{EthiopianDate.FromGregorian(startDate)} - {EthiopianDate.FromGregorian(endDate)} ዓ/ም"
        };
        report.Units.AddRange(units.Select(x => new MatrixUnit(x.Name, x.IsTotal, x.Group)));

        // ------------------------------------------------------------------ data for the week (latest row per woreda)
        var ids = reported.ToList();
        // Read only the reported weeks from the database; the exact (woreda, week) match below then picks each
        // woreda's own week. Without this every saved week of every woreda in scope was loaded on each request.
        var from = reports.Count == 0 ? DateTime.MinValue : reports.Min(x => x.StartDate.Date);
        var to = reports.Count == 0 ? DateTime.MinValue : reports.Max(x => x.EndDate.Date).AddDays(1);
        var keys = reports.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
        bool InWeek(Guid id, DateTime? start, DateTime? end) =>
            start.HasValue && end.HasValue && keys.Contains((id, start.Value.Date, end.Value.Date));

        var rain = (await db.RainfallReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var farming = (await db.FarmingActivities.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var growth = (await db.CropGrowths.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var pests = (await db.CropPestAndDeseaseReports.AsNoTracking()
                .Include(x => x.ImpactDetails).ThenInclude(x => x.PestDisease)
                .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var pasture = (await db.PastureStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var water = (await db.AnimalWaterSupplyStatuses.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var health = (await db.AnimalHealthStatuses.AsNoTracking()
                .Include(x => x.ImpactDetails).ThenInclude(x => x.Disease)
                .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.LasModifiedAt);
        var humanWater = (await db.HumanDrinkWaterIssues.AsNoTracking().Include(x => x.HumanDrinkWaterKebeles).Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var epidemics = (await db.EpidemicDiseases.AsNoTracking()
                .Include(x => x.EpidemicDiseaseSelections!).ThenInclude(x => x.EpidemicDiseaseType)
                .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var schools = (await db.SchoolReports.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, null);
        var grainSupply = (await db.GrainSupplys.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var animalSupply = (await db.AnimalSupplys.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var grainRows = (await db.GrainPricePerQuintals.AsNoTracking().Include(x => x.GrainType).Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, x.GrainTypeId), x => x.ReportDate);
        var animalRows = (await db.AnimalPricePerUnits.AsNoTracking().Include(x => x.AnimalType).Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => (x.WoredaId, x.AnimalTypeId), x => x.ReportDate);
        var accidents = (await db.WeeklyAccidents.AsNoTracking()
                .Include(x => x.AccidentDetails!).ThenInclude(x => x.AccidentType)
                .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var migrations = (await db.Migrations.AsNoTracking().Include(x => x.MigrationDetails)
                .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var deaths = (await db.Deaths.AsNoTracking().Include(x => x.DeathDetails)
                .Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var provided = (await db.WeeklyProvideds.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);
        var other = (await db.OtherProblems.AsNoTracking().Where(x => ids.Contains(x.WoredaId) && x.StartDate >= from && x.StartDate < to).ToListAsync())
            .Where(x => InWeek(x.WoredaId, x.StartDate, x.EndDate)).LatestPer(x => x.WoredaId, x => x.ReportDate);

        var grainTypes = await db.GrainTypes.AsNoTracking().Where(x => x.IsActive).ToListAsync();
        var animalTypes = await db.AnimalTypes.AsNoTracking().Where(x => x.IsActive).ToListAsync();

        // ------------------------------------------------------------------ row builders
        void Header() => report.Rows.Add(new MatrixRow { Kind = MatrixRowKind.UnitHeader, Number = "ተ.ቁ", Label = "ዝርዝር ስራዎች" });
        void Section(string number, string label, string? left = null, string? right = null) => report.Rows.Add(new MatrixRow
        {
            Kind = MatrixRowKind.Section, Number = number, Label = label,
            Cells = left == null ? [] : units.Select(_ => new MatrixCell(left, RightText: right)).ToList()
        });
        // Text rows: a unit none of whose woredas reported says so, and one with no rows for the topic
        // (dataFor) says "no data" rather than "no problem". The total column is empty unless asked for.
        // count: an optional number per unit (also for the total column), e.g. woredas in a rain class.
        void Text(string number, string label, Func<Unit, string> text, IEnumerable<Guid>? dataFor = null, bool total = false, Func<Unit, int>? count = null)
        {
            var withData = dataFor?.ToHashSet();
            report.Rows.Add(new MatrixRow
            {
                Kind = MatrixRowKind.Text, Number = number, Label = label,
                Cells = units.Select(u => new MatrixCell(
                    u.IsTotal && !total ? null
                    : !u.Reported.Overlaps(u.Woredas) ? NotReported
                    : withData != null && !withData.Overlaps(u.Woredas) ? NoData
                    : text(u), Count: count?.Invoke(u))).ToList()
            });
        }
        void Pair(string number, string label, Func<Unit, (decimal? Left, decimal? Right)> values, string format = "#,##0") => report.Rows.Add(new MatrixRow
        {
            Kind = MatrixRowKind.Pair, Number = number, Label = label, NumberFormat = format,
            Cells = units.Select(u => { var (l, r) = values(u); return new MatrixCell(Left: l, Right: r); }).ToList()
        });

        // "3 ወረዳ (ሀ፣ ለ፣ ሐ)"; every woreda of a multi-woreda unit → "ሁሉም ወረዳዎች (5)".
        string Woredas(Unit u, IEnumerable<Guid> woredaIds)
        {
            var list = woredaIds.Where(u.Contains).Distinct().ToList();
            if (list.Count == 0) return None;
            if (list.Count == u.Woredas.Count && list.Count > 1) return $"ሁሉም ወረዳዎች ({list.Count})";
            return $"{list.Count} ወረዳ ({string.Join(ListSeparator, list.Select(WoredaName).Order())})";
        }
        // One status per woreda → "ተመሳሳይ ነው", or "2 ወረዳ (ሀ፣ ለ) ቀንሷል፤ በሌሎች ተመሳሳይ ነው".
        string Statuses(Unit u, IEnumerable<(Guid WoredaId, string Status)> rows)
        {
            var groups = rows.Where(x => u.Contains(x.WoredaId)).GroupBy(x => x.Status)
                .OrderByDescending(g => g.Count()).ThenBy(g => g.Key).ToList();
            if (groups.Count == 0) return NoData;
            if (groups.Count == 1) return groups[0].Key;
            return string.Join(PartSeparator, groups.Skip(1).Select(g => $"{Woredas(u, g.Select(x => x.WoredaId))} {g.Key}"))
                + $"{PartSeparator}በሌሎች {groups[0].Key}";
        }
        static string Names(IEnumerable<string?> names) =>
            string.Join(ListSeparator, names.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()).Distinct().Order());

        // ------------------------------------------------------------------ where things happened
        // Kebele names exist for flood/drought (rainfall) and human drinking water; elsewhere only counts are saved.
        foreach (var r in rain) { r.DeserializeKebeles(); r.DeserializeDroughtKebeles(); }
        var nameIds = rain.SelectMany(r => r.SelectedKebeleIds.Concat(r.SelectedDroughtKebeleIds))
            .Concat(humanWater.SelectMany(h => (h.HumanDrinkWaterKebeles ?? []).Select(k => k.KebeleId)))
            .Concat(migrations.SelectMany(m => (m.MigrationDetails ?? []).SelectMany(d => new[] { d.OriginLocationId, d.DestinationLocationId }))
                .Where(x => x.HasValue).Select(x => x!.Value))
            .Distinct().ToList();
        var locationNames = await db.Locations.AsNoTracking().Where(x => nameIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.LocationAmharicName ?? x.LocationName ?? "");

        // "ሰሜን ጎንደር ዞን፣ ደባርቅ ወረዳ፣ 2 ቀበሌ (01፣ ዳባት)" — kebele names when saved, otherwise the kebele count.
        string Place(Guid woredaId, IEnumerable<Guid>? kebeleIds = null, int kebeleCount = 0)
        {
            var parts = new List<string>();
            if (places.TryGetValue(woredaId, out var p) && !string.IsNullOrWhiteSpace(p.Zone)) parts.Add($"{p.Zone} ዞን");
            parts.Add($"{WoredaName(woredaId)} ወረዳ");
            var kebeles = (kebeleIds ?? []).Select(id => locationNames.GetValueOrDefault(id)).Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim()).Distinct().Order().ToList();
            if (kebeles.Count > 0) parts.Add($"{kebeles.Count} ቀበሌ ({string.Join(ListSeparator, kebeles)})");
            else if (kebeleCount > 0) parts.Add($"{kebeleCount} ቀበሌ");
            return string.Join(ListSeparator, parts);
        }
        // One line per incident, "• <place>፡ <what>፤ <details>፤ <notes>", ordered by woreda.
        string Entries(Unit u, IEnumerable<(Guid WoredaId, string Place, string?[] Parts)> rows) => string.Join("\n",
            rows.Where(x => u.Contains(x.WoredaId)).OrderBy(x => WoredaName(x.WoredaId))
                .Select(x => $"• {x.Place}፡ {Sentence(x.Parts)}"));
        static string Sentence(params string?[] parts) =>
            string.Join(PartSeparator, parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim().TrimEnd('።', '.')));
        static string? Labeled(string label, string? text) => string.IsNullOrWhiteSpace(text) ? null : $"{label}፡ {text.Trim()}";
        static string? Count(int value, string unit) => value > 0 ? $"{value:#,0} {unit}" : null;
        // Beneficiaries = household heads + family members; the age and disability rows (e–l) describe
        // those same people and are not added (see Extensions/Beneficiaries).
        static int People(int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l) => (int)Beneficiaries.Count(a, b, c, d);
        // Remarks with no incident: "• <place>፡ <text>", or "-" when nobody wrote anything.
        string Notes(Unit u, IEnumerable<(Guid WoredaId, string? Text)> rows)
        {
            var text = Entries(u, rows.Where(x => !string.IsNullOrWhiteSpace(x.Text)).Select(x => (x.WoredaId, Place(x.WoredaId), new[] { x.Text })));
            return string.IsNullOrEmpty(text) ? "-" : text;
        }

        // ------------------------------------------------------------------ 1. Rain
        Header();
        Text("", "ሪፖርት የላኩ ወረዳዎች", u => $"{u.Woredas.Count(u.Reported.Contains)} ከ {u.Woredas.Count}", total: true,
            count: u => u.Woredas.Count(u.Reported.Contains));
        Section("1", "የዝናብ ሁኔታ");
        var rainBy = rain.ToDictionary(x => x.WoredaId);
        var rainClass = units.SelectMany(x => x.Woredas).Distinct().ToDictionary(w => w, w => ClassifyRain(rainBy.GetValueOrDefault(w)));
        Text("1.1", "ዝናብ ሙሉ በሙሉ ያገኙ ወረዳዎች", u => Woredas(u, u.Woredas.Where(w => rainClass[w] == RainClass.Full)),
            count: u => u.Woredas.Count(w => rainClass[w] == RainClass.Full));
        Text("1.2", "ዝናብ በከፊል ያገኙ ወረዳዎች", u => Woredas(u, u.Woredas.Where(w => rainClass[w] == RainClass.Partial)),
            count: u => u.Woredas.Count(w => rainClass[w] == RainClass.Partial));
        Text("1.3", "ዝናብ ያላገኙ ወረዳዎች", u => Woredas(u, u.Woredas.Where(w => rainClass[w] == RainClass.None)),
            count: u => u.Woredas.Count(w => rainClass[w] == RainClass.None));
        Text("1.4", "መረጃ ያልተገኘላቸው ወረዳዎች", u => Woredas(u, u.Woredas.Where(w => rainClass[w] == RainClass.NoData)),
            count: u => u.Woredas.Count(w => rainClass[w] == RainClass.NoData));
        Text("1.5", "የዝናቡ መጠንና ስርጭት", u => RainAmount(rain.Where(x => u.Contains(x.WoredaId)).ToList()), rain.Select(x => x.WoredaId));
        Text("1.6", "በከፍተኛ ዝናብ/ጎርፍ የተጎዱ ቀበሌዎች", u =>
        {
            var hit = rain.Where(x => x.SelectedKebeleIds.Count > 0 || !string.IsNullOrWhiteSpace(x.HighRainKebelieList)).ToList();
            var text = Entries(u, hit.Select(x => (x.WoredaId, Place(x.WoredaId, x.SelectedKebeleIds),
                new[] { "በከፍተኛ ዝናብ/ጎርፍ ተጎድተዋል", x.SelectedKebeleIds.Count == 0 ? x.HighRainKebelieList : null })));
            return string.IsNullOrEmpty(text) ? None : text;
        }, rain.Select(x => x.WoredaId));
        Text("1.7", "በድርቅ የተጎዱ ቀበሌዎች", u =>
        {
            var hit = rain.Where(x => x.SelectedDroughtKebeleIds.Count > 0).ToList();
            var text = Entries(u, hit.Select(x => (x.WoredaId, Place(x.WoredaId, x.SelectedDroughtKebeleIds), new string?[] { "በድርቅ ተጎድተዋል" })));
            return string.IsNullOrEmpty(text) ? None : text;
        }, rain.Select(x => x.WoredaId));
        Text("1.8", "ተጨማሪ ማብራሪያ", u => Notes(u, rain.Select(x => (x.WoredaId, x.Remarks))), rain.Select(x => x.WoredaId));

        // ------------------------------------------------------------------ 2. Farming (sums; left = መኸር, right = በልግ)
        Section("2", "የእርሻ ሥራ እንቅስቃሴ", "መኸር", "በልግ");
        (decimal?, decimal?) Farm(Unit u, Func<FarmingActivity, float> meher, Func<FarmingActivity, float> belg)
        {
            var rows = farming.Where(x => u.Contains(x.WoredaId)).ToList();
            return rows.Count == 0 ? (null, null) : (Round(rows.Sum(x => (decimal)meher(x))), Round(rows.Sum(x => (decimal)belg(x))));
        }
        const string hectares = "#,##0.##";
        Pair("2.1", "ዕቅድ በሄ/ር", u => Farm(u, x => x.MeherFarmPlan, x => x.AutumnFarmPlan), hectares);
        Pair("2.2", "የታረሰ በሄ/ር", u => Farm(u, x => x.MeherPloughed, x => x.AutumnPloughed), hectares);
        Pair("2.3", "የተዘራ በሄ/ር", u => Farm(u, x => x.MeherSown, x => x.AutumnSown), hectares);
        Pair("2.4", "አጨዳና ስብሰባ በሄ/ር", u => Farm(u, x => x.MeherHarvestingHHarvesting, x => x.AutumnHarvestingHHarvesting), hectares);
        Pair("2.5", "በቀሪ እርጥበት የተዘራ በሄ/ር", u => Farm(u, x => x.MeherSownWithResidualMoisture, x => x.AutumnSownWithResidualMoisture), hectares);
        Text("2.6", "ተጨማሪ ማብራሪያ", u => Notes(u, farming.Select(x => (x.WoredaId, x.Remarks))), farming.Select(x => x.WoredaId));

        // ------------------------------------------------------------------ 3. Crops
        Section("3", "የሰብል ሁኔታ");
        Text("3.1", "እድገትና አቋም", u =>
        {
            var rows = growth.Where(x => u.Contains(x.WoredaId)).ToList();
            var stages = new (string Name, decimal Hectares)[]
            {
                ("በዘር", rows.Sum(x => (decimal)x.AtSeedStage)), ("በብቅለት", rows.Sum(x => (decimal)x.AtGerminationStage)),
                ("በእድገት", rows.Sum(x => (decimal)x.AtGrowthStage)), ("ፍሬ በመሙላት", rows.Sum(x => (decimal)x.AtFruitStage)),
                ("በመሰብሰብ", rows.Sum(x => (decimal)x.AtHarvestingStage))
            }.Where(x => x.Hectares > 0).ToList();
            var summary = stages.Count == 0 ? "ሰብል የለም" : string.Join(ListSeparator, stages.Select(x => $"{x.Name} ({Num(x.Hectares)} ሄ/ር)"));
            var notes = Notes(u, rows.Select(x => (x.WoredaId, x.Remarks)));
            return notes == "-" ? summary : $"{summary}\n{notes}";
        }, growth.Select(x => x.WoredaId));
        Text("3.2", "የሰብል ተባይ ክስተት", u =>
        {
            var found = pests.Where(x => x.HasPestAndDeseasOccured).ToList();
            var text = Entries(u, found.Select(x => (x.WoredaId, Place(x.WoredaId), new[]
            {
                (Names(x.ImpactDetails.Select(d => d.PestDisease?.Name)) is { Length: > 0 } names ? names : "የሰብል ተባይ/በሽታ") + " ተከስቷል",
                Labeled("የተጎዳው ሰብል", x.TypeOfCropAffected),
                x.ImpactDetails.Sum(d => d.AffectedLandInHectares) is > 0 and var land ? $"የተጎዳ መሬት {Num(land)} ሄ/ር" : null,
                Names(x.ImpactDetails.Select(d => d.Notes)),
                x.Remarks
            })));
            return string.IsNullOrEmpty(text) ? None : text;
        }, pests.Select(x => x.WoredaId));

        // ------------------------------------------------------------------ 4. Livestock and drinking water
        Header();
        Section("4", "የእንስሳት ግጦሽና ውሀ አቅርቦት ሁኔታ");
        var waterBy = water.ToDictionary(x => x.WoredaId);
        Text("4.1", "የእንስሳት ግጦሽና ውሃ አቅርቦት", u =>
        {
            var problems = pasture.Where(x => !x.Enough)
                .Select(x => (x.WoredaId, Place(x.WoredaId, kebeleCount: waterBy.GetValueOrDefault(x.WoredaId)?.NoOfKebeliesWithPastureShortage ?? 0),
                    new[] { "የመኖ እጥረት አለ", Count(x.NumberOfAnimalsAffected, "እንስሳት ተጎድተዋል"), x.Remarks }))
                .Concat(water.Where(x => !x.Enough)
                    .Select(x => (x.WoredaId, Place(x.WoredaId, kebeleCount: x.NoOfKebeliesWithWaterSupply), new[] { "የእንስሳት ውሃ እጥረት አለ", x.Remarks })));
            var text = Entries(u, problems);
            return string.IsNullOrEmpty(text) ? NoProblem : text;
        }, pasture.Select(x => x.WoredaId).Concat(water.Select(x => x.WoredaId)));
        Text("4.2", "የእንስሳት አቋም", u =>
        {
            var summary = Statuses(u, water.Where(x => x.AnimalBodyCondition.HasValue).Select(x => (x.WoredaId, x.AnimalBodyCondition!.Value.ToString())));
            var low = Entries(u, water.Where(x => x.AnimalBodyCondition == AnimalBodyCondition.ዝቅተኛ).Select(x => (x.WoredaId, Place(x.WoredaId),
                new[] { "የእንስሳት አቋም ዝቅተኛ ነው", Count(x.LowBodyConditionAnimalsAffected, "እንስሳት"), Labeled("ምክንያት", x.LowBodyConditionReason) })));
            return string.IsNullOrEmpty(low) ? summary : $"{summary}\n{low}";
        });
        // Animal health: Enough == false means an outbreak was reported (wizard convention).
        Text("4.3", "የእንስሳት ጤና", u =>
        {
            var text = Entries(u, health.Where(x => !x.Enough).Select(x => (x.WoredaId, Place(x.WoredaId), new[]
            {
                (Names(x.ImpactDetails.Select(d => d.Disease?.Name)) is { Length: > 0 } names ? names : "በሽታ") + " በወረርሽኝ መልክ ተከስቷል",
                Labeled("የተጠቁ እንስሳት", x.DiseaseType),
                Count(x.ImpactDetails.Sum(d => d.NumberOfAffectedAnimals), "እንስሳት ተጎድተዋል"),
                Names(x.ImpactDetails.Select(d => d.Notes)),
                x.Remarks
            })));
            return string.IsNullOrEmpty(text) ? "በወረርሽኝ መልክ የተከሰተ በሽታ የለም" : text;
        }, health.Select(x => x.WoredaId));
        Text("4.4", "የሰው መጠጥ ውሃ", u =>
        {
            var text = Entries(u, humanWater.Where(x => x.HasWaterProblem).Select(x => (x.WoredaId,
                Place(x.WoredaId, (x.HumanDrinkWaterKebeles ?? []).Select(k => k.KebeleId), x.NumberOfAffectedKebeles), new[]
                {
                    "የመጠጥ ውሃ እጥረት አለ",
                    Count(People(x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren,
                        x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled), "ሰው ተጎድቷል"),
                    x.AdditionalNotes
                })));
            return string.IsNullOrEmpty(text) ? NoProblem : text;
        }, humanWater.Select(x => x.WoredaId));

        // ------------------------------------------------------------------ 5–6. People
        Section("5", "የሰው ጤና");
        Text("5.1", "በወረርሽኝ መልክ የተከሰተ በሽታ", u =>
        {
            var text = Entries(u, epidemics.Where(x => x.HasEpidemicDisease).Select(x => (x.WoredaId, Place(x.WoredaId, kebeleCount: x.NumberOfAffectedKebeles), new[]
            {
                (Names((x.EpidemicDiseaseSelections ?? []).Select(s => s.EpidemicDiseaseType?.Name)) is { Length: > 0 } names ? names : "በሽታ") + " በወረርሽኝ መልክ ተከስቷል",
                Count(People(x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren,
                    x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled), "ሰው ተጠቅቷል"),
                Count(x.PregnantWomen + x.LactatingMothers, "እርጉዝና የሚያጠቡ እናቶች"),
                x.Notes
            })));
            return string.IsNullOrEmpty(text) ? "በወረርሽኝ መልክ የተከሰተ በሽታ የለም" : text;
        }, epidemics.Select(x => x.WoredaId));
        Section("6", "የትምህርት ሁኔታ");
        Text("6.1", "በትምህርትና መማር ሂደት ላይ ያጋጠመ ችግር", u =>
        {
            var text = Entries(u, schools.Where(x => x.HasTeachingLearningProblem).Select(x => (x.WoredaId, Place(x.WoredaId), new[]
            {
                Labeled("ችግር", x.Cause) ?? "ችግር አለ",
                Count(x.MaleStudentDropouts + x.FemaleStudentDropouts, "ተማሪዎች አቋርጠዋል"),
                Count(x.ClosedSchools, "ትምህርት ቤቶች ተዘግተዋል"),
                Labeled("የተወሰደ መፍትሄ", Names([x.MajorSolutionOne, x.MajorSolutionTwo, x.MajorSolutionThree, x.OtherSolutionDescription]))
            })));
            return string.IsNullOrEmpty(text) ? NoProblem : text;
        }, schools.Select(x => x.WoredaId));

        // ------------------------------------------------------------------ 7. Market
        Section("7", "የገበያ ሁኔታ");
        Text("7.1", "የዋጋ ለውጥ ከባለፈው ሳምንት ጋር ሲነጻጸር", u =>
        {
            var grain = PriceTrend(grainRows.Where(x => u.Contains(x.WoredaId)).Select(x => (x.GrainType?.GrainName ?? "-", x.WeeklyPrice, x.PriceDifference)));
            var animal = PriceTrend(animalRows.Where(x => u.Contains(x.WoredaId)).Select(x => (x.AnimalType?.AnimalTypeName ?? "-", x.WeeklyPrice, x.PriceDifference)));
            return $"እህል፡ {grain}\nእንስሳት፡ {animal}";
        });
        // Supply: the status summary, then each woreda's stated reason.
        string Supply(Unit u, IEnumerable<(Guid WoredaId, string Status, string? Reason)> rows)
        {
            var list = rows.ToList();
            var summary = Statuses(u, list.Select(x => (x.WoredaId, x.Status)));
            var reasons = Entries(u, list.Where(x => !string.IsNullOrWhiteSpace(x.Reason)).Select(x => (x.WoredaId, Place(x.WoredaId), new[] { x.Status, x.Reason })));
            return string.IsNullOrEmpty(reasons) ? summary : $"{summary}\n{reasons}";
        }
        Text("7.2", "የእህል አቅርቦት", u => Supply(u, grainSupply.Select(x => (x.WoredaId, x.GrainSupplyStatus.GetDisplayName(), x.AdditionalNotes))));
        Text("7.3", "የእንስሳት አቅርቦት", u => Supply(u, animalSupply.Select(x => (x.WoredaId, x.GrainSupplyStatus.GetDisplayName(), x.AdditionalNotes))));

        Header();
        Section("7.4", "የእህል ዋጋ በኩንታል (አማካይ፣ ብር)", "ያለፈው ሳምንት", "በዚህ ሳምንት");
        foreach (var type in Ordered(grainTypes.Select(x => (x.GrainTypeId, x.GrainName))
                     .Concat(grainRows.Where(x => x.GrainType != null).Select(x => (x.GrainTypeId, x.GrainType!.GrainName))), GrainOrder))
            Pair("", type.Name, u => Prices(grainRows.Where(x => x.GrainTypeId == type.Id && u.Contains(x.WoredaId)).Select(x => (x.WeeklyPrice, x.PriceDifference))));
        Section("7.5", "የእንስሳት ዋጋ በአንድ (አማካይ፣ ብር)", "ያለፈው ሳምንት", "በዚህ ሳምንት");
        foreach (var type in Ordered(animalTypes.Select(x => (x.AnimalTypeId, x.AnimalTypeName))
                     .Concat(animalRows.Where(x => x.AnimalType != null).Select(x => (x.AnimalTypeId, x.AnimalType!.AnimalTypeName))), AnimalOrder))
            Pair("", type.Name, u => Prices(animalRows.Where(x => x.AnimalTypeId == type.Id && u.Contains(x.WoredaId)).Select(x => (x.WeeklyPrice, x.PriceDifference))));

        // ------------------------------------------------------------------ 8–11. Hazards, migration, aid, other
        Section("8", "በሳምንቱ የታዩ ወቅታዊ ሁኔታዎች (አደጋዎች)");
        Text("8.1", "የተከሰቱ አደጋዎች", u =>
        {
            // One line per hazard type, so each hazard gets its own damage figures.
            var rows = accidents.Where(x => x.HasAccident).SelectMany(a => (a.AccidentDetails ?? []).Count == 0
                ? [(a.WoredaId, Place(a.WoredaId), new[] { "አደጋ ተከስቷል", a.Notes })]
                : (a.AccidentDetails ?? []).Select(d => (a.WoredaId, Place(a.WoredaId), new[]
                {
                    $"{d.AccidentType?.AccidentName ?? "አደጋ"} ተከስቷል",
                    d.DamagedLandInHectares is > 0 ? $"የተጎዳ መሬት {Num(d.DamagedLandInHectares.Value)} ሄ/ር" : null,
                    d.DamageRateInPercent is > 0 ? $"የጉዳት መጠን {Num(d.DamageRateInPercent.Value)}%" : null,
                    Count(d.AffectedHouseholdsMale + d.AffectedHouseholdsFemale, "አባወራ/እማወራ ተጎድተዋል"),
                    d.Notes,
                    a.Notes
                })));
            var text = Entries(u, rows);
            return string.IsNullOrEmpty(text) ? None : text;
        }, accidents.Select(x => x.WoredaId));
        Section("9", "በተለያዩ ምክንያት ስደት ወይም ሞት ካለ");
        Text("9.1", "ስደትና ሞት", u =>
        {
            string Where(Guid? id, Guid woredaId) => id.HasValue ? locationNames.GetValueOrDefault(id.Value, "-") : WoredaName(woredaId);
            var moved = migrations.Where(x => x.HasMigration).SelectMany(m => (m.MigrationDetails ?? []).Select(d => (m.WoredaId, Place(m.WoredaId), new[]
            {
                $"{Count(People(d.MaleHouseholdHeads, d.FemaleHouseholdHeads, d.MaleFamilyMembers, d.FemaleFamilyMembers, d.MaleChildren, d.FemaleChildren,
                    d.MaleYouth, d.FemaleYouth, d.MaleElderly, d.FemaleElderly, d.MaleDisabled, d.FemaleDisabled), "ሰው") ?? "ሰዎች"} ተሰደዋል " +
                    $"({Where(d.OriginLocationId, m.WoredaId)} → {Where(d.DestinationLocationId, m.WoredaId)})",
                Labeled("ምክንያት", d.MigrationReason),
                d.Notes,
                m.GeneralNotes
            })));
            var died = deaths.Where(x => x.HasDeath).SelectMany(x => (x.DeathDetails ?? []).Select(d => (x.WoredaId, Place(x.WoredaId), new[]
            {
                $"{Count(People(d.MaleHouseholdHeads, d.FemaleHouseholdHeads, d.MaleFamilyMembers, d.FemaleFamilyMembers, d.MaleChildren, d.FemaleChildren,
                    d.MaleYouth, d.FemaleYouth, d.MaleElderly, d.FemaleElderly, d.MaleDisabled, d.FemaleDisabled), "ሰው") ?? "ሰዎች"} ሞተዋል",
                Labeled("ምክንያት", d.DeathReason),
                d.Notes
            })));
            var parts = new[] { Entries(u, moved), Entries(u, died) }.Where(x => !string.IsNullOrEmpty(x)).ToList();
            return parts.Count == 0 ? None : string.Join("\n", parts);
        }, migrations.Select(x => x.WoredaId).Concat(deaths.Select(x => x.WoredaId)));
        Section("10", "በእርዳታ አቅርቦትና ስርጭት ላይ የታዩ ችግሮች");
        Text("10.1", "ያጋጠሙ ችግሮች", u =>
        {
            var text = Entries(u, provided.Where(x => x.IfProblem.HasValue && x.IfProblem != Problem.No)
                .Select(x => (x.WoredaId, Place(x.WoredaId), new[] { x.IfProblem!.Value.GetDisplayName(), x.Notes })));
            return string.IsNullOrEmpty(text) ? None : text;
        }, provided.Select(x => x.WoredaId));
        Section("11", "ሌሎች");
        Text("11.1", "ሌላ ያልተገለፀ ችግር", u =>
        {
            var text = Entries(u, other.Where(x => x.HasOtherProblem).Select(x => (x.WoredaId, Place(x.WoredaId), new[]
            {
                x.ProblemName,
                Count(People(x.MaleHouseholdHeads, x.FemaleHouseholdHeads, x.MaleFamilyMembers, x.FemaleFamilyMembers, x.MaleChildren, x.FemaleChildren,
                    x.MaleYouth, x.FemaleYouth, x.MaleElderly, x.FemaleElderly, x.MaleDisabled, x.FemaleDisabled), "ሰው ተጎድቷል"),
                x.GeneralNotes
            })));
            return string.IsNullOrEmpty(text) ? None : text;
        }, other.Select(x => x.WoredaId));

        return report;
    }

    // Region → one unit per zone; zone → one per woreda; woreda → itself. Woredas without a zone go under "-".
    private static List<Unit> BuildUnits(ReportScope scope, Dictionary<Guid, WoredaPlace> places, HashSet<Guid> reported, bool woredaColumns)
    {
        string NameOf(Guid id) => places.TryGetValue(id, out var p) && !string.IsNullOrWhiteSpace(p.Woreda) ? p.Woreda : "-";
        string ZoneOf(Guid id) => places.TryGetValue(id, out var p) && !string.IsNullOrWhiteSpace(p.Zone) ? p.Zone : "-";
        if (scope.Level == ReportScopeLevel.Region && !woredaColumns)
            return scope.WoredaIds.GroupBy(ZoneOf)
                .OrderBy(g => g.Key)
                .Select(g => new Unit(g.Key, g.ToHashSet(), reported))
                .ToList();
        // Woreda columns carry their zone (Group) so a region-wide grid can be split back into zones.
        return scope.WoredaIds.OrderBy(ZoneOf).ThenBy(NameOf)
            .Select(id => new Unit(NameOf(id), [id], reported, Group: ZoneOf(id))).ToList();
    }

    internal enum RainClass { NoData, Full, Partial, None }

    /// <summary>
    /// A woreda's rain for the week from its kebele counts: every reporting kebele full → Full, every one without
    /// rain → None, anything mixed → Partial. No report, or only "no data" kebeles → NoData.
    /// </summary>
    internal static RainClass ClassifyRain(RainfallReport? report)
    {
        if (report == null) return RainClass.NoData;
        var total = report.FullCoverageKebeles + report.PartialCoverageKebeles + report.NoRainKebeles;
        if (total <= 0) return RainClass.NoData;
        if (report.FullCoverageKebeles == total) return RainClass.Full;
        if (report.NoRainKebeles == total) return RainClass.None;
        return RainClass.Partial;
    }

    /// <summary>
    /// The span of rain amounts reported, on the scale ዝቅተኛ → መካከለኛ → ከፍተኛ: "ዝቅተኛ", "ከዝቅተኛ እስከ መካከለኛ", ….
    /// Each amount category covers part of the scale (e.g. ከዝቅተኛ - መካከለኛ covers ዝቅተኛ and መካከለኛ).
    /// </summary>
    internal static string RainAmount(IReadOnlyCollection<RainfallReport> reports)
    {
        if (reports.Count == 0) return "መረጃ የለም";
        string[] scale = ["ዝቅተኛ", "መካከለኛ", "ከፍተኛ"];
        var spans = new (int Kebeles, int Low, int High)[]
        {
            (reports.Sum(x => x.LowAmountKebeles), 0, 0),
            (reports.Sum(x => x.LowMediumAmountKebeles), 0, 1),
            (reports.Sum(x => x.MediumAmountKebeles), 1, 1),
            (reports.Sum(x => x.MediumHighAmountKebeles), 1, 2),
            (reports.Sum(x => x.HighAmountKebeles), 2, 2)
        }.Where(x => x.Kebeles > 0).ToList();
        if (spans.Count == 0)
            return reports.Sum(x => x.NoRainKebeles) > 0 ? "ዝናብ የለም" : "መረጃ የለም";
        var low = spans.Min(x => x.Low);
        var high = spans.Max(x => x.High);
        return low == high ? scale[low] : $"ከ{scale[low]} እስከ {scale[high]}";
    }

    /// <summary>
    /// Average previous/current price over the woredas that reported one; a missing or non-positive price is "no data".
    /// The previous price comes back from the saved difference (see <see cref="WeeklyFormReportService.PreviousPrice"/>).
    /// </summary>
    internal static (decimal? Previous, decimal? Current) Prices(IEnumerable<(decimal Current, decimal? Difference)> rows)
    {
        var list = rows.ToList();
        var previous = list.Select(x => WeeklyFormReportService.PreviousPrice(x.Current, x.Difference)).Where(x => x > 0).Select(x => x!.Value).ToList();
        var current = list.Where(x => x.Current > 0).Select(x => x.Current).ToList();
        return (previous.Count == 0 ? null : Math.Round(previous.Average(), 0), current.Count == 0 ? null : Math.Round(current.Average(), 0));
    }

    /// <summary>
    /// "ጭማሪ፡ ጤፍ፣ ስንዴ፤ ቅናሽ፡ ገብስ፤ ተመሳሳይ፡ ማሽላ". Each type compares the average of the woredas that have a price in
    /// both weeks; a change under 1% either way counts as ተመሳሳይ.
    /// </summary>
    internal static string PriceTrend(IEnumerable<(string Name, decimal Current, decimal? Difference)> rows)
    {
        var changes = rows
            .Select(x => (x.Name, x.Current, Previous: WeeklyFormReportService.PreviousPrice(x.Current, x.Difference)))
            .Where(x => x.Current > 0 && x.Previous > 0)
            .GroupBy(x => x.Name)
            .Select(g => (Name: g.Key, Percent: (g.Average(x => x.Current) - g.Average(x => x.Previous!.Value)) / g.Average(x => x.Previous!.Value) * 100))
            .ToList();
        if (changes.Count == 0) return "ለማነጻጸር የሚያስችል ዋጋ የለም";
        var parts = new List<string>();
        void Add(string label, IEnumerable<string> names)
        {
            var list = names.Order().ToList();
            if (list.Count > 0) parts.Add($"{label}፡ {string.Join(ListSeparator, list)}");
        }
        Add("ጭማሪ", changes.Where(x => x.Percent >= 1).Select(x => x.Name));
        Add("ቅናሽ", changes.Where(x => x.Percent <= -1).Select(x => x.Name));
        Add("ተመሳሳይ", changes.Where(x => x.Percent > -1 && x.Percent < 1).Select(x => x.Name));
        return string.Join(PartSeparator, parts);
    }

    private static List<(Guid Id, string Name)> Ordered(IEnumerable<(Guid Id, string Name)> types, string[] preferred) =>
        types.GroupBy(x => x.Id).Select(g => g.First())
            .OrderBy(x => Array.IndexOf(preferred, x.Name.Trim()) is var i && i >= 0 ? i : int.MaxValue)
            .ThenBy(x => x.Name)
            .ToList();

    private static decimal Round(decimal value) => Math.Round(value, 2);
    private static string Num(decimal value) => value.ToString(value % 1 == 0 ? "#,0" : "#,0.##", CultureInfo.InvariantCulture);
    private static string People(int count) => count > 0 ? $"{count:#,0} ሰው" : "";
}
