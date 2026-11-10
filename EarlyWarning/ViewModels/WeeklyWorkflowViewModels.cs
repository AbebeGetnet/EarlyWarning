using EarlyWarning.Enums;

namespace EarlyWarning.ViewModels;

public class WeeklyWorkflowIndexViewModel
{
    public string ScopeName { get; set; } = "";
    /// <summary>Review/approval lists only.</summary>
    public List<WeeklyWorkflowRowViewModel> Reports { get; set; } = [];

    // የተጠቃለለ ሪፖርት: the selected week in the standard narrative format.
    public string? ErrorMessage { get; set; }
    public List<WeeklyFormReportWeek> AvailableWeeks { get; set; } = [];
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public WeeklyNarrativeReport? Narrative { get; set; }
}

public class WeeklyWorkflowRowViewModel
{
    public Guid Id { get; set; }
    public Guid WoredaId { get; set; }
    public string Woreda { get; set; } = "";
    public string Zone { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public WeeklySubmissionStatus Status { get; set; }
    public DateTime LastActionAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class WeeklyAggregateViewModel
{
    public int WoredaReports { get; set; }
    public int ApprovedReports { get; set; }
    public int PendingReports { get; set; }
    public int FullRainCoverageKebeles { get; set; }
    public int PartialRainCoverageKebeles { get; set; }
    public int NoRainKebeles { get; set; }
    public List<RainfallZoneAggregateViewModel> RainfallZones { get; set; } = [];
    public List<FarmingZoneAggregateViewModel> FarmingZones { get; set; } = [];
    public float AffectedCropLandHectares { get; set; }
    public int CropAffectedPeople { get; set; }
    public int AnimalDiseaseAffectedPeople { get; set; }
    public List<AggregatedTextViewModel> TextEntries { get; set; } = [];
    public List<AnimalMigrationAggregateViewModel> AnimalMigrationRoutes { get; set; } = [];
    public List<AggregateSectionViewModel> Sections { get; set; } = [];
    // One entry per event (flood, drought, …): affected kebeles, woredas and zones are counted separately.
    public List<AffectedLocationSummaryViewModel> AffectedLocations { get; set; } = [];
    public bool IsSnapshot { get; set; }
    public DateTime? GeneratedAt { get; set; }
}

public record RainfallZoneAggregateViewModel(
    string Zone,
    int FullRainWoredas,
    int PartialRainWoredas,
    int NoRainWoredas,
    int NoDataWoredas)
{
    public int TotalWoredas => FullRainWoredas + PartialRainWoredas + NoRainWoredas + NoDataWoredas;
}

public record FarmingZoneAggregateViewModel(
    string Zone,
    decimal MeherFarmPlan,
    decimal MeherPloughed,
    decimal MeherSown,
    decimal MeherHarvested,
    decimal MeherResidualMoistureSown,
    decimal AutumnFarmPlan,
    decimal AutumnSown);

public class AggregateSectionViewModel
{
    public string Name { get; set; } = "";
    public List<AggregateMetricViewModel> Metrics { get; set; } = [];
}

// Weight: how many underlying observations (or how much quantity) an averaged value stands for, so that a
// higher-level summary can combine averages as a weighted mean instead of an average of averages.
public record AggregateMetricViewModel(string Name, decimal Value, string Unit = "", string Method = "SUM", decimal Weight = 0);

/// <summary>
/// Locations affected by one event. Kebeles is the sum of the woredas' reported kebeles; Woredas counts each
/// woreda with at least one affected kebele once; Zones counts each zone holding at least one such woreda once.
/// </summary>
public class AffectedLocationSummaryViewModel
{
    public string Event { get; set; } = "";
    public int Kebeles { get; set; }
    public int Woredas { get; set; }
    public int Zones { get; set; }
    public List<AffectedWoredaViewModel> Locations { get; set; } = [];
}

public class AffectedWoredaViewModel
{
    public Guid? ZoneId { get; set; }
    public string Zone { get; set; } = "";
    public Guid WoredaId { get; set; }
    public string Woreda { get; set; } = "";
    public int Kebeles { get; set; }
}

public class AnimalMigrationAggregateViewModel
{
    public string Direction { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public string AnimalType { get; set; } = "";
    public long NumberOfAnimals { get; set; }
}

public record AggregatedTextViewModel(string Source, string Location, string Text, string Zone = "");

public class WeeklyWorkflowDetailsViewModel
{
    public WeeklyWorkflowRowViewModel Report { get; set; } = new();
    public List<WeeklyWorkflowHistoryViewModel> History { get; set; } = [];
    public WeeklyAggregateViewModel Aggregate { get; set; } = new();
    public WeeklySubmissionFormViewModel Form { get; set; } = new();
    public bool CanZoneApprove { get; set; }
    public bool CanRegionApprove { get; set; }
    public bool ZoneDecisionWindowClosed { get; set; }
    // Issued only by the details page; the approve/reject action rejects requests without it.
    public string? ApprovalToken { get; set; }
}

public class ZoneWeeklyQueueViewModel
{
    public string Region { get; set; } = "";
    public List<ZoneWeeklySubmissionViewModel> Submissions { get; set; } = [];
}

/// <summary>Page 2: the standard report per zone (region users) or per woreda (zone and woreda users).</summary>
public class WeeklyReportPageTwoViewModel
{
    public string ScopeName { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public List<WeeklyFormReportWeek> AvailableWeeks { get; set; } = [];
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    /// <summary>"ዞን" or "ወረዳ": what each report in <see cref="Reports"/> covers.</summary>
    public string BreakdownWord { get; set; } = "ዞን";
    public List<WeeklyNarrativeReport> Reports { get; set; } = [];
}

public class ZoneWeeklySubmissionViewModel
{
    public Guid ZoneId { get; set; }
    public string Zone { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int WoredaCount { get; set; }
    public List<WeeklyWorkflowRowViewModel> Woredas { get; set; } = [];
    public WeeklyAggregateViewModel Aggregate { get; set; } = new();
}

public class WeeklyWorkflowHistoryViewModel
{
    public string User { get; set; } = "";
    public string Location { get; set; } = "";
    public WeeklyWorkflowAction Action { get; set; }
    public WeeklySubmissionStatus? PreviousStatus { get; set; }
    public WeeklySubmissionStatus NewStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime PerformedAt { get; set; }
}
