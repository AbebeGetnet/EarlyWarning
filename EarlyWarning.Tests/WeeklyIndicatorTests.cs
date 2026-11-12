using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.Models.AnimalPrice;
using EarlyWarning.Models.EpidemicDisease;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Tests;

/// <summary>
/// Key indicators for response planning. Sample week, region with two zones:
///   A1 (zone A): no rain, human epidemic, GAM (5 + 12) / 100 = 17%, wheat aid 100 requested / 60 provided / 30 distributed,
///                maize 4,000 (last week 3,600), goat 6,000 (last week 6,500)          → emergency
///   A2 (zone A): pasture shortage only                                                    → watch
///   B1 (zone B): nothing wrong; 200 children screened, 6 moderate (GAM 3%); wheat aid 50 / 50 / 50 → normal
///   B2 (zone B): sends no report
/// </summary>
public class WeeklyIndicatorTests : IDisposable
{
    private static readonly DateTime WeekStart = new(2026, 10, 5);
    private static readonly DateTime WeekEnd = new(2026, 10, 7);

    private readonly EarlyWarningDbContext _db;
    private readonly Locations _region, _zoneA, _zoneB, _a1, _a2, _b1;

    public WeeklyIndicatorTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"indicators-{Guid.NewGuid()}").Options);
        _region = Location("ክልል", LocationLevel.ክልል, null);
        _zoneA = Location("ዞን ሀ", LocationLevel.ዞን, _region);
        _zoneB = Location("ዞን ለ", LocationLevel.ዞን, _region);
        _a1 = Location("ወረዳ A1", LocationLevel.ወረዳ, _zoneA);
        _a2 = Location("ወረዳ A2", LocationLevel.ወረዳ, _zoneA);
        _b1 = Location("ወረዳ B1", LocationLevel.ወረዳ, _zoneB);
        Location("ወረዳ B2", LocationLevel.ወረዳ, _zoneB);   // in scope, sends nothing
        foreach (var woreda in new[] { _a1, _a2, _b1 })
            _db.WeeklyReportSubmissions.Add(new WeeklyReportSubmission
            {
                WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportingWeekStart = WeekStart,
                Status = WeeklySubmissionStatus.RegionApproved, CreatedById = "seed", CreatedAt = WeekStart, LastActionAt = WeekEnd, RowVersion = new byte[8]
            });

        // A1
        _db.RainfallReports.Add(new RainfallReport { Id = Guid.NewGuid(), WoredaId = _a1.Id, StartDate = WeekStart, EndDate = WeekEnd, LasModifiedAt = WeekEnd, NoRainKebeles = 8 });
        _db.EpidemicDiseases.Add(new EpidemicDisease
        {
            EpidemicDiseaseId = Guid.NewGuid(), WoredaId = _a1.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd,
            HasEpidemicDisease = true, NumberOfAffectedKebeles = 2, MaleHouseholdHeads = 10, FemaleHouseholdHeads = 5, MaleFamilyMembers = 20, FemaleFamilyMembers = 15,
            MaleChildren = 9   // a breakdown of the 50 people above, never added
        });
        _db.MalnutritionReports.Add(new MalnutritionReport { WoredaId = _a1.Id, StartDate = WeekStart, EndDate = WeekEnd, ChildrenScreened = 100, ChildrenSevere = 5, ChildrenModerate = 12 });
        var wheat = new SupplyType { SupplyTypeId = Guid.NewGuid(), SupplyTypeName = "ስንዴ" };
        _db.SupplyTypes.Add(wheat);
        Aid(_a1, wheat, requested: 100, provided: 60, distributed: 30);
        var maize = new GrainType { GrainTypeId = Guid.NewGuid(), GrainName = "በቆሎ", IsActive = true };
        var goat = new AnimalType { AnimalTypeId = Guid.NewGuid(), AnimalTypeName = "ፍየል", IsActive = true };
        _db.GrainTypes.Add(maize);
        _db.AnimalTypes.Add(goat);
        _db.GrainPricePerQuintals.Add(new GrainPricePerQuintal
        {
            GrainPricePerQuintalId = Guid.NewGuid(), WoredaId = _a1.Id, GrainTypeId = maize.GrainTypeId, StartDate = WeekStart, EndDate = WeekEnd,
            WeeklyPrice = 4_000, PriceDifference = 400, ReportDate = WeekEnd
        });
        _db.AnimalPricePerUnits.Add(new AnimalPricePerUnit
        {
            AnimalPricePerUnitId = Guid.NewGuid(), WoredaId = _a1.Id, AnimalTypeId = goat.AnimalTypeId, StartDate = WeekStart, EndDate = WeekEnd,
            WeeklyPrice = 6_000, PriceDifference = -500, ReportDate = WeekEnd
        });

        // A2
        _db.PastureStatuses.Add(new PastureStatus { Id = Guid.NewGuid(), WoredaId = _a2.Id, StartDate = WeekStart, EndDate = WeekEnd, LasModifiedAt = WeekEnd, Enough = false, NumberOfAnimalsAffected = 40 });

        // B1
        _db.RainfallReports.Add(new RainfallReport { Id = Guid.NewGuid(), WoredaId = _b1.Id, StartDate = WeekStart, EndDate = WeekEnd, LasModifiedAt = WeekEnd, FullCoverageKebeles = 12 });
        _db.MalnutritionReports.Add(new MalnutritionReport { WoredaId = _b1.Id, StartDate = WeekStart, EndDate = WeekEnd, ChildrenScreened = 200, ChildrenModerate = 6 });
        Aid(_b1, wheat, requested: 50, provided: 50, distributed: 50);
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Facts_ReadEveryTopicOfTheWoredasWeek()
    {
        var facts = await Load();
        var a1 = facts.Single(x => x.WoredaId == _a1.Id);
        Assert.True(a1.NoRain);
        Assert.True(a1.Epidemic);
        Assert.Equal(50, a1.EpidemicPeople);              // household heads + family only
        Assert.Equal(17m, a1.GamRate);
        Assert.Equal(11.1m, a1.StaplePriceChange);         // (4,000 − 3,600) / 3,600
        Assert.Equal(AlertLevel.Emergency, a1.Level);      // rain 2 + epidemic 3 + GAM ≥ 15% 3 + SAM 1 = 9 (+ price 1)
        Assert.Equal(AlertLevel.Watch, facts.Single(x => x.WoredaId == _a2.Id).Level);
        Assert.Equal(AlertLevel.Normal, facts.Single(x => x.WoredaId == _b1.Id).Level);
    }

    [Fact]
    public async Task Summary_PoolsMalnutritionAid_AndRanksWoredas()
    {
        var s = WeeklyIndicators.Summarize(await Load(), woredasInScope: 4);

        // GAM is recomputed from the pooled counts, never averaged: (5 + 12 + 0 + 6) / 300 = 7.67%.
        Assert.Equal(7.67m, s.MalnutritionTotal!.GamRate);
        Assert.Equal(2, s.Malnutrition.Count);

        // Aid is pooled per item and unit: 150 requested, 110 provided, 80 distributed.
        var wheat = Assert.Single(s.Aid);
        Assert.Equal((150m, 110m, 80m), (wheat.Requested, wheat.Provided, wheat.Distributed));
        Assert.Equal(73.3m, wheat.Coverage);
        Assert.Equal(40m, wheat.Gap);

        // Terms of trade: 6,000 / (4,000 / 100) = 150 kg of maize per goat; last week 6,500 / 36 = 180.6 kg.
        var trade = Assert.Single(s.TermsOfTrade);
        Assert.Equal(("ፍየል", "በቆሎ", 150m, 180.6m), (trade.Animal, trade.Grain, trade.Current, trade.Previous!.Value));
        Assert.Equal(-16.9m, trade.Change);

        // Priorities: A1 first; B1 (no flag) is not listed.
        Assert.Equal(["ወረዳ A1", "ወረዳ A2"], s.Priorities.Select(x => x.Woreda));
        Assert.Equal(AlertLevel.Emergency, s.Kpis[0].Level);   // overview line comes first

        // Coverage: 3 of 4 woredas reported.
        Assert.Contains(s.Kpis, k => k.Label == "የሪፖርት ሽፋን" && k.Value.StartsWith("3 ከ 4") && k.Level == AlertLevel.Watch);

        // Actions: the most severe first, each with its woredas; scope-wide aid gap and reporting reminders follow.
        Assert.Equal(["ስነ-ምግብ", "ጤና"], s.Actions.Take(2).Select(x => x.Sector).Order());   // epidemic and GAM ≥ 15%: emergency
        Assert.All(s.Actions.Take(2), a => Assert.Equal(["ወረዳ A1 (ዞን ሀ)"], a.Woredas));
        Assert.Contains(s.Actions, a => a.Sector == "እንስሳት" && a.Woredas.SequenceEqual(["ወረዳ A2 (ዞን ሀ)"]));
        Assert.Contains(s.Actions, a => a.Sector == "እርዳታ" && a.Action.Contains("ስንዴ 40"));
        Assert.Contains(s.Actions, a => a.Sector == "ሪፖርት");
    }

    [Fact]
    public async Task Narrative_GetsIndicatorSections_ExcelGridIsUnchanged()
    {
        var scopes = new ReportScopeService(_db);
        var user = new ApplicationUser { Id = "u", FirstName = "T", LastName = "U", LocationId = _region.Id, UserLocationLevl = LocationLevel.ክልል };
        var scope = (await scopes.ResolveAsync(user)).Scope!;
        var reports = (await scopes.GetSubmissionsAsync(scope)).ToList();
        var grid = await new WeeklyMatrixReportService(_db).BuildAsync(scope, reports, WeekStart, WeekEnd);
        var excelBefore = WeeklyMatrixExcelWriter.Write(grid).Length;
        var rowsBefore = grid.Rows.Count;

        var narrative = WeeklyNarrativeReportBuilder.Build(grid, scope.LocationName, scope.Level);
        var summary = WeeklyIndicators.Summarize(await new WeeklyIndicatorService(_db).LoadAsync(reports), narrative.WoredasInScope);
        WeeklyIndicatorReport.Append(narrative, summary);

        Assert.Equal("kpi", narrative.Sections[0].Key);
        Assert.Equal(["nutrition", "priorities", "actions"], narrative.Sections.TakeLast(3).Select(x => x.Key));
        Assert.Equal(["12", "13", "14"], narrative.Sections.TakeLast(3).Select(x => x.Number));
        Assert.Contains(narrative.Sections.Single(x => x.Key == "aid").Blocks, b => b.Kind == NarrativeBlockKind.Table && b.Headers![2] == "የተጠየቀ");
        // The Excel workbook is written from the grid, which the indicators never touch.
        Assert.Equal(rowsBefore, grid.Rows.Count);
        Assert.Equal(excelBefore, WeeklyMatrixExcelWriter.Write(grid).Length);
    }

    [Fact]
    public async Task FormReport_GetsNutritionKpiPriorityAndActionSections()
    {
        var summary = WeeklyIndicators.Summarize(await Load(), 4);
        var sections = WeeklyIndicatorReport.FormSections(summary, firstNumber: 12).ToList();
        Assert.Equal(["12", "13", "14", "15"], sections.Select(x => x.Number));
        Assert.Contains(sections[0].Items, x => x.Label == "GAM መጠን" && x.Value!.StartsWith("7.7%"));
    }

    [Theory]
    [InlineData(4.9, AlertLevel.Normal)]
    [InlineData(5, AlertLevel.Watch)]
    [InlineData(10, AlertLevel.Alarm)]
    [InlineData(15, AlertLevel.Emergency)]
    public void GamBands_FollowTheWhoThresholds(double rate, AlertLevel expected) =>
        Assert.Equal(expected, WeeklyIndicators.GamBand((decimal)rate).Level);

    [Fact]
    public void Levels_LifeThreateningFlagIsAtLeastAlarm()
    {
        Assert.Equal(AlertLevel.Normal, WeeklyIndicators.LevelFor([]));
        Assert.Equal(AlertLevel.Watch, WeeklyIndicators.LevelFor([new("pasture", 1, ""), new("aid", 1, "")]));
        Assert.Equal(AlertLevel.Alarm, WeeklyIndicators.LevelFor([new("deaths", 3, "")]));
        Assert.Equal(AlertLevel.Alarm, WeeklyIndicators.LevelFor([new("rain", 2, ""), new("body", 2, "")]));
        Assert.Equal(AlertLevel.Emergency, WeeklyIndicators.LevelFor([new("epidemic", 3, ""), new("rain", 2, ""), new("flood", 2, "")]));
    }

    // ------------------------------------------------------------------ helpers

    private async Task<List<WoredaWeekFacts>> Load()
    {
        var reports = await _db.WeeklyReportSubmissions.AsNoTracking().ToListAsync();
        return await new WeeklyIndicatorService(_db).LoadAsync(reports);
    }

    private void Aid(Locations woreda, SupplyType type, decimal requested, decimal provided, decimal distributed) =>
        _db.WeeklyProvideds.Add(new WeeklyProvided
        {
            WeeklyProvidedId = Guid.NewGuid(), WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd,
            WeeklyProvidedDetail =
            [
                new WeeklyProvidedDetail
                {
                    WeeklyProvidedDetailId = Guid.NewGuid(), SupplyTypeId = type.SupplyTypeId, Measurement = Measurement.ኩንታል,
                    RequestedQuantity = requested, ProvidedQuantity = provided, DistributedQuantity = distributed
                }
            ]
        });

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
}
