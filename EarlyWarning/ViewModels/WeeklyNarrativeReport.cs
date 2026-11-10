namespace EarlyWarning.ViewModels;

/// <summary>
/// The weekly early-warning report in the commission's standard narrative format (as in the regional
/// "ሳምንታዊ ሪፖርት" document): header, covered period, then numbered sections made of sentences,
/// tables and bullets that name the zone, woreda and kebele, each closed by a "በሌሎች … የለም" sentence.
/// </summary>
public sealed class WeeklyNarrativeReport
{
    public string Organization { get; init; } = "";
    public string? Directorate { get; init; }
    public string Title { get; init; } = "የቅድሚያ ማስጠንቀቂያና ምላሽ ሳምንታዊ ሪፖርት";
    /// <summary>"ሪፖርቱ የሚሸፍነው ጊዜ 17/08/2018 — 23/08/2018 ዓ/ም".</summary>
    public string Period { get; init; } = "";
    public string ScopeName { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int WoredasInScope { get; init; }
    public int WoredasReported { get; init; }
    public List<NarrativeSection> Sections { get; } = [];

    // Figures the dashboard shows as tiles and charts (the same numbers as in the text).
    public string UnitWord { get; init; } = "ዞን";
    public List<NarrativeRainRow> RainRows { get; } = [];
    public NarrativeFarming Meher { get; set; } = new();
    public NarrativeFarming Belg { get; set; } = new();
}

/// <summary>A numbered heading (level 1 "3", level 2 "3.1") and its content.</summary>
public sealed class NarrativeSection
{
    public string Number { get; init; } = "";
    public string Title { get; init; } = "";
    public int Level { get; init; } = 1;
    /// <summary>Stable id for the dashboard (e.g. "animal-health"); empty for plain headings.</summary>
    public string Key { get; init; } = "";
    /// <summary>Incidents listed (one per woreda/hazard line).</summary>
    public int IncidentCount { get; set; }
    public List<NarrativeBlock> Blocks { get; } = [];
}

public enum NarrativeBlockKind { Paragraph, Bullets, Table }

public sealed record NarrativeBlock(
    NarrativeBlockKind Kind,
    string? Text = null,
    IReadOnlyList<string>? Items = null,
    IReadOnlyList<string>? Headers = null,
    IReadOnlyList<IReadOnlyList<string>>? Rows = null,
    IReadOnlyList<string>? Footer = null);

/// <summary>Woredas per rain class in one zone (or woreda) row of the rain table.</summary>
public sealed record NarrativeRainRow(string Unit, int Full, int Partial, int None, int NoData)
{
    public int Total => Full + Partial + None + NoData;
}

public sealed class NarrativeFarming
{
    public decimal Plan { get; init; }
    public decimal Ploughed { get; init; }
    public decimal Sown { get; init; }
    public decimal Harvested { get; init; }
    public decimal ResidualMoisture { get; init; }
    public bool HasData => Plan > 0 || Ploughed > 0 || Sown > 0 || Harvested > 0 || ResidualMoisture > 0;
    public decimal? PloughedPercent => Plan > 0 ? Math.Round(Ploughed / Plan * 100, 1) : null;
    public decimal? SownPercent => Ploughed > 0 ? Math.Round(Sown / Ploughed * 100, 1) : null;
}
