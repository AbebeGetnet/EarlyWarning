using System.Security.Claims;
using System.Text.Json;
using EarlyWarning.Controllers;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Models.AnimalPrice;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EarlyWarning.Tests;

/// <summary>
/// Registration wizard rules: high-rainfall and drought kebeles are mutually exclusive, crop areas follow
/// ploughed ≥ seeded ≥ harvested (ploughed may exceed the plan), and weekly prices compare only with the
/// immediately preceding reporting week (New when there is no history, Uncompared when only older weeks exist).
/// </summary>
public class RegistrationWizardRulesTests : IDisposable
{
    private static readonly DateTime WeekStart = new(2026, 10, 12); // Monday
    private static readonly DateTime WeekEnd = WeekStart.AddDays(2);

    private readonly EarlyWarningDbContext _db;
    private readonly Locations _woreda, _kebele1, _kebele2;
    private readonly ApplicationUser _encoder;

    public RegistrationWizardRulesTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"wizard-rules-{Guid.NewGuid()}").Options);
        _woreda = Location("ደባርቅ", LocationLevel.ወረዳ, null);
        _kebele1 = Location("ቀበሌ 01", LocationLevel.ቀበሌ, _woreda);
        _kebele2 = Location("ቀበሌ 02", LocationLevel.ቀበሌ, _woreda);
        _encoder = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(), UserName = "encoder", FirstName = "Data", LastName = "Encoder",
            LocationId = _woreda.Id, UserLocationLevl = LocationLevel.ወረዳ
        };
        _db.Users.Add(_encoder);
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task PreviousWeekPrices_UseOnlyTheImmediatelyPrecedingWeek()
    {
        var comparedGrain = Grain("ጤፍ");
        var olderOnlyGrain = Grain("ስንዴ");
        var newGrain = Grain("በቆሎ");
        GrainPrice(comparedGrain, WeekStart.AddDays(-7), 5000);
        GrainPrice(comparedGrain, WeekStart.AddDays(-21), 3000);
        GrainPrice(olderOnlyGrain, WeekStart.AddDays(-14), 4000);

        var comparedAnimal = Animal("በሬ");
        var olderOnlyAnimal = Animal("ፍየል");
        var newAnimal = Animal("በግ");
        AnimalPrice(comparedAnimal, WeekStart.AddDays(-7), 40000);
        AnimalPrice(comparedAnimal, WeekStart.AddDays(-28), 30000);
        AnimalPrice(olderOnlyAnimal, WeekStart.AddDays(-28), 9000);
        _db.SaveChanges();

        var result = Assert.IsType<JsonResult>(await Controller().GetPreviousWeekPrices(_woreda.Id, WeekStart));
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
        var grains = Rows(json, "grainPriceRows");
        var animals = Rows(json, "animalPriceRows");

        AssertCompared(grains[comparedGrain], 5000);
        AssertNotCompared(grains[olderOnlyGrain], WeeklyStatus.Uncompared);
        AssertNotCompared(grains[newGrain], WeeklyStatus.New);
        AssertCompared(animals[comparedAnimal], 40000);
        AssertNotCompared(animals[olderOnlyAnimal], WeeklyStatus.Uncompared);
        AssertNotCompared(animals[newAnimal], WeeklyStatus.New);
    }

    [Fact]
    public async Task SubmitWizard_RejectsKebeleSelectedAsBothHighRainfallAndDrought()
    {
        var controller = Controller();
        await controller.SubmitWizard(Model(), [_kebele1.Id], [_kebele1.Id, _kebele2.Id], [], []);

        Assert.True(controller.ModelState.TryGetValue("SelectedDroughtKebeleIds", out var entry));
        Assert.NotEmpty(entry!.Errors);
    }

    [Fact]
    public async Task SubmitWizard_AcceptsDistinctHighRainfallAndDroughtKebeles()
    {
        var model = Model();
        // An unrelated error (seeded > ploughed) keeps the submission from reaching persistence.
        model.FarmingActivity.MeherSown = 1;
        var controller = Controller();
        await controller.SubmitWizard(model, [_kebele1.Id], [_kebele2.Id], [], []);

        Assert.False(controller.ModelState.TryGetValue("SelectedDroughtKebeleIds", out var entry) && entry.Errors.Count > 0);
    }

    [Fact]
    public async Task SubmitWizard_AllowsPloughedAreaAbovePlan()
    {
        var model = Model();
        model.FarmingActivity = new FarmingActivity
        {
            MeherFarmPlan = 10, MeherPloughed = 20, MeherSown = 20, MeherHarvestingHHarvesting = 15,
            AutumnFarmPlan = 5, AutumnPloughed = 8, AutumnSown = 6, AutumnHarvestingHHarvesting = 6
        };
        var controller = Controller();
        // The overlapping kebele keeps the submission invalid so it stops before persisting.
        await controller.SubmitWizard(model, [_kebele1.Id], [_kebele1.Id], [], []);

        Assert.DoesNotContain(controller.ModelState, x => x.Key.StartsWith("FarmingActivity.") && x.Value!.Errors.Count > 0);
    }

    [Fact]
    public async Task SubmitWizard_RejectsSeededAbovePloughedAndHarvestedAboveSeeded()
    {
        var model = Model();
        model.FarmingActivity = new FarmingActivity
        {
            MeherFarmPlan = 100, MeherPloughed = 20, MeherSown = 25, MeherHarvestingHHarvesting = 30,
            AutumnFarmPlan = 100, AutumnPloughed = 10, AutumnSown = 12, AutumnHarvestingHHarvesting = 13
        };
        var controller = Controller();
        await controller.SubmitWizard(model, [], [], [], []);

        foreach (var key in new[] { "FarmingActivity.MeherSown", "FarmingActivity.MeherHarvestingHHarvesting",
                     "FarmingActivity.AutumnSown", "FarmingActivity.AutumnHarvestingHHarvesting" })
        {
            Assert.True(controller.ModelState.TryGetValue(key, out var entry), key);
            Assert.NotEmpty(entry!.Errors);
        }
    }

    [Fact]
    public async Task SubmitWizard_RequiresDamageFieldsForEverySelectedDisease()
    {
        var pest = new CropPestAndDesease { Id = Guid.NewGuid(), Name = "ተምች" };
        var filledPest = new CropPestAndDesease { Id = Guid.NewGuid(), Name = "ዋግ" };
        var disease = new AnimalDisease { Id = Guid.NewGuid(), Name = "አባ ሰንጋ" };
        _db.AddRange(pest, filledPest, disease);
        _db.SaveChanges();
        var model = Model();
        model.CropPestAndDeseaseReport.HasPestAndDeseasOccured = true;
        model.AnimalHealthStatus.Enough = false;
        model.CropPestImpactDetails =
        [
            new CropPestImpactDetail { CropPestAndDeseaseId = pest.Id },
            new CropPestImpactDetail { CropPestAndDeseaseId = filledPest.Id, AffectedLandInHectares = 3, DamagePercentage = 25 }
        ];
        model.AnimalDiseaseImpactDetails = [new AnimalDiseaseImpactDetail { AnimalDiseaseId = disease.Id }];
        var controller = Controller();
        await controller.SubmitWizard(model, [], [], [pest.Id, filledPest.Id], [disease.Id]);

        AssertError(controller, "CropPestImpactDetails[0].AffectedLandInHectares");
        AssertError(controller, "CropPestImpactDetails[0].DamagePercentage");
        AssertError(controller, "AnimalDiseaseImpactDetails[0].NumberOfAffectedAnimals");
        Assert.DoesNotContain(controller.ModelState, x => x.Key.StartsWith("CropPestImpactDetails[1]") && x.Value!.Errors.Count > 0);
    }

    [Fact]
    public async Task SubmitWizard_RequiresDescriptionWhenOtherDamageCategoryIsChosen()
    {
        var typeId = Guid.NewGuid();
        var model = Model();
        model.WeeklyAccidents.HasAccident = true;
        model.SelectedAccidentTypeIds = [typeId];
        model.AccidentDetails = [new Models.WeeklyAccidents.AccidentDetail { AccidentTypeId = typeId, SelectedDamageCategories = [AccidentDamageCategory.Other] }];
        var controller = Controller();
        await controller.SubmitWizard(model, [], [], [], []);

        AssertError(controller, "AccidentDetails[0].OtherDamageDescription");
    }

    private static void AssertError(RegisterationWizardController controller, string key)
    {
        Assert.True(controller.ModelState.TryGetValue(key, out var entry), key);
        Assert.NotEmpty(entry!.Errors);
    }

    private RegisterationWizardController Controller()
    {
        var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db), Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), [], [], new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(),
            null!, NullLogger<UserManager<ApplicationUser>>.Instance);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, _encoder.Id), new Claim(ClaimTypes.Role, "Data Encoder")], "test"));
        return new RegisterationWizardController(_db, userManager)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } }
        };
    }

    private static RegistrationWithardViewModel Model() => new()
    {
        StartDate = WeekStart, EndDate = WeekEnd, UserId = "",
        RainfallReport = new RainfallReport(), FarmingActivity = new FarmingActivity(),
        CropPestAndDeseaseReport = new CropPestAndDeseaseReport(), PastureStatus = new PastureStatus(),
        AnimalWaterSupplyStatus = new AnimalWaterSupplyStatus(), AnimalHealthStatus = new AnimalHealthStatus(),
        HumanDrinkWaterIssue = new HumanDrinkWaterIssue()
    };

    private static Dictionary<Guid, JsonElement> Rows(JsonDocument json, string name) =>
        json.RootElement.GetProperty(name).EnumerateArray().ToDictionary(x => x.GetProperty("id").GetGuid());

    private static void AssertCompared(JsonElement row, decimal previousPrice)
    {
        Assert.Equal(previousPrice, row.GetProperty("previousWeekPrice").GetDecimal());
        Assert.Equal(JsonValueKind.Null, row.GetProperty("noComparisonStatus").ValueKind);
    }

    private static void AssertNotCompared(JsonElement row, WeeklyStatus status)
    {
        Assert.Equal(JsonValueKind.Null, row.GetProperty("previousWeekPrice").ValueKind);
        Assert.Equal((int)status, row.GetProperty("noComparisonStatus").GetInt32());
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

    private Guid Grain(string name)
    {
        var grain = new GrainType { GrainTypeId = Guid.NewGuid(), GrainName = name };
        _db.GrainTypes.Add(grain);
        return grain.GrainTypeId;
    }

    private Guid Animal(string name)
    {
        var animal = new AnimalType { AnimalTypeId = Guid.NewGuid(), AnimalTypeName = name };
        _db.AnimalTypes.Add(animal);
        return animal.AnimalTypeId;
    }

    private void GrainPrice(Guid grainTypeId, DateTime start, decimal price) =>
        _db.GrainPricePerQuintals.Add(new GrainPricePerQuintal
        {
            GrainPricePerQuintalId = Guid.NewGuid(), WoredaId = _woreda.Id, GrainTypeId = grainTypeId,
            StartDate = start, EndDate = start.AddDays(2), WeeklyPrice = price
        });

    private void AnimalPrice(Guid animalTypeId, DateTime start, decimal price) =>
        _db.AnimalPricePerUnits.Add(new AnimalPricePerUnit
        {
            AnimalPricePerUnitId = Guid.NewGuid(), WoredaId = _woreda.Id, AnimalTypeId = animalTypeId,
            StartDate = start, EndDate = start.AddDays(2), WeeklyPrice = price
        });
}
