using EarlyWarning.Models;

public class RegistrationWizardIndexViewModel
{
    public Guid WoredaId { get; set; }
    public string WoredaName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public RainfallReport RainfallReport { get; set; }
    public FarmingActivity FarmingActivity { get; set; }
    public CropPestAndDeseaseReport CropPestAndDeseaseReport { get; set; }
    public PastureStatus PastureStatus { get; set; }
    public AnimalWaterSupplyStatus AnimalWaterSupplyStatus { get; set; }
    public AnimalHealthStatus AnimalHealthStatus { get; set; }
    public List<string> CropDiseaseNames { get; set; } = new();
    public List<string> AnimalDiseaseNames { get; set; } = new();
    public List<string> KebeleNames { get; set; } = new();
}