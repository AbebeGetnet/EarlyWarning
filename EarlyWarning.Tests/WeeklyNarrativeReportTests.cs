using System.Reflection;
using EarlyWarning.Models;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;

namespace EarlyWarning.Tests;

/// <summary>
/// The narrative weekly report follows the commission's standard: header and period, sections 1–11, the zone
/// rain table, incident bullets naming zone/woreda/kebele, and a "በሌሎች … የለም" closing for the zones without one.
/// Uses the data of <see cref="WeeklyMatrixReportTests"/>: zone A (ደባርቅ full rain, ጃናሞራ partial, በየዳ not
/// reported) and zone B (ደምቢያ no rain).
/// </summary>
public class WeeklyNarrativeReportTests : IDisposable
{
    private readonly WeeklyMatrixReportTests _data = new();

    public void Dispose() => _data.Dispose();

    [Fact]
    public async Task Region_HasStandardHeaderAndSectionOrder()
    {
        var report = await Narrative();

        Assert.Equal("የአማራ ክልል አደጋ ስጋት ስራ አመራር ኮሚሽን", report.Organization);
        Assert.Equal("የቅድሚያ ማስጠንቀቂያና ምላሽ ዳይሬክቶሬት", report.Directorate);
        Assert.Contains("17/08/2018", report.Period);
        Assert.Equal((3, 4), (report.WoredasReported, report.WoredasInScope));
        Assert.Equal(["1", "2", "2.1", "2.2", "3", "3.1", "3.2", "3.3", "4", "5", "6", "7", "7.1", "7.2", "7.3", "8", "9", "10", "11"],
            report.Sections.Select(x => x.Number));
    }

    [Fact]
    public async Task Rain_SentenceAndZoneTable_CountWoredas()
    {
        var report = await Narrative();
        var rain = report.Sections.Single(x => x.Number == "1");

        var sentence = rain.Blocks.First(x => x.Kind == NarrativeBlockKind.Paragraph).Text!;
        Assert.Contains("ዝናብ ሙሉ በሙሉ ያገኙ 1 ወረዳዎች", sentence);
        Assert.Contains("በከፊል ያገኙ 1 ወረዳዎች", sentence);
        Assert.Contains("ዝናብ ያላገኙ 1 ወረዳዎች", sentence);
        Assert.Contains("መረጃ ያልተገኘላቸው ደግሞ 1 ወረዳዎች", sentence);   // በየዳ sent nothing
        Assert.Contains("በድምሩ 4 ወረዳዎች", sentence);

        var table = rain.Blocks.Single(x => x.Kind == NarrativeBlockKind.Table);
        Assert.Equal(["1", "ማዕከላዊ ጎንደር", "", "", "1", "", "1"], table.Rows![0]);
        Assert.Equal(["2", "ሰሜን ጎንደር", "1", "1", "", "1", "3"], table.Rows![1]);
        Assert.Equal(["", "ድምር", "1", "1", "1", "1", "4"], table.Footer);
    }

    [Fact]
    public async Task Incidents_AreBulletedWithPlace_AndOtherZonesGetTheClosingSentence()
    {
        AddHumanWater(Woreda("_b1"), problem: true, notes: "ምንጩ ደርቋል");
        AddHumanWater(Woreda("_a1"), problem: false);
        var report = await Narrative();
        var water = report.Sections.Single(x => x.Key == "human-water");

        var bullets = water.Blocks.Single(x => x.Kind == NarrativeBlockKind.Bullets).Items!;
        Assert.Equal("ማዕከላዊ ጎንደር ዞን፣ ደምቢያ ወረዳ፡ የመጠጥ ውሃ እጥረት አለ፤ ምንጩ ደርቋል", Assert.Single(bullets));
        Assert.Equal("በሌሎች መረጃ በተገኘባቸው ዞኖች የሰው መጠጥ ውሃ እጥረት የለም።", water.Blocks.Last().Text);
        Assert.Equal(1, water.IncidentCount);
    }

    [Fact]
    public async Task NoIncidentAnywhere_SaysSoForTheWholeRegion()
    {
        AddHumanWater(Woreda("_a1"), problem: false);
        var report = await Narrative();
        var water = report.Sections.Single(x => x.Key == "human-water");

        Assert.Equal("በአማራ ክልል የሰው መጠጥ ውሃ እጥረት የለም።", Assert.Single(water.Blocks).Text);
    }

    [Fact]
    public async Task PerZone_SplitsAWoredaGridIntoOneReportPerZone()
    {
        var grid = await _data.BuildGrid(Woreda("_region"), woredaColumns: true);
        var reports = WeeklyNarrativeReportBuilder.BuildPerZone(grid);

        Assert.Equal(["ማዕከላዊ ጎንደር", "ሰሜን ጎንደር"], reports.Select(x => x.ScopeName));
        var north = reports[1];
        Assert.Equal("የሰሜን ጎንደር ዞን አደጋ ስጋት ስራ አመራር ኮሚሽን ተጠሪ ጽ/ቤት", north.Organization);
        Assert.Equal((2, 3), (north.WoredasReported, north.WoredasInScope));
        // Woreda rows in the zone's rain table, and the zone's own average price (ደባርቅ 10,000, ጃናሞራ 11,000).
        Assert.Equal(new[] { "ደባርቅ", "ጃናሞራ", "በየዳ" }.Order(), north.RainRows.Select(x => x.Unit).Order());
        var prices = north.Sections.Single(x => x.Key == "prices").Blocks.First(x => x.Kind == NarrativeBlockKind.Table);
        Assert.Equal(["ጤፍ", "10,400", "10,500", "+1%"], prices.Rows![0]);
    }

    // ------------------------------------------------------------------ helpers

    private async Task<WeeklyNarrativeReport> Narrative()
    {
        var grid = await _data.BuildGrid(Woreda("_region"));
        return WeeklyNarrativeReportBuilder.Build(grid, "አማራ", ReportScopeLevel.Region);
    }

    private Locations Woreda(string field) =>
        (Locations)typeof(WeeklyMatrixReportTests).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(_data)!;

    private void AddHumanWater(Locations woreda, bool problem, string? notes = null)
    {
        _data.Db.HumanDrinkWaterIssues.Add(new HumanDrinkWaterIssue
        {
            HumanDrinkWaterIssueId = Guid.NewGuid(), WoredaId = woreda.Id, StartDate = WeeklyMatrixReportTests.WeekStart,
            EndDate = WeeklyMatrixReportTests.WeekEnd, ReportDate = WeeklyMatrixReportTests.WeekEnd, HasWaterProblem = problem, AdditionalNotes = notes
        });
        _data.Db.SaveChanges();
    }
}
