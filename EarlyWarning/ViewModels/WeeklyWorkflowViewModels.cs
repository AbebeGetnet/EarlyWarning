using EarlyWarning.Enums;

namespace EarlyWarning.ViewModels;

public class WeeklyWorkflowIndexViewModel
{
    public string ScopeName { get; set; } = "";
    public List<WeeklyWorkflowRowViewModel> Reports { get; set; } = [];
    public WeeklyAggregateViewModel Aggregate { get; set; } = new();
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

public record AggregateMetricViewModel(string Name, decimal Value, string Unit = "", string Method = "SUM");

public class AnimalMigrationAggregateViewModel
{
    public string Direction { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public string AnimalType { get; set; } = "";
    public long NumberOfAnimals { get; set; }
}

public record AggregatedTextViewModel(string Source, string Location, string Text);

public class WeeklyWorkflowDetailsViewModel
{
    public WeeklyWorkflowRowViewModel Report { get; set; } = new();
    public List<WeeklyWorkflowHistoryViewModel> History { get; set; } = [];
    public WeeklyAggregateViewModel Aggregate { get; set; } = new();
}

public class ZoneWeeklyQueueViewModel
{
    public string Region { get; set; } = "";
    public List<ZoneWeeklySubmissionViewModel> Submissions { get; set; } = [];
}

public class WeeklyReportPageTwoViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<WeeklyReportPageTwoZoneViewModel> Zones { get; set; } = [];
}

public class WeeklyReportPageTwoZoneViewModel
{
    public string Zone { get; set; } = "";
    public WeeklyAggregateViewModel Aggregate { get; set; } = new();
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
