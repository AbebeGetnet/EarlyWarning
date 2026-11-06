using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Tests;

/// <summary>
/// Zone and region reports: kebeles, woredas and zones affected by an event are counted separately; prices are
/// averaged per item over valid woreda observations; text keeps its woreda/zone source.
/// Sample region (affected kebeles of the human drinking water event):
///   Zone A: A1 5, A2 3, A3 0, A4 2  → 10 kebeles, 3 woredas
///   Zone B: B1 0                    →  0
///   Zone C: C1 4, C2 3              →  7 kebeles, 2 woredas
///   Zone D: D1 4                    →  4 kebeles, 1 woreda
///   Region                          → 21 kebeles, 6 woredas, 3 zones
/// </summary>
public class ReportAggregationRulesTests : IDisposable
{
    private static readonly DateTime WeekStart = new(2026, 10, 5);
    private static readonly DateTime WeekEnd = new(2026, 10, 11);

    private readonly EarlyWarningDbContext _db;
    private readonly Locations _zoneA, _zoneB, _zoneC, _zoneD;
    private readonly Locations _a1, _a2, _a3, _a4, _b1, _c1, _c2, _d1;
    private readonly GrainType _teff, _maize;

    public ReportAggregationRulesTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"aggregation-{Guid.NewGuid()}").Options);

        var region = Location("ክልል", LocationLevel.ክልል, null);
        _zoneA = Location("ዞን ሀ", LocationLevel.ዞን, region);
        _zoneB = Location("ዞን ለ", LocationLevel.ዞን, region);
        _zoneC = Location("ዞን ሐ", LocationLevel.ዞን, region);
        _zoneD = Location("ዞን መ", LocationLevel.ዞን, region);
        _a1 = Location("ወረዳ A1", LocationLevel.ወረዳ, _zoneA);
        _a2 = Location("ወረዳ A2", LocationLevel.ወረዳ, _zoneA);
        _a3 = Location("ወረዳ A3", LocationLevel.ወረዳ, _zoneA);
        _a4 = Location("ወረዳ A4", LocationLevel.ወረዳ, _zoneA);
        _b1 = Location("ወረዳ B1", LocationLevel.ወረዳ, _zoneB);
        _c1 = Location("ወረዳ C1", LocationLevel.ወረዳ, _zoneC);
        _c2 = Location("ወረዳ C2", LocationLevel.ወረዳ, _zoneC);
        _d1 = Location("ወረዳ D1", LocationLevel.ወረዳ, _zoneD);

        foreach (var (woreda, kebeles) in new[] { (_a1, 5), (_a2, 3), (_a3, 0), (_a4, 2), (_b1, 0), (_c1, 4), (_c2, 3), (_d1, 4) })
        {
            Submission(woreda);
            HumanWater(woreda, kebeles, notes: kebeles > 0 ? "ምንጭ ደርቋል" : null);
        }

        _teff = new GrainType { GrainTypeId = Guid.NewGuid(), GrainName = "ጤፍ" };
        _maize = new GrainType { GrainTypeId = Guid.NewGuid(), GrainName = "በቆሎ" };
        _db.AddRange(_teff, _maize);
        // Teff: three woredas in zone A at 100, one in zone C at 200 → 125 over the observations (not 150 = mean of zone means).
        GrainPrice(_a1, _teff, 100, difference: 10); // last week 90
        GrainPrice(_a2, _teff, 100);
        GrainPrice(_a3, _teff, 100);
        GrainPrice(_a4, _teff, 0);      // no valid price: ignored, not a 0 observation
        GrainPrice(_c1, _teff, 200);
        GrainPrice(_d1, _maize, 50);    // a different item is never mixed into teff

        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    // ------------------------------------------------------------------ zone report

    [Fact]
    public async Task ZoneReport_CountsKebelesAndAffectedWoredasSeparately()
    {
        var items = HumanWaterItems(await Build(ReportScopeLevel.Zone, _a1, _a2, _a3, _a4));

        Assert.Equal("10", Value(items, "ችግር ያለባቸው ቀበሌዎች በቁጥር"));
        Assert.Equal("3 ወረዳ", Value(items, "የተጎዱ ወረዳዎች ብዛት"));
        Assert.DoesNotContain(items, x => x.Label == "የተጎዱ ዞኖች ብዛት"); // a zone report is one zone

        var list = Assert.Single(items.Single(x => x.Label == "የተጎዱ ወረዳዎች").ZoneTexts);
        Assert.Equal("ዞን ሀ ዞን", list.Group);
        Assert.Contains("ወረዳ A1 (5 ቀበሌ)", list.Text);
        Assert.Contains("ወረዳ A2 (3 ቀበሌ)", list.Text);
        Assert.Contains("ወረዳ A4 (2 ቀበሌ)", list.Text);
        Assert.DoesNotContain("A3", list.Text);
        Assert.Equal("ወረዳ A1 (5 ቀበሌ)፣ ወረዳ A2 (3 ቀበሌ)፣ ወረዳ A4 (2 ቀበሌ)። ድምር፡ 3 የተጎዱ ወረዳዎች እና 10 የተጎዱ ቀበሌዎች", list.Text);
        Assert.DoesNotContain(items, x => x.Label == "የተጎዱ ዞኖች"); // zone → woreda brackets only
    }

    [Fact]
    public async Task ZoneReport_ConcatenatesTextPerWoreda_SkippingEmpty()
    {
        var items = HumanWaterItems(await Build(ReportScopeLevel.Zone, _a1, _a2, _a3, _a4));

        var notes = Assert.Single(items.Single(x => x.Label == "ማስታወሻ").ZoneTexts);
        Assert.Equal("ወረዳ A1፡ ምንጭ ደርቋል፤ ወረዳ A2፡ ምንጭ ደርቋል፤ ወረዳ A4፡ ምንጭ ደርቋል", notes.Text); // same text, different woredas: all kept
    }

    // ------------------------------------------------------------------ region report

    [Fact]
    public async Task RegionReport_CountsKebelesWoredasAndZonesSeparately()
    {
        var items = HumanWaterItems(await Build(ReportScopeLevel.Region, _a1, _a2, _a3, _a4, _b1, _c1, _c2, _d1));

        Assert.Equal("21", Value(items, "ችግር ያለባቸው ቀበሌዎች በቁጥር"));
        Assert.Equal("6 ወረዳ", Value(items, "የተጎዱ ወረዳዎች ብዛት"));
        Assert.Equal("3 ዞን", Value(items, "የተጎዱ ዞኖች ብዛት"));

        // Region line: each affected zone with its affected woredas in brackets; zone B (none affected) is left out.
        var zones = Value(items, "የተጎዱ ዞኖች")!;
        Assert.Contains("ዞን ሀ ዞን (3 ወረዳ)", zones);
        Assert.Contains("ዞን ሐ ዞን (2 ወረዳ)", zones);
        Assert.Contains("ዞን መ ዞን (1 ወረዳ)", zones);
        Assert.DoesNotContain("ዞን ለ", zones);

        // Under each zone: its woredas with their kebeles in brackets, and the zone's totals.
        var groups = items.Single(x => x.Label == "የተጎዱ ወረዳዎች").ZoneTexts;
        Assert.Equal(["ዞን ሀ ዞን (3 ወረዳ)", "ዞን ሐ ዞን (2 ወረዳ)", "ዞን መ ዞን (1 ወረዳ)"], groups.Select(x => x.Group).Order());
        Assert.Equal("ወረዳ C1 (4 ቀበሌ)፣ ወረዳ C2 (3 ቀበሌ)። ድምር፡ 2 የተጎዱ ወረዳዎች እና 7 የተጎዱ ቀበሌዎች", groups.Single(x => x.Group == "ዞን ሐ ዞን (2 ወረዳ)").Text);
        Assert.EndsWith("ድምር፡ 3 የተጎዱ ወረዳዎች እና 10 የተጎዱ ቀበሌዎች", groups.Single(x => x.Group == "ዞን ሀ ዞን (3 ወረዳ)").Text);
    }

    [Fact]
    public async Task RegionReport_YesNoAnswersCountWoredasAndZones()
    {
        var items = HumanWaterItems(await Build(ReportScopeLevel.Region, _a1, _a2, _a3, _a4, _b1, _c1, _c2, _d1));

        Assert.Equal("6 ወረዳ / 3 ዞን", Value(items, "ችግር አለ"));
        Assert.Equal("2 ወረዳ / 2 ዞን", Value(items, "ችግር የለም")); // A3 (zone A) and B1 (zone B)
    }

    [Fact]
    public async Task WoredaReport_KeepsItsDetailWithoutHigherLevelCounts()
    {
        var items = HumanWaterItems(await Build(ReportScopeLevel.Woreda, _a1));

        Assert.Equal("5", Value(items, "ችግር ያለባቸው ቀበሌዎች በቁጥር"));
        Assert.DoesNotContain(items, x => x.Label is "የተጎዱ ወረዳዎች ብዛት" or "የተጎዱ ዞኖች ብዛት" or "የተጎዱ ወረዳዎች");
    }

    [Fact]
    public async Task RegionReport_AveragesEachPriceItemOverValidWoredaObservations()
    {
        var sections = await Build(ReportScopeLevel.Region, _a1, _a2, _a3, _a4, _b1, _c1, _c2, _d1);
        var table = sections.Single(x => x.Number == "8").Items.First(x => x.Kind == FormReportItemKind.Table && x.TableHeaders[0] == "የእህል አይነት");

        var teff = table.TableRows.Single(x => x[0] == "ጤፍ");
        Assert.Equal("90", teff[1]);        // previous week: only A1 reported one
        Assert.Equal("125", teff[2]);       // (100+100+100+200)/4; A4's 0 is not a price
        Assert.Equal("+10 (+11.11%)", teff[3]); // compared only where both weeks exist (A1: 90 → 100)
        Assert.Equal("4", teff[4]);
        Assert.Equal("50", table.TableRows.Single(x => x[0] == "በቆሎ")[2]);
    }

    // ------------------------------------------------------------------ aggregate (dashboard / page two)

    [Fact]
    public async Task Aggregate_PerZoneSummariesCombineIntoRegionCounts()
    {
        var service = new WeeklyAggregationService(_db);
        var perZone = new List<WeeklyAggregateViewModel>();
        foreach (var zone in new[] { new[] { _a1, _a2, _a3, _a4 }, new[] { _b1 }, new[] { _c1, _c2 }, new[] { _d1 } })
            perZone.Add(await service.BuildAsync(await Reports(zone)));

        var zoneA = perZone[0].AffectedLocations.Single(x => x.Event == AffectedLocations.Events.HumanWaterShortage);
        Assert.Equal((10, 3, 1), (zoneA.Kebeles, zoneA.Woredas, zoneA.Zones));

        var region = AffectedLocations.Combine(AffectedLocations.Events.HumanWaterShortage,
            perZone.SelectMany(x => x.AffectedLocations).Where(x => x.Event == AffectedLocations.Events.HumanWaterShortage));
        Assert.Equal((21, 6, 3), (region.Kebeles, region.Woredas, region.Zones));

        // Built in one pass over the whole region, the result is the same.
        var direct = (await service.BuildAsync(await Reports(_a1, _a2, _a3, _a4, _b1, _c1, _c2, _d1)))
            .AffectedLocations.Single(x => x.Event == AffectedLocations.Events.HumanWaterShortage);
        Assert.Equal((21, 6, 3), (direct.Kebeles, direct.Woredas, direct.Zones));
    }

    [Fact]
    public async Task Aggregate_PriceAveragesCarryTheirObservationCount_ForWeightedRegionalSummary()
    {
        var service = new WeeklyAggregationService(_db);
        var zoneA = Teff(await service.BuildAsync(await Reports(_a1, _a2, _a3, _a4)));
        var zoneC = Teff(await service.BuildAsync(await Reports(_c1, _c2)));

        Assert.Equal((100m, 3m), (zoneA.Value, zoneA.Weight)); // A4's 0 price is excluded
        Assert.Equal((200m, 1m), (zoneC.Value, zoneC.Weight));
        // ReportPageTwo combines zone averages weighted by observations: 125, not the 150 of an average of averages.
        Assert.Equal(125m, (zoneA.Value * zoneA.Weight + zoneC.Value * zoneC.Weight) / (zoneA.Weight + zoneC.Weight));

        var zoneD = await service.BuildAsync(await Reports(_d1));
        var section = zoneD.Sections.Single(x => x.Name.StartsWith("8.2"));
        Assert.DoesNotContain(section.Metrics, x => x.Name.StartsWith("ጤፍ"));            // no teff data: no 0 average
        Assert.DoesNotContain(section.Metrics, x => x.Name == "በቆሎ — ያለፈው ሳምንት");    // no previous price: omitted, not 0
    }

    // ------------------------------------------------------------------ helpers

    private static AggregateMetricViewModel Teff(WeeklyAggregateViewModel aggregate) =>
        aggregate.Sections.Single(x => x.Name.StartsWith("8.2")).Metrics.Single(x => x.Name == "ጤፍ — በዚህ ሳምንት");

    private async Task<List<FormReportSection>> Build(ReportScopeLevel level, params Locations[] woredas) =>
        await new WeeklyFormReportService(_db).BuildAsync(await Reports(woredas), level);

    private async Task<List<WeeklyReportSubmission>> Reports(params Locations[] woredas)
    {
        var ids = woredas.Select(x => x.Id).ToList();
        return await _db.WeeklyReportSubmissions.Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .Where(x => ids.Contains(x.WoredaId)).ToListAsync();
    }

    private static List<FormReportItem> HumanWaterItems(List<FormReportSection> sections) => sections.Single(x => x.Number == "5").Items;

    private static string? Value(List<FormReportItem> items, string label) => items.Single(x => x.Label == label && x.Kind == FormReportItemKind.Line).Value;

    private Locations Location(string name, LocationLevel level, Locations? parent)
    {
        var location = new Locations
        {
            Id = Guid.NewGuid(), LocationName = name, LocationAmharicName = name, LocationCode = name, PhoneNumber = "0000",
            Level = level, ParentId = parent?.Id, IsActive = true
        };
        _db.Locations.Add(location);
        return location;
    }

    private void Submission(Locations woreda) =>
        _db.WeeklyReportSubmissions.Add(new WeeklyReportSubmission
        {
            WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportingWeekStart = WeekStart,
            Status = WeeklySubmissionStatus.RegionApproved, CreatedById = "seed", CreatedAt = WeekStart, LastActionAt = WeekEnd, RowVersion = new byte[8]
        });

    private void HumanWater(Locations woreda, int kebeles, string? notes) =>
        _db.HumanDrinkWaterIssues.Add(new HumanDrinkWaterIssue
        {
            HumanDrinkWaterIssueId = Guid.NewGuid(), WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd,
            HasWaterProblem = kebeles > 0, NumberOfAffectedKebeles = kebeles, AdditionalNotes = notes, ReportDate = WeekEnd
        });

    // As the wizard saves it: the difference to the immediately preceding week, or null when there was none.
    private void GrainPrice(Locations woreda, GrainType grain, decimal price, decimal? difference = null) =>
        _db.GrainPricePerQuintals.Add(new GrainPricePerQuintal
        {
            GrainPricePerQuintalId = Guid.NewGuid(), WoredaId = woreda.Id, GrainTypeId = grain.GrainTypeId, StartDate = WeekStart, EndDate = WeekEnd,
            WeeklyPrice = price, PriceDifference = difference, ReportDate = WeekEnd
        });
}
