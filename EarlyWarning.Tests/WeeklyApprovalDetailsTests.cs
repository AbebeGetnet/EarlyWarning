using System.Security.Claims;
using EarlyWarning.Controllers;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.Models.WeeklyAccidents;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EarlyWarning.Tests;

/// <summary>
/// Zone/Region decisions are only possible from the record's details page: the page shows the full submitted
/// form read-only and issues the token that the approve/reject action requires.
/// </summary>
public class WeeklyApprovalDetailsTests : IDisposable
{
    private static readonly DateTime WeekStart = new(2026, 10, 5);
    private static readonly byte[] RowVersion = [1, 2, 3];

    private readonly EarlyWarningDbContext _db;
    private readonly IDataProtectionProvider _protection = new EphemeralDataProtectionProvider();
    private readonly Locations _region, _zone, _woreda;
    private readonly ApplicationUser _encoder, _regionAdmin, _otherRegionAdmin;
    private readonly WeeklyReportSubmission _submission;

    public WeeklyApprovalDetailsTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"approval-details-{Guid.NewGuid()}").Options);
        _region = Location("አማራ", LocationLevel.ክልል, null);
        _zone = Location("ሰሜን ጎንደር", LocationLevel.ዞን, _region);
        _woreda = Location("ደባርቅ", LocationLevel.ወረዳ, _zone);
        _encoder = User("encoder", _woreda, LocationLevel.ወረዳ);
        _regionAdmin = User("region", _region, LocationLevel.ክልል);
        _otherRegionAdmin = User("region2", _region, LocationLevel.ክልል);
        _submission = new WeeklyReportSubmission
        {
            WoredaId = _woreda.Id, StartDate = WeekStart, EndDate = WeekStart.AddDays(2), ReportingWeekStart = WeekStart,
            Status = WeeklySubmissionStatus.ZoneApproved, CreatedById = _encoder.Id, CreatedAt = WeekStart, LastActionAt = WeekStart,
            RowVersion = RowVersion
        };
        _db.WeeklyReportSubmissions.Add(_submission);
        SeedSubmittedForm();
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Act_WithoutDetailsPageToken_DoesNotChangeTheReport()
    {
        var controller = Controller(_regionAdmin, "RegionAdmin");
        await controller.Act(_submission.Id, "Approve", null, RowVersion, approvalToken: null);
        await controller.Act(_submission.Id, "Approve", null, RowVersion, approvalToken: "forged-token");

        Assert.Equal(WeeklySubmissionStatus.ZoneApproved, await Status());
        Assert.Empty(_db.WeeklyReportWorkflowHistories);
    }

    [Fact]
    public async Task Act_WithTokenIssuedToAnotherUser_IsRejected()
    {
        var token = (await DetailsModel(_otherRegionAdmin, "RegionAdmin")).ApprovalToken;
        Assert.NotNull(token);

        await Controller(_regionAdmin, "RegionAdmin").Act(_submission.Id, "Approve", null, RowVersion, token);

        Assert.Equal(WeeklySubmissionStatus.ZoneApproved, await Status());
    }

    [Fact]
    public async Task RegionApproval_FromDetailsPage_ApprovesTheReport()
    {
        var details = await DetailsModel(_regionAdmin, "RegionAdmin");
        Assert.True(details.CanRegionApprove);
        Assert.False(details.CanZoneApprove);

        await Controller(_regionAdmin, "RegionAdmin").Act(_submission.Id, "Approve", "ok", RowVersion, details.ApprovalToken);

        Assert.Equal(WeeklySubmissionStatus.RegionApproved, await Status());
        var history = Assert.Single(_db.WeeklyReportWorkflowHistories);
        Assert.Equal(WeeklyWorkflowAction.RegionApprove, history.Action);
    }

    [Fact]
    public async Task DetailsPage_ForDataEncoder_HasNoApprovalControls()
    {
        var details = await DetailsModel(_encoder, "Data Encoder");

        Assert.False(details.CanZoneApprove || details.CanRegionApprove);
        Assert.Null(details.ApprovalToken);
    }

    [Fact]
    public async Task DetailsPage_ShowsTheSubmittedFormPrefilled()
    {
        var form = (await DetailsModel(_regionAdmin, "RegionAdmin")).Form;

        var crop = Assert.Single(form.CropPest!.ImpactDetails);
        Assert.Equal(12.5m, crop.AffectedLandInHectares);
        Assert.Equal(40m, crop.DamagePercentage);
        Assert.Equal(["ኬሚካል ርጭት", "በእጅ ማስወገድ"], form.CropPest.DefensiveActivities.Select(x => x.Activity).Order().Reverse().ToList());
        Assert.Equal(30, Assert.Single(form.AnimalHealth!.ImpactDetails).NumberOfAffectedAnimals);
        var accident = Assert.Single(form.Accidents!.AccidentDetails!);
        Assert.Equal([AccidentDamageCategory.Human, AccidentDamageCategory.House], accident.SelectedDamageCategories);
        Assert.Equal([AidCategory.Food, AidCategory.NonFood],
            form.Aid!.WeeklyProvidedDetail!.Select(x => x.SupplyType!.Category).Order().ToList());
    }

    [Fact]
    public void DamageCategories_RoundTripBetweenCheckboxListAndFlags()
    {
        var detail = new AccidentDetail { SelectedDamageCategories = [AccidentDamageCategory.Animal, AccidentDamageCategory.Other] };

        Assert.Equal(AccidentDamageCategory.Animal | AccidentDamageCategory.Other, detail.DamageCategories);
        Assert.Equal([AccidentDamageCategory.Animal, AccidentDamageCategory.Other], detail.SelectedDamageCategories);
    }

    private async Task<WeeklyWorkflowDetailsViewModel> DetailsModel(ApplicationUser user, string role)
    {
        var result = Assert.IsType<ViewResult>(await Controller(user, role).Details(_submission.Id));
        return Assert.IsType<WeeklyWorkflowDetailsViewModel>(result.Model);
    }

    private async Task<WeeklySubmissionStatus> Status() =>
        (await _db.WeeklyReportSubmissions.AsNoTracking().SingleAsync(x => x.Id == _submission.Id)).Status;

    private WeeklyWorkflowController Controller(ApplicationUser user, string role)
    {
        var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db), Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), [], [], new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(),
            null!, NullLogger<UserManager<ApplicationUser>>.Instance);
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Role, role)], "test"))
        };
        return new WeeklyWorkflowController(_db, userManager, new WeeklyAggregationService(_db), new WeeklySubmissionFormLoader(_db), _protection)
        {
            ControllerContext = new ControllerContext { HttpContext = http },
            TempData = new TempDataDictionary(http, new NullTempDataProvider())
        };
    }

    private void SeedSubmittedForm()
    {
        var end = WeekStart.AddDays(2);
        var pest = new CropPestAndDesease { Id = Guid.NewGuid(), Name = "ተምች" };
        var disease = new AnimalDisease { Id = Guid.NewGuid(), Name = "አባ ሰንጋ" };
        var accidentType = new AccidentType { AccidentTypeId = Guid.NewGuid(), AccidentName = "ጎርፍ" };
        var food = new SupplyType { SupplyTypeId = Guid.NewGuid(), SupplyTypeName = "ስንዴ", Category = AidCategory.Food };
        var nonFood = new SupplyType { SupplyTypeId = Guid.NewGuid(), SupplyTypeName = "ብርድ ልብስ", Category = AidCategory.NonFood };
        _db.AddRange(pest, disease, accidentType, food, nonFood);

        var cropReport = new CropPestAndDeseaseReport { Id = Guid.NewGuid(), WoredaId = _woreda.Id, StartDate = WeekStart, EndDate = end, HasPestAndDeseasOccured = true };
        cropReport.ImpactDetails.Add(new CropPestImpactDetail { Id = Guid.NewGuid(), CropPestAndDeseaseId = pest.Id, AffectedLandInHectares = 12.5m, DamagePercentage = 40m });
        cropReport.DefensiveActivities.Add(new CropDefensiveActivity { Id = Guid.NewGuid(), Activity = "ኬሚካል ርጭት", AreaCoveredInHectares = 10 });
        cropReport.DefensiveActivities.Add(new CropDefensiveActivity { Id = Guid.NewGuid(), Activity = "በእጅ ማስወገድ" });
        _db.CropPestAndDeseaseReports.Add(cropReport);

        var health = new AnimalHealthStatus { Id = Guid.NewGuid(), WoredaId = _woreda.Id, StartDate = WeekStart, EndDate = end, Enough = false };
        health.ImpactDetails.Add(new AnimalDiseaseImpactDetail { Id = Guid.NewGuid(), AnimalDiseaseId = disease.Id, NumberOfAffectedAnimals = 30 });
        _db.AnimalHealthStatuses.Add(health);

        _db.WeeklyAccidents.Add(new WeeklyAccidents
        {
            WeeklyAccidentsId = Guid.NewGuid(), WoredaId = _woreda.Id, StartDate = WeekStart, EndDate = end, HasAccident = true,
            AccidentDetails = [new AccidentDetail { AccidentDetailId = Guid.NewGuid(), AccidentTypeId = accidentType.AccidentTypeId, DamageCategories = AccidentDamageCategory.Human | AccidentDamageCategory.House }]
        });
        _db.WeeklyProvideds.Add(new WeeklyProvided
        {
            WeeklyProvidedId = Guid.NewGuid(), WoredaId = _woreda.Id, StartDate = WeekStart, EndDate = end,
            WeeklyProvidedDetail =
            [
                new WeeklyProvidedDetail { WeeklyProvidedDetailId = Guid.NewGuid(), SupplyTypeId = food.SupplyTypeId, ProvidedQuantity = 10, DistributedQuantity = 5 },
                new WeeklyProvidedDetail { WeeklyProvidedDetailId = Guid.NewGuid(), SupplyTypeId = nonFood.SupplyTypeId, ProvidedQuantity = 4, DistributedQuantity = 4 }
            ]
        });
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

    private ApplicationUser User(string name, Locations location, LocationLevel level)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(), UserName = name, FirstName = name, LastName = "Test",
            LocationId = location.Id, UserLocationLevl = level
        };
        _db.Users.Add(user);
        return user;
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
