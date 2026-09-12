using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

public class AnimalDiseaseImpactDetail
{
    [Key] public Guid Id { get; set; }
    public Guid AnimalHealthStatusId { get; set; }
    [ForeignKey(nameof(AnimalHealthStatusId))] public AnimalHealthStatus? Report { get; set; }
    public Guid AnimalDiseaseId { get; set; }
    [ForeignKey(nameof(AnimalDiseaseId))] public AnimalDisease? Disease { get; set; }
    [Range(0, int.MaxValue)] public int NumberOfAffectedAnimals { get; set; }
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
    [StringLength(500)] public string? Notes { get; set; }
}
