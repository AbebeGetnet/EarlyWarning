using System.Text.Json;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.Models.Migration;
using EarlyWarning.Models.WeeklyAccidents;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Tests;

/// <summary>
/// Beneficiary tables: household heads + family members are the people counted; children, youth, elderly
/// (age) and disabled (health status) describe those same people and never increase the count.
/// Sample, per woreda (male / female):
///   household heads 40 / 30, family 100 / 80            → primary 140 / 110 = 250
///   children 20 / 25, youth 30 / 20, elderly 10 / 5     → by age 60 / 50 (within 140 / 110)
///   disabled 5 / 4                                      → within 140 / 110
/// The old rule added every row: 140+60+5 + 110+50+4 = 369 per woreda.
/// </summary>
public class BeneficiaryTotalsTests : IDisposable
{
    private static readonly DateTime WeekStart = new(2026, 10, 5);
    private static readonly DateTime WeekEnd = new(2026, 10, 11);
    private static readonly Beneficiaries Sample = new(40, 30, 100, 80, 20, 25, 30, 20, 10, 5, 5, 4);

    private readonly EarlyWarningDbContext _db;
    private readonly Locations _zoneA, _zoneB, _a1, _a2, _b1;

    public BeneficiaryTotalsTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"beneficiaries-{Guid.NewGuid()}").Options);
        var region = Location("ክልል", LocationLevel.ክልል, null);
        _zoneA = Location("ዞን ሀ", LocationLevel.ዞን, region);
        _zoneB = Location("ዞን ለ", LocationLevel.ዞን, region);
        _a1 = Location("ወረዳ A1", LocationLevel.ወረዳ, _zoneA);
        _a2 = Location("ወረዳ A2", LocationLevel.ወረዳ, _zoneA);
        _b1 = Location("ወረዳ B1", LocationLevel.ወረዳ, _zoneB);
        foreach (var woreda in new[] { _a1, _a2, _b1 }) Seed(woreda);
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    // ------------------------------------------------------------------ the rule

    [Fact]
    public void Totals_CountPrimaryRowsOnly_MaleAndFemaleSeparately()
    {
        Assert.Equal(140, Sample.Male);
        Assert.Equal(110, Sample.Female);
        Assert.Equal(250, Sample.Total);
        Assert.Equal(60, Sample.MaleByAge);
        Assert.Equal(50, Sample.FemaleByAge);
        Assert.Equal(250, Beneficiaries.Count(40, 30, 100, 80));
        Assert.Empty(Sample.Problems());
    }

    [Theory]
    [InlineData(typeof(HumanDrinkWaterIssue))] // MaleHouseholdHeads … naming
    [InlineData(typeof(PastureStatus))]        // MaleHouseHold / ChildhMale … naming
    public void From_ReadsEveryTableNaming(Type type)
    {
        var record = Activator.CreateInstance(type)!;
        Fill(record, Sample);
        Assert.Equal(Sample, Beneficiaries.From(record));
    }

    [Fact]
    public void From_AccidentHasNoFamilyRow_SoHouseholdsAreThePrimary()
    {
        var accident = new AccidentDetail { AffectedHouseholdsMale = 12, AffectedHouseholdsFemale = 8, AffectedChildrenMale = 5, AffectedDisabledFemale = 2 };
        var people = Beneficiaries.From(accident)!.Value;
        Assert.Equal(20, people.Total);
        Assert.Empty(people.Problems());
    }

    [Fact]
    public void Problems_AreReportedPerSex_ForAgeGroupsAndDisabled()
    {
        // Male: 10 + 5 primary, children+youth+elderly 9+4+3 = 16 > 15. Female: disabled 7 > 6. Male disabled 15 = 15 is allowed.
        var people = new Beneficiaries(10, 6, 5, 0, 9, 0, 4, 0, 3, 0, 15, 7);
        var problems = people.Problems().ToList();
        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, x => x.StartsWith("ወንድ") && x.Contains("(16)") && x.Contains("(15)"));
        Assert.Contains(problems, x => x.StartsWith("ሴት") && x.Contains("አካል ጉዳተኞች (7)") && x.Contains("(6)"));
    }

    [Fact]
    public void Problems_AllowDistributionsBelowThePrimaryTotal_AndZeros()
    {
        Assert.Empty(new Beneficiaries(5, 5, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0).Problems());
        Assert.Empty(default(Beneficiaries).Problems());
    }

    [Fact]
    public void ValidationFilter_FindsTablesInViewModelsAndLists()
    {
        var state = new ModelStateDictionary();
        var model = new RegistrationWithardViewModel
        {
            HumanDrinkWaterIssue = new HumanDrinkWaterIssue { MaleHouseholdHeads = 1, MaleChildren = 3 },
            DeathDetails = [new DeathDetail { FemaleFamilyMembers = 2, FemaleDisabled = 2 }, new DeathDetail { FemaleFamilyMembers = 1, FemaleElderly = 2 }]
        };
        BeneficiaryValidationFilter.Check(state, "model", model, depth: 0);
        Assert.True(state.ContainsKey(nameof(model.HumanDrinkWaterIssue)));
        Assert.False(state.ContainsKey("DeathDetails[0]")); // disabled 2 = primary 2: allowed
        Assert.True(state.ContainsKey("DeathDetails[1]"));
        Assert.Equal(2, state.ErrorCount);
    }

    // ------------------------------------------------------------------ aggregation across levels

    [Fact]
    public async Task Aggregate_PeopleMetrics_ExcludeDistributions_AtWoredaZoneAndRegion()
    {
        var service = new WeeklyAggregationService(_db);
        foreach (var (woredas, expected) in new[] { (new[] { _a1 }, 250m), (new[] { _a1, _a2 }, 500m), (new[] { _a1, _a2, _b1 }, 750m) })
        {
            var aggregate = await service.BuildAsync(await Reports(woredas));
            Assert.Equal(expected, Metric(aggregate, "5.", "በመጠጥ ውሃ የተጎዳ ማህበረሰብ"));
            Assert.Equal(expected, Metric(aggregate, "10.", "የዕለት እርዳታ ተጠቃሚ"));
            Assert.Equal(expected, Metric(aggregate, "11.", "በጉዳቱ የተጎዳ ማህበረሰብ"));
            Assert.Equal(expected, Metric(aggregate, "በተለያዩ ምክንያት ስደት", "የተሰደዱ ማህበረሰብ"));
            Assert.Equal(expected, Metric(aggregate, "የሞተ ካለ", "የሞተ ማህበረሰብ"));
            // Accidents have households only (12 + 8 per woreda); the children/disabled rows are not added.
            Assert.Equal(expected / 250m * 20m, Metric(aggregate, "9.", "በእያንዳንዱ ጉዳት የተጎዳ ማህበረሰብ"));
        }
    }

    [Fact]
    public async Task Aggregate_RegionEqualsSumOfZones_WithoutDuplicatingBeneficiaries()
    {
        var service = new WeeklyAggregationService(_db);
        var zoneA = await service.BuildAsync(await Reports(_a1, _a2));
        var zoneB = await service.BuildAsync(await Reports(_b1));
        var region = await service.BuildAsync(await Reports(_a1, _a2, _b1));
        Assert.Equal(Metric(zoneA, "5.", "በመጠጥ ውሃ የተጎዳ ማህበረሰብ") + Metric(zoneB, "5.", "በመጠጥ ውሃ የተጎዳ ማህበረሰብ"),
            Metric(region, "5.", "በመጠጥ ውሃ የተጎዳ ማህበረሰብ"));
    }

    [Fact]
    public async Task FormReport_PeopleTotalsArePrimary_AndBreakdownsKeepTheirValues()
    {
        var sections = await new WeeklyFormReportService(_db).BuildAsync(await Reports(_a1, _a2, _b1), ReportScopeLevel.Region);
        var people = sections.Single(x => x.Number == "5").Items.Single(x => x.Kind == FormReportItemKind.People).People!;
        Assert.Equal(3 * 140, people.MaleTotal);
        Assert.Equal(3 * 110, people.FemaleTotal);
        Assert.Equal(750, people.Total);
        // The distributions are still reported, separately per sex.
        Assert.Equal(3 * 20, people.MaleChildren);
        Assert.Equal(3 * 25, people.FemaleChildren);
        Assert.Equal(3 * 4, people.FemaleDisabled);
    }

    [Fact]
    public async Task Snapshots_FromTheOldRule_AreNotServed()
    {
        var service = new WeeklyAggregationService(_db);
        var reports = await Reports(_a1);
        await service.SaveSnapshotAsync(_zoneA.Id, LocationLevel.ዞን, WeekStart, WeekEnd, WeeklySubmissionStatus.ZoneApproved, "u", reports);
        Assert.NotNull(await service.GetSnapshotAsync(_zoneA.Id, LocationLevel.ዞን, WeekStart, WeekEnd));

        var row = _db.WeeklyAggregateSnapshots.Single();
        var old = JsonSerializer.Deserialize<Dictionary<string, object>>(row.AggregateJson)!;
        old.Remove(nameof(WeeklyAggregateViewModel.CalculationVersion));
        old.Remove("calculationVersion");
        row.AggregateJson = JsonSerializer.Serialize(old);
        await _db.SaveChangesAsync();
        Assert.Null(await service.GetSnapshotAsync(_zoneA.Id, LocationLevel.ዞን, WeekStart, WeekEnd));
    }

    // ------------------------------------------------------------------ helpers

    private static decimal Metric(WeeklyAggregateViewModel aggregate, string sectionStart, string metric) =>
        aggregate.Sections.Where(x => x.Name.StartsWith(sectionStart)).SelectMany(x => x.Metrics).Single(x => x.Name == metric).Value;

    private async Task<List<WeeklyReportSubmission>> Reports(params Locations[] woredas)
    {
        var ids = woredas.Select(x => x.Id).ToList();
        return await _db.WeeklyReportSubmissions.Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .Where(x => ids.Contains(x.WoredaId)).ToListAsync();
    }

    private void Seed(Locations woreda)
    {
        _db.WeeklyReportSubmissions.Add(new WeeklyReportSubmission
        {
            WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportingWeekStart = WeekStart,
            Status = WeeklySubmissionStatus.RegionApproved, CreatedById = "seed", CreatedAt = WeekStart, LastActionAt = WeekEnd, RowVersion = new byte[8]
        });
        _db.HumanDrinkWaterIssues.Add(Filled(new HumanDrinkWaterIssue
        {
            HumanDrinkWaterIssueId = Guid.NewGuid(), WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd,
            HasWaterProblem = true, NumberOfAffectedKebeles = 1
        }));
        _db.AssistanceRecipients.Add(Filled(new AssistanceRecipient { WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd }));
        _db.OtherProblems.Add(Filled(new OtherProblem { WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd, HasOtherProblem = true, ProblemName = "x" }));
        var migration = new Migration { WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd, HasMigration = true };
        migration.MigrationDetails = [Filled(new MigrationDetail { IsInward = false, OriginLocationId = woreda.Id, DestinationLocationId = _b1.Id })];
        _db.Migrations.Add(migration);
        var death = new Death { WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd, HasDeath = true };
        death.DeathDetails = [Filled(new DeathDetail { DeathReason = "x" })];
        _db.Deaths.Add(death);
        _db.WeeklyAccidents.Add(new WeeklyAccidents
        {
            WoredaId = woreda.Id, StartDate = WeekStart, EndDate = WeekEnd, ReportDate = WeekEnd, HasAccident = true,
            AccidentDetails = [new AccidentDetail { AccidentTypeId = Guid.NewGuid(), AffectedHouseholdsMale = 12, AffectedHouseholdsFemale = 8, AffectedChildrenMale = 5, AffectedDisabledFemale = 2 }]
        });
    }

    private static T Filled<T>(T record) where T : notnull { Fill(record, Sample); return record; }

    // Writes the sample into whichever naming scheme the record uses.
    private static void Fill(object record, Beneficiaries b)
    {
        var values = new[] { b.MaleHouseholds, b.FemaleHouseholds, b.MaleFamily, b.FemaleFamily, b.MaleChildren, b.FemaleChildren,
            b.MaleYouth, b.FemaleYouth, b.MaleElderly, b.FemaleElderly, b.MaleDisabled, b.FemaleDisabled };
        string[][] schemes =
        [
            ["MaleHouseholdHeads", "FemaleHouseholdHeads", "MaleFamilyMembers", "FemaleFamilyMembers", "MaleChildren", "FemaleChildren", "MaleYouth", "FemaleYouth", "MaleElderly", "FemaleElderly", "MaleDisabled", "FemaleDisabled"],
            ["MaleHouseHold", "FemaleHouseHold", "MaleFamily", "FemaleFamily", "ChildhMale", "ChildFemale", "YouthMale", "YouthFemale", "ElderlyMale", "ElderlyFemale", "DisabledMale", "DisabledFemale"]
        ];
        var scheme = schemes.First(s => record.GetType().GetProperty(s[0]) != null);
        for (var i = 0; i < scheme.Length; i++) record.GetType().GetProperty(scheme[i])!.SetValue(record, (int)values[i]);
    }

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
