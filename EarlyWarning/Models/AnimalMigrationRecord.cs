using EarlyWarning.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

public class AnimalMigrationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WeeklyReportSubmissionId { get; set; }
    [ForeignKey(nameof(WeeklyReportSubmissionId))] public WeeklyReportSubmission Submission { get; set; } = null!;
    public Guid WoredaId { get; set; }
    [ForeignKey(nameof(WoredaId))] public Locations Woreda { get; set; } = null!;
    public AnimalMigrationDirection Direction { get; set; }
    public Guid OriginLocationId { get; set; }
    [ForeignKey(nameof(OriginLocationId))] public Locations OriginLocation { get; set; } = null!;
    public Guid DestinationLocationId { get; set; }
    [ForeignKey(nameof(DestinationLocationId))] public Locations DestinationLocation { get; set; } = null!;
    public MigratingAnimalType AnimalType { get; set; }
    [MaxLength(100)] public string? OtherAnimalType { get; set; }
    [Range(1, 1_000_000_000)] public int NumberOfAnimals { get; set; }
}
