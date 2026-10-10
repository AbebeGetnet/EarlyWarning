namespace EarlyWarning.ViewModels;

public class DashboardViewModel
{
    public string ScopeName { get; set; } = "";
    public string UserDisplayName { get; set; } = "";
    /// <summary>Reports of the latest week in the user's scope.</summary>
    public List<WeeklyWorkflowRowViewModel> Reports { get; set; } = [];
    public WeeklyAggregateViewModel Aggregate { get; set; } = new();
    /// <summary>Woredas reporting per week, oldest first (up to the last 8 weeks).</summary>
    public List<DashboardTrendPoint> Trend { get; set; } = [];
    /// <summary>Affected people per hazard/cause for the latest week, largest first.</summary>
    public List<DashboardCause> AffectedByCause { get; set; } = [];
}

public record DashboardTrendPoint(DateTime WeekStart, int Woredas);

public record DashboardCause(string Cause, decimal People);
