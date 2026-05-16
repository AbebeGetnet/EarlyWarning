using EarlyWarning.Models;

namespace EarlyWarning.ViewModels
{
    public class RegistrationWithardViewModel
    {
        // Shared across all reports
        public Guid WoredaId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Remarks { get; set; }

        public RainfallReport RainfallReport { get; set; }
        public FarmingActivity FarmingActivity { get; set; }
        public CropPestAndDeseaseReport CropPestAndDeseaseReport { get; set; }
        public PastureStatus PastureStatus { get; set; }
        public AnimalWaterSupplyStatus AnimalWaterSupplyStatus { get; set; }
        public AnimalHealthStatus AnimalHealthStatus { get; set; }

        // Disease selection lists (populated from DB)
        public List<Locations> Kebelies { get; set; } = new List<Locations>();
        public List<AnimalDisease> AnimalDiseases { get; set; } = new List<AnimalDisease>();
        public List<CropPestAndDesease> CropPestAndDeseases { get; set; } = new List<CropPestAndDesease>();

        // Selected IDs for crop and animal diseases
        public List<Guid> SelectedKebeliesIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedCropDiseaseIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedAnimalDiseaseIds { get; set; } = new List<Guid>();
    }
}
