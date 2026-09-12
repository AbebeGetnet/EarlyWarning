using EarlyWarning.Enums;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models;

public class WeeklyAggregateSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScopeLocationId { get; set; }
    public LocationLevel ScopeLevel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public WeeklySubmissionStatus ApprovalStatus { get; set; }
    public int SourceReportCount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedById { get; set; } = "";
    [MaxLength] public string AggregateJson { get; set; } = "{}";
}
