using EarlyWarning.Services;

namespace EarlyWarning.ViewModels;

/// <summary>
/// The weekly report laid out like the regional Excel summary: report topics down the rows, one column
/// block per zone (region users) or per woreda (zone users). Each block has two sub-columns; text rows
/// span both, number rows use them for two figures (መኸር/በልግ, ያለፈው/በዚህ ሳምንት).
/// </summary>
public sealed class WeeklyMatrixReport
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Title { get; init; } = "";
    public string ScopeName { get; init; } = "";
    public ReportScopeLevel ScopeLevel { get; init; }
    /// <summary>What a column is: "ዞን" (region report) or "ወረዳ".</summary>
    public string UnitWord { get; init; } = "ዞን";
    public int WoredasInScope { get; init; }
    public int WoredasReported { get; init; }
    public List<MatrixUnit> Units { get; } = [];
    public List<MatrixRow> Rows { get; } = [];
}

/// <summary>The Excel export page: every week the user can export, newest first.</summary>
public sealed class WeeklyExcelExportViewModel
{
    public string ScopeName { get; init; } = "";
    public int WoredasInScope { get; init; }
    public string? ErrorMessage { get; init; }
    public List<WeeklyExcelExportWeek> Weeks { get; init; } = [];
}

public sealed record WeeklyExcelExportWeek(DateTime StartDate, DateTime EndDate, int WoredasReported);

/// <param name="Name">Zone or woreda name shown in the column header.</param>
/// <param name="IsTotal">The trailing "all units" column: filled for number rows only.</param>
/// <param name="Group">For woreda columns: the woreda's zone.</param>
public sealed record MatrixUnit(string Name, bool IsTotal = false, string Group = "");

public enum MatrixRowKind
{
    /// <summary>Repeats the column headers (unit names).</summary>
    UnitHeader,
    /// <summary>Numbered section title; may carry sub-column captions in <see cref="MatrixRow.Cells"/>.</summary>
    Section,
    /// <summary>One text per unit, spanning both sub-columns.</summary>
    Text,
    /// <summary>Two numbers per unit.</summary>
    Pair
}

public sealed class MatrixRow
{
    public MatrixRowKind Kind { get; init; }
    public string Number { get; init; } = "";
    public string Label { get; init; } = "";
    /// <summary>One cell per unit, in <see cref="WeeklyMatrixReport.Units"/> order; empty for headers.</summary>
    public List<MatrixCell> Cells { get; init; } = [];
    /// <summary>Excel number format for <see cref="MatrixRowKind.Pair"/> rows.</summary>
    public string NumberFormat { get; init; } = "#,##0";
}

/// <param name="Text">Text spanning both sub-columns (text rows) or the left caption (section rows).</param>
/// <param name="Left">Left number (pair rows).</param>
/// <param name="Right">Right number (pair rows), or the right caption via <paramref name="RightText"/>.</param>
/// <param name="Count">A number behind the text where one exists (e.g. woredas in a rain class).</param>
public sealed record MatrixCell(string? Text = null, decimal? Left = null, decimal? Right = null, string? RightText = null, int? Count = null);
