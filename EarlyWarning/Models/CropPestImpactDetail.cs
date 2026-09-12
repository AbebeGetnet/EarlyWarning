using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

public class CropPestImpactDetail
{
    [Key] public Guid Id { get; set; }
    public Guid CropPestAndDeseaseReportId { get; set; }
    [ForeignKey(nameof(CropPestAndDeseaseReportId))] public CropPestAndDeseaseReport? Report { get; set; }
    public Guid CropPestAndDeseaseId { get; set; }
    [ForeignKey(nameof(CropPestAndDeseaseId))] public CropPestAndDesease? PestDisease { get; set; }
    [Range(0, int.MaxValue)] public int MaleHouseholdHeads { get; set; }
    [Range(0, int.MaxValue)] public int FemaleHouseholdHeads { get; set; }
    [Range(0, int.MaxValue)] public int MaleFamilyMembers { get; set; }
    [Range(0, int.MaxValue)] public int FemaleFamilyMembers { get; set; }
    [Range(0, int.MaxValue)] public int MaleChildren { get; set; }
    [Range(0, int.MaxValue)] public int FemaleChildren { get; set; }
    [Range(0, int.MaxValue)] public int MaleYouth { get; set; }
    [Range(0, int.MaxValue)] public int FemaleYouth { get; set; }
    [Range(0, int.MaxValue)] public int MaleElderly { get; set; }
    [Range(0, int.MaxValue)] public int FemaleElderly { get; set; }
    [Range(0, int.MaxValue)] public int MaleDisabled { get; set; }
    [Range(0, int.MaxValue)] public int FemaleDisabled { get; set; }
    [Range(0, 1000000)] public decimal AffectedLandInHectares { get; set; }
    [StringLength(200)] public string? AffectedCropType { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}
