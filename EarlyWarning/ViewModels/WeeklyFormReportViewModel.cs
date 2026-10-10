using EarlyWarning.Services;

namespace EarlyWarning.ViewModels;

/// <summary>
/// Weekly early-warning report laid out like the paper form (sections 1–11):
/// numbers are totalled across all woredas in scope, free text is concatenated per zone.
/// </summary>
public class WeeklyFormReportViewModel
{
    /// <summary>Set when the user's scope cannot be resolved (no / unsupported location); nothing else is shown.</summary>
    public string? ErrorMessage { get; set; }
    public ReportScopeLevel ScopeLevel { get; set; }
    public string ScopeName { get; set; } = "";
    public string? ZoneName { get; set; }
    public string? RegionName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    /// <summary>Woredas that submitted a report for the selected week.</summary>
    public int WoredaCount { get; set; }
    /// <summary>All woredas inside the user's boundary.</summary>
    public int WoredasInScope { get; set; }
    /// <summary>Zones that have at least one submitted woreda report for the selected week.</summary>
    public List<string> Zones { get; set; } = [];
    public List<WeeklyFormReportWeek> AvailableWeeks { get; set; } = [];
    public List<FormReportSection> Sections { get; set; } = [];
    public string PreparedBy { get; set; } = "";
}

public record WeeklyFormReportWeek(DateTime StartDate, DateTime EndDate);

public class FormReportSection
{
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public List<FormReportItem> Items { get; set; } = [];
}

public enum FormReportItemKind { Heading, Line, People, ZoneText, Table }

public class FormReportItem
{
    public FormReportItemKind Kind { get; set; }
    public string Number { get; set; } = "";
    public string Label { get; set; } = "";
    public int Indent { get; set; }
    public string? Value { get; set; }
    public FormReportPeople? People { get; set; }
    /// <summary>
    /// Zone/region reports: one entry per zone ("X ዞን" → "woreda፡ text፤ …").
    /// Woreda reports: entries with an empty group holding the woreda's own text.
    /// </summary>
    public List<FormReportZoneText> ZoneTexts { get; set; } = [];
    public List<string> TableHeaders { get; set; } = [];
    public List<List<string>> TableRows { get; set; } = [];
}

public record FormReportZoneText(string Group, string Text);

public class FormReportPeople
{
    public int MaleHouseholds { get; set; }
    public int FemaleHouseholds { get; set; }
    public int MaleFamily { get; set; }
    public int FemaleFamily { get; set; }
    public int MaleChildren { get; set; }
    public int FemaleChildren { get; set; }
    public int MaleYouth { get; set; }
    public int FemaleYouth { get; set; }
    public int MaleElderly { get; set; }
    public int FemaleElderly { get; set; }
    public int MaleDisabled { get; set; }
    public int FemaleDisabled { get; set; }
    /// <summary>Null when the source record has no lactating/pregnant field.</summary>
    public int? LactatingAndPregnant { get; set; }
    public bool HasFamilyRow { get; set; } = true;

    public void Add(int mh, int fh, int mf, int ff, int mc, int fc, int my, int fy, int me, int fe, int md, int fd)
    {
        MaleHouseholds += mh; FemaleHouseholds += fh; MaleFamily += mf; FemaleFamily += ff;
        MaleChildren += mc; FemaleChildren += fc; MaleYouth += my; FemaleYouth += fy;
        MaleElderly += me; FemaleElderly += fe; MaleDisabled += md; FemaleDisabled += fd;
    }
}
