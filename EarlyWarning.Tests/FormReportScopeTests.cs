using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Tests;

/// <summary>
/// The form report must show exactly the data inside the signed-in user's administrative boundary:
/// woreda → own woreda, zone → its woredas, region → all its zones' woredas. Numbers are summed once per
/// woreda; text is concatenated per zone for zone/region reports.
/// </summary>
public class FormReportScopeTests : IDisposable
{
    private static readonly DateTime WeekStart = new(2026, 10, 5);
    private static readonly DateTime WeekEnd = new(2026, 10, 11);
    private static readonly DateTime DraftWeekStart = new(2026, 10, 12);
    private static readonly DateTime DraftWeekEnd = new(2026, 10, 18);

    private readonly EarlyWarningDbContext _db;
    private readonly Locations _region, _otherRegion, _zone1, _zone2, _otherZone, _emptyZone, _w1, _w2, _w3, _w4, _kebele;

    public FormReportScopeTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"form-report-{Guid.NewGuid()}").Options);

        _region = Location("አማራ", LocationLevel.ክልል, null);
        _otherRegion = Location("ትግራይ", LocationLevel.ክልል, null);
        _zone1 = Location("ሰሜን ጎንደር", LocationLevel.ዞን, _region);
        _zone2 = Location("ማዕከላዊ ጎንደር", LocationLevel.ዞን, _region);
        _otherZone = Location("ማዕከላዊ ትግራይ", LocationLevel.ዞን, _otherRegion);
        _emptyZone = Location("ባዶ ዞን", LocationLevel.ዞን, _region);
        _w1 = Location("ደባርቅ", LocationLevel.ወረዳ, _zone1);
        _w2 = Location("ጃናሞራ", LocationLevel.ወረዳ, _zone1);
        _w3 = Location("ጎንደር ዙሪያ", LocationLevel.ወረዳ, _zone2);
        _w4 = Location("አክሱም", LocationLevel.ወረዳ, _otherZone);
        _kebele = Location("ቀበሌ 01", LocationLevel.ቀበሌ, _w1);

        // Week under test: every woreda reported. W1 saved twice (an older duplicate must be ignored).
        Submission(_w1, WeekStart, WeekEnd, WeeklySubmissionStatus.SubmittedToZone, lastAction: new DateTime(2026, 10, 12));
        Submission(_w1, WeekStart, WeekEnd, WeeklySubmissionStatus.SubmittedToZone, lastAction: new DateTime(2026, 10, 10));
        Submission(_w2, WeekStart, WeekEnd, WeeklySubmissionStatus.ZoneApproved);
        Submission(_w3, WeekStart, WeekEnd, WeeklySubmissionStatus.RegionApproved);
        Submission(_w4, WeekStart, WeekEnd, WeeklySubmissionStatus.SubmittedToZone);
        // A later week that W2 has only drafted: visible to W2 itself, never to zone/region users.
        Submission(_w2, DraftWeekStart, DraftWeekEnd, WeeklySubmissionStatus.Draft);

        Pasture(_w1, 10, "W1 note", modified: new DateTime(2026, 10, 12));
        Pasture(_w1, 999, "W1 stale duplicate", modified: new DateTime(2026, 10, 6));
        Pasture(_w2, 20, "W2 note");
        Pasture(_w3, 40, "W3 note");
        Pasture(_w4, 80, "W4 note");
        Pasture(_w2, 1000, "W2 draft week", start: DraftWeekStart, end: DraftWeekEnd);

        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    // ------------------------------------------------------------------ woreda user

    [Fact]
    public async Task WoredaUser_SeesOnlyOwnWoreda_WithoutDoubleCounting()
    {
        var (scope, sections) = await BuildFor(User(_w1));

        Assert.Equal(ReportScopeLevel.Woreda, scope.Level);
        Assert.Equal([_w1.Id], scope.WoredaIds);
        Assert.Equal("10", AffectedAnimals(sections)); // not 30/70/80, and not 1009 from the stale duplicate
        var text = Assert.Single(PastureNotes(sections));
        Assert.Equal("", text.Group);                   // woreda reports show plain text, no zone/woreda prefix
        Assert.Equal("W1 note", text.Text);
    }

    [Fact]
    public async Task WoredaUser_SeesOwnDraftWeek()
    {
        var scopes = new ReportScopeService(_db);
        var scope = (await scopes.ResolveAsync(User(_w2))).Scope!;

        var weeks = (await scopes.GetSubmissionsAsync(scope)).Select(x => x.StartDate).Distinct().ToList();

        Assert.Contains(DraftWeekStart, weeks);
    }

    [Fact]
    public async Task RoleDoesNotWidenScope_RegionAdminAssignedToWoredaSeesOnlyThatWoreda()
    {
        // Real data contains a RegionAdmin assigned to a woreda; the assigned location decides the boundary.
        var user = User(_w3);
        user.Role = "RegionAdmin";
        user.UserLocationLevl = LocationLevel.ክልል; // stale profile level must not matter either

        var (scope, sections) = await BuildFor(user);

        Assert.Equal(ReportScopeLevel.Woreda, scope.Level);
        Assert.Equal("40", AffectedAnimals(sections));
    }

    // ------------------------------------------------------------------ zone user

    [Fact]
    public async Task ZoneUser_SumsOwnZonesWoredas_AndConcatenatesTextUnderZone()
    {
        var (scope, sections) = await BuildFor(User(_zone1));

        Assert.Equal(ReportScopeLevel.Zone, scope.Level);
        Assert.Equal(new[] { _w1.Id, _w2.Id }.Order(), scope.WoredaIds.Order());
        Assert.Equal("30", AffectedAnimals(sections)); // W1 (10) + W2 (20)

        var zoneText = Assert.Single(PastureNotes(sections));
        Assert.Equal("ሰሜን ጎንደር ዞን", zoneText.Group);
        Assert.Contains("ደባርቅ፡ W1 note", zoneText.Text);
        Assert.Contains("ጃናሞራ፡ W2 note", zoneText.Text);
        Assert.DoesNotContain("W3", zoneText.Text);
        Assert.DoesNotContain("stale", zoneText.Text);
    }

    [Fact]
    public async Task ZoneUser_NeverSeesDrafts()
    {
        var scopes = new ReportScopeService(_db);
        var scope = (await scopes.ResolveAsync(User(_zone1))).Scope!;

        var submissions = await scopes.GetSubmissionsAsync(scope);

        Assert.DoesNotContain(submissions, x => x.Status == WeeklySubmissionStatus.Draft);
        Assert.DoesNotContain(submissions, x => x.StartDate == DraftWeekStart);
    }

    [Fact]
    public async Task ZoneWithNoWoredas_ProducesEmptyScopeAndNoData()
    {
        var scopes = new ReportScopeService(_db);
        var scope = (await scopes.ResolveAsync(User(_emptyZone))).Scope!;

        Assert.Empty(scope.WoredaIds);
        Assert.Empty(await scopes.GetSubmissionsAsync(scope));
        Assert.Empty(await new WeeklyFormReportService(_db).BuildAsync([], scope.Level));
    }

    // ------------------------------------------------------------------ region user

    [Fact]
    public async Task RegionUser_SumsAllZonesInRegion_AndGroupsTextByZone()
    {
        var (scope, sections) = await BuildFor(User(_region));

        Assert.Equal(ReportScopeLevel.Region, scope.Level);
        Assert.Equal(new[] { _w1.Id, _w2.Id, _w3.Id }.Order(), scope.WoredaIds.Order());
        Assert.Equal("70", AffectedAnimals(sections)); // 10 + 20 + 40; W4 belongs to another region

        var texts = PastureNotes(sections);
        Assert.Equal(["ማዕከላዊ ጎንደር ዞን", "ሰሜን ጎንደር ዞን"], texts.Select(x => x.Group).Order());
        Assert.DoesNotContain(texts, x => x.Text.Contains("W4"));
    }

    [Fact]
    public async Task RegionUser_CannotReachAnotherRegionsWoredas()
    {
        var scopes = new ReportScopeService(_db);
        var scope = (await scopes.ResolveAsync(User(_region))).Scope!;

        var submissions = await scopes.GetSubmissionsAsync(scope);

        Assert.DoesNotContain(submissions, x => x.WoredaId == _w4.Id);
        Assert.Single(submissions, x => x.WoredaId == _w1.Id && x.StartDate == WeekStart); // duplicate collapsed
    }

    // ------------------------------------------------------------------ missing / unsupported locations

    [Fact]
    public async Task UserWithoutLocation_GetsErrorAndNoScope()
    {
        var result = await new ReportScopeService(_db).ResolveAsync(new ApplicationUser { LocationId = Guid.Empty });

        Assert.Null(result.Scope);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public async Task UserWithUnknownLocation_GetsErrorAndNoScope()
    {
        var result = await new ReportScopeService(_db).ResolveAsync(new ApplicationUser { LocationId = Guid.NewGuid() });

        Assert.Null(result.Scope);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public async Task KebeleUser_IsNotGivenAWoredaOrHigherScope()
    {
        var result = await new ReportScopeService(_db).ResolveAsync(User(_kebele));

        Assert.Null(result.Scope);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    // ------------------------------------------------------------------ helpers

    private async Task<(ReportScope Scope, List<FormReportSection> Sections)> BuildFor(ApplicationUser user)
    {
        var scopes = new ReportScopeService(_db);
        var resolved = await scopes.ResolveAsync(user);
        Assert.Null(resolved.Error);
        var scope = resolved.Scope!;
        var week = (await scopes.GetSubmissionsAsync(scope))
            .Where(x => x.StartDate == WeekStart && x.EndDate == WeekEnd).ToList();
        return (scope, await new WeeklyFormReportService(_db).BuildAsync(week, scope.Level));
    }

    private static string? AffectedAnimals(List<FormReportSection> sections) =>
        sections.Single(x => x.Number == "4").Items
            .First(x => x.Kind == FormReportItemKind.Line && x.Label == "ዝቅተኛ ከሆነ የተጎዳ የእንስሳት ቁጥር").Value;

    // The first "ማስታወሻ" in section 4 belongs to 4.1 (pasture).
    private static List<FormReportZoneText> PastureNotes(List<FormReportSection> sections) =>
        sections.Single(x => x.Number == "4").Items
            .First(x => x.Kind == FormReportItemKind.ZoneText && x.Label == "ማስታወሻ").ZoneTexts;

    private static ApplicationUser User(Locations location) =>
        new() { Id = Guid.NewGuid().ToString(), FirstName = "Test", LastName = "User", LocationId = location.Id, UserLocationLevl = location.Level!.Value };

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

    private void Submission(Locations woreda, DateTime start, DateTime end, WeeklySubmissionStatus status, DateTime? lastAction = null) =>
        _db.WeeklyReportSubmissions.Add(new WeeklyReportSubmission
        {
            WoredaId = woreda.Id, StartDate = start, EndDate = end, ReportingWeekStart = start, Status = status,
            CreatedById = "seed", CreatedAt = start, LastActionAt = lastAction ?? end, RowVersion = new byte[8]
        });

    private void Pasture(Locations woreda, int animals, string remarks, DateTime? modified = null, DateTime? start = null, DateTime? end = null) =>
        _db.PastureStatuses.Add(new PastureStatus
        {
            Id = Guid.NewGuid(), WoredaId = woreda.Id, StartDate = start ?? WeekStart, EndDate = end ?? WeekEnd,
            Enough = false, NumberOfAnimalsAffected = animals, Remarks = remarks, LasModifiedAt = modified ?? WeekEnd
        });
}
