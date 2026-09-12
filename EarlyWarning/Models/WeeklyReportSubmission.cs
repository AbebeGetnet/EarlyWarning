using EarlyWarning.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

public class WeeklyReportSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WoredaId { get; set; }
    [ForeignKey(nameof(WoredaId))] public Locations Woreda { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReportingWeekStart { get; set; }
    // Draft is intentionally explicit because SubmittedToZone is the enum's
    // legacy zero value. A newly constructed workflow must never look submitted.
    public WeeklySubmissionStatus Status { get; set; } = WeeklySubmissionStatus.Draft;
    public string CreatedById { get; set; } = null!;
    [ForeignKey(nameof(CreatedById))] public ApplicationUser CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActionAt { get; set; }
    public string? DraftDataJson { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = null!;
    public ICollection<WeeklyReportWorkflowHistory> History { get; set; } = new List<WeeklyReportWorkflowHistory>();
}

public class WeeklyReportWorkflowHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WeeklyReportSubmissionId { get; set; }
    [ForeignKey(nameof(WeeklyReportSubmissionId))] public WeeklyReportSubmission Submission { get; set; } = null!;
    public string UserId { get; set; } = null!;
    [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = null!;
    public Guid ActorLocationId { get; set; }
    public LocationLevel ActorLocationLevel { get; set; }
    public WeeklyWorkflowAction Action { get; set; }
    public WeeklySubmissionStatus? PreviousStatus { get; set; }
    public WeeklySubmissionStatus NewStatus { get; set; }
    [MaxLength(2000)] public string? Comment { get; set; }
    public DateTime PerformedAt { get; set; }
}
