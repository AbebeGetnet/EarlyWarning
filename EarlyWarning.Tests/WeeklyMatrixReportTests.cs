using ClosedXML.Excel;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Tests;

/// <summary>
/// The Excel weekly report: one column block per zone for region users, rain classified per woreda,
/// woredas that sent nothing listed as "no data", and price averages per zone.
/// </summary>
public class WeeklyMatrixReportTests : IDisposable
{
    internal static readonly DateTime WeekStart = new(2026, 4, 25);
    internal static readonly DateTime WeekEnd = new(2026, 5, 1);

    private readonly EarlyWarningDbContext _db;
    private readonly Locations _region, _zoneA, _zoneB, _a1, _a2, _a3, _b1;
    private readonly GrainType _teff;

    public WeeklyMatrixReportTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"matrix-report-{Guid.NewGuid()}").Options);

        _region = Location("አማራ", LocationLevel.ክልል, null);
        _zoneA = Location("ሰሜን ጎንደር", LocationLevel.ዞን, _region);
        _zoneB = Location("ማዕከላዊ ጎንደር", LocationLevel.ዞን, _region);
        _a1 = Location("ደባርቅ", LocationLevel.ወረዳ, _zoneA);
        _a2 = Location("ጃናሞራ", LocationLevel.ወረዳ, _zoneA);
        _a3 = Location("በየዳ", LocationLevel.ወረዳ, _zoneA);   // sends nothing this week
        _b1 = Location("ደምቢያ", LocationLevel.ወረዳ, _zoneB);

        foreach (var woreda in new[] { _a1, _a2, _b1 })
            _db.WeeklyReportSubmissions.Add(new WeeklyReportSubmission
            {
                WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportingWeekStart = WeekStart,
                Status = WeeklySubmissionStatus.RegionApproved, CreatedById = "seed", CreatedAt = WeekStart, LastActionAt = WeekEnd, RowVersion = new byte[8]
            });

        Rain(_a1, full: 10, partial: 0, none: 0, low: 4, medium: 6);
        Rain(_a2, full: 2, partial: 3, none: 1, low: 6);
        Rain(_b1, full: 0, partial: 0, none: 9);

        _teff = new GrainType { GrainTypeId = Guid.NewGuid(), GrainName = "ጤፍ", IsActive = true };
        _db.GrainTypes.Add(_teff);
        Price(_a1, current: 10_000, difference: 200);   // previous 9,800
        Price(_a2, current: 11_000, difference: 0);     // previous 11,000
        Price(_b1, current: 9_000, difference: -500);   // previous 9,500

        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    // Shared with WeeklyNarrativeReportTests, which builds on the same data.
    internal EarlyWarningDbContext Db => _db;
    internal Task<WeeklyMatrixReport> BuildGrid(Locations scopeLocation, bool woredaColumns = false) => Build(scopeLocation, woredaColumns);

    [Fact]
    public void EthiopianDate_ConvertsKnownDates()
    {
        Assert.Equal(new EthiopianDate(2018, 1, 1), EthiopianDate.FromGregorian(new DateTime(2025, 9, 11)));   // እንቁጣጣሽ 2018
        Assert.Equal(new EthiopianDate(2018, 8, 17), EthiopianDate.FromGregorian(new DateTime(2026, 4, 25)));
        Assert.Equal(new EthiopianDate(2015, 13, 6), EthiopianDate.FromGregorian(new DateTime(2023, 9, 11)));  // ጳጉሜ 6 (2015 is a leap year)
        Assert.Equal(new EthiopianDate(2016, 1, 1), EthiopianDate.FromGregorian(new DateTime(2023, 9, 12)));
        Assert.Equal(new EthiopianDate(2016, 13, 5), EthiopianDate.FromGregorian(new DateTime(2024, 9, 10)));
        Assert.Equal("17/08/2018", EthiopianDate.FromGregorian(new DateTime(2026, 4, 25)).ToString());
    }

    [Fact]
    public async Task RegionUser_GetsOneColumnPerZone_PlusTotal()
    {
        var report = await Build(_region);

        Assert.Equal(["ማዕከላዊ ጎንደር", "ሰሜን ጎንደር", "ጠቅላላ ድምር"], report.Units.Select(x => x.Name));
        Assert.Contains("17/08/2018", report.Title);
        Assert.Equal(["1 ከ 1", "2 ከ 3", "3 ከ 4"], TextRow(report, "ሪፖርት የላኩ ወረዳዎች"));
    }

    [Fact]
    public async Task Incidents_NameZoneWoredaAndKebeles_WithNotes()
    {
        var k1 = Location("ቀበሌ 01", LocationLevel.ቀበሌ, _a1);
        var k2 = Location("ቀበሌ 02", LocationLevel.ቀበሌ, _a1);
        var dembiaKebele = Location("ቆላድባ", LocationLevel.ቀበሌ, _b1);
        var rain = _db.RainfallReports.Single(x => x.WoredaId == _a1.Id);
        rain.SelectedKebeleIds = [k2.Id, k1.Id];
        rain.SerializeKebeles();
        var issue = new EarlyWarning.Models.HumanDrinkWaterIssue.HumanDrinkWaterIssue
        {
            HumanDrinkWaterIssueId = Guid.NewGuid(), WoredaId = _b1.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd,
            HasWaterProblem = true, NumberOfAffectedKebeles = 1, MaleHouseholdHeads = 30, FemaleHouseholdHeads = 20,
            MaleChildren = 12, FemaleYouth = 9, FemaleDisabled = 3, // breakdowns of the 50: never added to it
            AdditionalNotes = "ምንጩ ደርቋል።"
        };
        _db.HumanDrinkWaterIssues.Add(issue);
        _db.HumanDrinkWaterKebeles.Add(new EarlyWarning.Models.HumanDrinkWaterIssue.HumanDrinkWaterKebele
        {
            HumanDrinkWaterKebeleId = Guid.NewGuid(), HumanDrinkWaterIssueId = issue.HumanDrinkWaterIssueId, KebeleId = dembiaKebele.Id
        });
        await _db.SaveChangesAsync();

        var report = await Build(_region);

        Assert.Equal("• ሰሜን ጎንደር ዞን፣ ደባርቅ ወረዳ፣ 2 ቀበሌ (ቀበሌ 01፣ ቀበሌ 02)፡ በከፍተኛ ዝናብ/ጎርፍ ተጎድተዋል", TextRow(report, "1.6")[1]);
        Assert.Equal("የለም", TextRow(report, "1.6")[0]);
        Assert.Equal("• ማዕከላዊ ጎንደር ዞን፣ ደምቢያ ወረዳ፣ 1 ቀበሌ (ቆላድባ)፡ የመጠጥ ውሃ እጥረት አለ፤ 50 ሰው ተጎድቷል፤ ምንጩ ደርቋል",
            TextRow(report, "4.4")[0]);
    }

    [Fact]
    public async Task TopicWithoutRows_SaysNoData_NotNoProblem()
    {
        var report = await Build(_region);

        // No crop-growth or pasture rows were saved: "መረጃ የለም", never "ሰብል የለም" / "ችግር የለም".
        Assert.Equal([WeeklyMatrixReportService.NoData, WeeklyMatrixReportService.NoData, null], TextRow(report, "3.1"));
        Assert.Equal([WeeklyMatrixReportService.NoData, WeeklyMatrixReportService.NoData, null], TextRow(report, "4.1"));
    }

    [Fact]
    public async Task Rain_IsClassifiedPerWoreda_AndMissingWoredasAreNoData()
    {
        var report = await Build(_region);

        // Zone order: ማዕከላዊ ጎንደር (B), ሰሜን ጎንደር (A).
        Assert.Equal(["የለም", "1 ወረዳ (ደባርቅ)", null], TextRow(report, "1.1"));
        Assert.Equal(["የለም", "1 ወረዳ (ጃናሞራ)", null], TextRow(report, "1.2"));
        Assert.Equal(["1 ወረዳ (ደምቢያ)", "የለም", null], TextRow(report, "1.3"));
        Assert.Equal(["የለም", "1 ወረዳ (በየዳ)", null], TextRow(report, "1.4"));
        Assert.Equal(["ዝናብ የለም", "ከዝቅተኛ እስከ መካከለኛ", null], TextRow(report, "1.5"));
    }

    [Fact]
    public async Task Prices_AreAveragedPerZone_AndOverAllWoredasInTheTotal()
    {
        var report = await Build(_region);
        var teff = report.Rows.Single(x => x.Kind == MatrixRowKind.Pair && x.Label == "ጤፍ");

        Assert.Equal((9_500m, 9_000m), (teff.Cells[0].Left, teff.Cells[0].Right));     // ማዕከላዊ ጎንደር
        Assert.Equal((10_400m, 10_500m), (teff.Cells[1].Left, teff.Cells[1].Right));   // ሰሜን ጎንደር
        Assert.Equal((10_100m, 10_000m), (teff.Cells[2].Left, teff.Cells[2].Right));   // all three woredas
    }

    [Fact]
    public async Task ZoneUser_GetsOneColumnPerWoreda()
    {
        var report = await Build(_zoneA);

        Assert.Equal(["በየዳ", "ደባርቅ", "ጃናሞራ", "ጠቅላላ ድምር"], report.Units.Select(x => x.Name));
        Assert.Equal(WeeklyMatrixReportService.NotReported, TextRow(report, "1.1")[0]);  // በየዳ sent nothing
    }

    [Fact]
    public async Task Excel_HasZoneHeadersAndTitle()
    {
        var bytes = WeeklyMatrixExcelWriter.Write(await Build(_region));

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = Assert.Single(workbook.Worksheets);
        Assert.Contains("17/08/2018", sheet.Cell(1, 1).GetString());
        Assert.Equal("ዝርዝር ስራዎች", sheet.Cell(3, 2).GetString());
        Assert.Equal("ማዕከላዊ ጎንደር (1)", sheet.Cell(3, 3).GetString());
        Assert.Equal("ሰሜን ጎንደር (2)", sheet.Cell(3, 5).GetString());
        Assert.Equal("ጠቅላላ ድምር", sheet.Cell(3, 7).GetString());
    }

    [Theory]
    [InlineData(8, new[] { 8 })]
    [InlineData(9, new[] { 9 })]          // 8 zones + total: the total stays with them
    [InlineData(15, new[] { 8, 7 })]      // 14 zones + total, as the Amhara workbook (8 + 6 zones)
    public void Pages_SplitAtEightColumns(int units, int[] expected) =>
        Assert.Equal(expected, WeeklyMatrixExcelWriter.Pages(units).Select(x => x.Count));

    [Fact]
    public void PriceTrend_GroupsByDirection() =>
        Assert.Equal("ጭማሪ፡ ጤፍ፤ ቅናሽ፡ ገብስ፤ ተመሳሳይ፡ ማሽላ", WeeklyMatrixReportService.PriceTrend(
        [
            ("ጤፍ", 10_200m, 200m), ("ገብስ", 7_000m, -300m), ("ማሽላ", 5_000m, 20m)   // +2%, -4.1%, +0.4%
        ]));

    // ------------------------------------------------------------------ helpers

    private async Task<WeeklyMatrixReport> Build(Locations scopeLocation, bool woredaColumns = false)
    {
        var scopes = new ReportScopeService(_db);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(), FirstName = "Test", LastName = "User",
            LocationId = scopeLocation.Id, UserLocationLevl = scopeLocation.Level!.Value
        };
        var scope = (await scopes.ResolveAsync(user)).Scope!;
        var week = (await scopes.GetSubmissionsAsync(scope)).Where(x => x.StartDate == WeekStart).ToList();
        return await new WeeklyMatrixReportService(_db).BuildAsync(scope, week, WeekStart, WeekEnd, woredaColumns);
    }

    private static List<string?> TextRow(WeeklyMatrixReport report, string numberOrLabel) =>
        report.Rows.Single(x => x.Kind == MatrixRowKind.Text && (x.Number == numberOrLabel || x.Label == numberOrLabel))
            .Cells.Select(x => x.Text).ToList();

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

    private void Rain(Locations woreda, int full, int partial, int none, int low = 0, int medium = 0) =>
        _db.RainfallReports.Add(new RainfallReport
        {
            Id = Guid.NewGuid(), WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, LasModifiedAt = WeekEnd,
            FullCoverageKebeles = full, PartialCoverageKebeles = partial, NoRainKebeles = none,
            LowAmountKebeles = low, MediumAmountKebeles = medium
        });

    private void Price(Locations woreda, decimal current, decimal difference) =>
        _db.GrainPricePerQuintals.Add(new GrainPricePerQuintal
        {
            GrainPricePerQuintalId = Guid.NewGuid(), WoredaId = woreda.Id, GrainTypeId = _teff.GrainTypeId,
            StartDate = WeekStart, EndDate = WeekEnd, WeeklyPrice = current, PriceDifference = difference, ReportDate = WeekEnd
        });
}
