using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.Models.AnimalPrice;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Models.SupplyofGrain;
using EarlyWarning.Models.WeeklyAccidents;

namespace EarlyWarning.ViewModels;

/// <summary>Everything a woreda submitted for one reporting week, shown read-only on the approval details page.</summary>
public class WeeklySubmissionFormViewModel
{
    public RainfallReport? Rainfall { get; set; }
    public List<string> HighRainfallKebeles { get; set; } = [];
    public List<string> DroughtKebeles { get; set; } = [];
    public FarmingActivity? Farming { get; set; }
    public CropGrowth? CropGrowth { get; set; }
    public CropPestAndDeseaseReport? CropPest { get; set; }
    public PastureStatus? Pasture { get; set; }
    public AnimalWaterSupplyStatus? AnimalWater { get; set; }
    public AnimalHealthStatus? AnimalHealth { get; set; }
    public Models.HumanDrinkWaterIssue.HumanDrinkWaterIssue? HumanWater { get; set; }
    public List<string> HumanWaterKebeles { get; set; } = [];
    public Models.EpidemicDisease.EpidemicDisease? Epidemic { get; set; }
    public GrainSupply? GrainSupply { get; set; }
    public Models.AnimalSupply.AnimalSupply? AnimalSupply { get; set; }
    public GrainPrice? GrainPrice { get; set; }
    public Models.AnimalPrice.AnimalPrice? AnimalPrice { get; set; }
    public List<GrainPricePerQuintal> GrainPriceRows { get; set; } = [];
    public List<AnimalPricePerUnit> AnimalPriceRows { get; set; } = [];
    public WeeklyAccidents? Accidents { get; set; }
    public Models.Migration.Migration? Migration { get; set; }
    public Models.Migration.Death? Death { get; set; }
    public List<AnimalMigrationRecord> AnimalMigrations { get; set; } = [];
    public AssistanceRecipient? AssistanceRecipient { get; set; }
    public WeeklyProvided? Aid { get; set; }
    public OtherProblem? OtherProblem { get; set; }
    public SchoolReport? School { get; set; }
    public MalnutritionReport? Malnutrition { get; set; }
}
