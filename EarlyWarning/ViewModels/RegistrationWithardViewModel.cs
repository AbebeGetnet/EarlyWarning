using EarlyWarning.Models;

using EarlyWarning.Models.Aid;
using EarlyWarning.Models.AnimalPrice;
using EarlyWarning.Models.AnimalSupply;
using EarlyWarning.Models.EpidemicDisease;
using EarlyWarning.Models.Migration;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Models.SupplyofGrain;
using EarlyWarning.Models.WeeklyAccidents;
using EarlyWarning.Models.HumanDrinkWaterIssue;

namespace EarlyWarning.ViewModels
{
    public class RegistrationWithardViewModel
    {
        // Shared across all reports
        public Guid WoredaId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; }
        public string? Remarks { get; set; }

        public RainfallReport RainfallReport { get; set; }
        public FarmingActivity FarmingActivity { get; set; }
        public CropPestAndDeseaseReport CropPestAndDeseaseReport { get; set; }
        public PastureStatus PastureStatus { get; set; }
        public AnimalWaterSupplyStatus AnimalWaterSupplyStatus { get; set; }
        public AnimalHealthStatus AnimalHealthStatus { get; set; }
        public EpidemicDisease EpidemicDisease { get; set; } = new();
        public GrainSupply GrainSupply { get; set; } = new();
        public AnimalSupply AnimalSupply { get; set; } = new();
        public GrainPrice GrainPrice { get; set; } = new();
        public AnimalPrice AnimalPrice { get; set; } = new();
        public WeeklyAccidents WeeklyAccidents { get; set; } = new();
        public AccidentDetail AccidentDetail { get; set; } = new();
        public Migration Migration { get; set; } = new();
        public MigrationDetail MigrationDetail { get; set; } = new();
        public Death Death { get; set; } = new();
        public DeathDetail DeathDetail { get; set; } = new();
        public AssistanceRecipient AssistanceRecipient { get; set; } = new();
        public WeeklyProvided WeeklyProvided { get; set; } = new();
        public OtherProblem OtherProblem { get; set; } = new();
        public HumanDrinkWaterIssue HumanDrinkWaterIssue { get; set; } = new();
        public SchoolReport SchoolReport { get; set; } = new();
        public List<GrainPricePerQuintal> GrainPriceRows { get; set; } = new();
        public List<AnimalPricePerUnit> AnimalPriceRows { get; set; } = new();
        public List<AccidentDetail> AccidentDetails { get; set; } = new();
        public List<MigrationDetail> MigrationDetails { get; set; } = new();
        public List<DeathDetail> DeathDetails { get; set; } = new();
        public List<WeeklyProvidedDetail> AidDistributionDetails { get; set; } = new();
        public List<CropPestImpactDetail> CropPestImpactDetails { get; set; } = new();
        public List<AnimalDiseaseImpactDetail> AnimalDiseaseImpactDetails { get; set; } = new();
        public List<EpidemicDiseaseSelection> EpidemicImpactDetails { get; set; } = new();

        // Disease selection lists (populated from DB)
        public List<Locations> Kebelies { get; set; } = new List<Locations>();
        public List<AnimalDisease> AnimalDiseases { get; set; } = new List<AnimalDisease>();
        public List<CropPestAndDesease> CropPestAndDeseases { get; set; } = new List<CropPestAndDesease>();
        public List<EpidemicDiseaseType> EpidemicDiseaseTypes { get; set; } = new();
        public List<AccidentType> AccidentTypes { get; set; } = new();
        public List<GrainType> GrainTypes { get; set; } = new();
        public List<AnimalType> AnimalTypes { get; set; } = new();
        public List<SupplyType> SupplyTypes { get; set; } = new();
        public List<Locations> MigrationLocations { get; set; } = new();
        public EarlyWarning.Enums.AnimalMigrationDirection AnimalMigrationDirection { get; set; }
        public Guid? InwardAnimalMigrationOriginId { get; set; }
        public Guid? OutwardAnimalMigrationDestinationId { get; set; }
        public List<AnimalMigrationEntryViewModel> AnimalMigrationRows { get; set; } = new();

        // Selected IDs for crop and animal diseases
        public List<Guid> SelectedKebeliesIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedCropDiseaseIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedAnimalDiseaseIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedEpidemicDiseaseTypeIds { get; set; } = new();
        public List<Guid> SelectedAccidentTypeIds { get; set; } = new();
        public List<Guid> SelectedHumanWaterKebeleIds { get; set; } = new();
    }

    public class AnimalMigrationEntryViewModel
    {
        public EarlyWarning.Enums.AnimalMigrationDirection Direction { get; set; }
        public EarlyWarning.Enums.MigratingAnimalType AnimalType { get; set; }
        public string? OtherAnimalType { get; set; }
        public int NumberOfAnimals { get; set; }
    }
}
