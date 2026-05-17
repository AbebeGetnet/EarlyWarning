using EarlyWarning.Models;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.Wizard
{
    public class CombinedReportWizardModel
    {
        // Common properties
        [Required(ErrorMessage = "ወረዳ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [Required(ErrorMessage = "የሪፖርት ሳምንት ያስፈልጋል")]
        public DateTime ReportingWeekStart { get; set; }

        // Step 1
        public RainfallReport Rainfall { get; set; } = new RainfallReport();

        // Step 2
        public FarmingActivity Farming { get; set; } = new FarmingActivity();

        // Step 3
        public CropGrowth Growth { get; set; } = new CropGrowth();

        // Step 4
        public CropPestAndDeseaseReport PestDisease { get; set; } = new CropPestAndDeseaseReport();

        // Step 5
        public PastureStatus Pasture { get; set; } = new PastureStatus();

        // Step 6
        public AnimalWaterSupplyStatus WaterSupply { get; set; } = new AnimalWaterSupplyStatus();

        // Step 7
        public AnimalHealthStatus AnimalHealth { get; set; } = new AnimalHealthStatus();

        // Wizard state
        public int CurrentStep { get; set; } = 1;
        public bool IsSubmitted { get; set; } = false;
    }
}