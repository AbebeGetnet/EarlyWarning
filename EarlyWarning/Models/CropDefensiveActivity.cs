using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

/// <summary>A defensive/control activity carried out in response to the week's crop pest or disease damage.</summary>
public class CropDefensiveActivity
{
    [Key] public Guid Id { get; set; }
    public Guid CropPestAndDeseaseReportId { get; set; }
    [ForeignKey(nameof(CropPestAndDeseaseReportId))] public CropPestAndDeseaseReport? Report { get; set; }
    [Display(Name = "የተወሰደ የመከላከል ተግባር")]
    [StringLength(300)] public string Activity { get; set; } = string.Empty;
    [Display(Name = "የተሸፈነ መሬት (ሄ/ር)")]
    [Range(0, 1000000)] public decimal? AreaCoveredInHectares { get; set; }
}
