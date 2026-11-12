namespace EarlyWarning.ViewModels;

/// <summary>
/// Alert level of a woreda or an indicator, from the flags raised by the week's data
/// (see <see cref="Services.WeeklyIndicators"/> for the rules).
/// </summary>
public enum AlertLevel { Normal = 0, Watch = 1, Alarm = 2, Emergency = 3 }

/// <summary>One flag raised for a woreda: a stable code, its weight (1–3) and a short description with figures.</summary>
public sealed record IndicatorFlag(string Code, int Weight, string Text);

/// <summary>Everything the indicators need from one woreda's weekly report (one row per reporting woreda).</summary>
public sealed class WoredaWeekFacts
{
    public Guid WoredaId { get; init; }
    public string Woreda { get; init; } = "";
    public string Zone { get; init; } = "";

    // Rain
    public bool RainReported { get; set; }
    public bool NoRain { get; set; }
    public int NoRainKebeles { get; set; }
    public int DroughtKebeles { get; set; }
    public int FloodKebeles { get; set; }

    // Farming (hectares)
    public decimal MeherPlan { get; set; }
    public decimal MeherPloughed { get; set; }
    public decimal MeherSown { get; set; }
    public decimal BelgPlan { get; set; }
    public decimal BelgPloughed { get; set; }
    public decimal BelgSown { get; set; }

    // Crop pests
    public bool PestOutbreak { get; set; }
    public decimal PestHectares { get; set; }
    public decimal PestControlledHectares { get; set; }
    public long PestPeople { get; set; }

    // Livestock
    public bool PastureShortage { get; set; }
    public bool AnimalWaterShortage { get; set; }
    public bool LowBodyCondition { get; set; }
    public bool AnimalDiseaseOutbreak { get; set; }
    public long AnimalsAffectedByDisease { get; set; }
    public long AnimalsAffectedByFeedShortage { get; set; }
    public long AnimalsMovedOut { get; set; }
    public long AnimalsMovedIn { get; set; }
    public long PasturePeople { get; set; }
    public long AnimalWaterPeople { get; set; }
    public long AnimalHealthPeople { get; set; }

    // People
    public bool HumanWaterProblem { get; set; }
    public int HumanWaterKebeles { get; set; }
    public long HumanWaterPeople { get; set; }
    public bool Epidemic { get; set; }
    public int EpidemicKebeles { get; set; }
    public long EpidemicPeople { get; set; }
    public long EpidemicPregnantLactating { get; set; }
    public bool Hazard { get; set; }
    public long HazardHouseholds { get; set; }
    public decimal HazardHectares { get; set; }
    public long MigratedPeople { get; set; }
    public long Deaths { get; set; }
    public bool OtherProblem { get; set; }
    public long OtherProblemPeople { get; set; }

    // Education
    public bool SchoolProblem { get; set; }
    public int ClosedSchools { get; set; }
    public int Dropouts { get; set; }

    // Nutrition (screening)
    public int ChildrenScreened { get; set; }
    public int ChildrenSevere { get; set; }
    public int ChildrenModerate { get; set; }
    public int PlwScreened { get; set; }
    public int PlwModerate { get; set; }
    public decimal? GamRate => ChildrenScreened > 0 ? Math.Round((ChildrenSevere + ChildrenModerate) * 100m / ChildrenScreened, 1) : null;
    public decimal? PlwMamRate => PlwScreened > 0 ? Math.Round(PlwModerate * 100m / PlwScreened, 1) : null;

    // Market: per-type prices this week and last week (birr; grain per quintal, animals per head)
    public List<PricePoint> GrainPrices { get; } = [];
    public List<PricePoint> AnimalPrices { get; } = [];
    /// <summary>Average change of the grain prices that have both weeks, in percent.</summary>
    public decimal? StaplePriceChange { get; set; }

    // Aid
    public long AidRecipients { get; set; }
    public bool AidProblem { get; set; }
    public string? AidProblemText { get; set; }
    public List<AidLine> Aid { get; } = [];

    public List<IndicatorFlag> Flags { get; } = [];
    public int Score => Flags.Sum(x => x.Weight);
    public AlertLevel Level => Services.WeeklyIndicators.LevelFor(Flags);
}

public sealed record PricePoint(string Type, decimal Current, decimal? Previous);

/// <summary>An aid item: quantities in the item's own unit (they are never added across units).</summary>
public sealed record AidLine(string Item, string Unit, decimal Requested, decimal Provided, decimal Distributed)
{
    /// <summary>Provided ÷ requested, %.</summary>
    public decimal? Coverage => Requested > 0 ? Math.Round(Provided / Requested * 100, 1) : null;
    /// <summary>Distributed ÷ provided, %.</summary>
    public decimal? DistributionRate => Provided > 0 ? Math.Round(Distributed / Provided * 100, 1) : null;
    public decimal Gap => Math.Max(0, Requested - Provided);
}

/// <summary>A key indicator line: group (sector), label, value, explanation and alert level.</summary>
public sealed record IndicatorKpi(string Group, string Label, string Value, string Note = "", AlertLevel Level = AlertLevel.Normal);

/// <summary>A woreda that needs attention, with its score, level and the flags behind it.</summary>
public sealed record WoredaPriority(string Woreda, string Zone, int Score, AlertLevel Level, IReadOnlyList<string> Flags);

/// <summary>A recommended response for one problem, with the woredas where it applies.</summary>
public sealed record RecommendedAction(string Sector, string Action, AlertLevel Level, IReadOnlyList<string> Woredas);

/// <summary>People affected, by cause (household heads + family; see Extensions/Beneficiaries).</summary>
public sealed record AffectedByCause(string Cause, long People, int Woredas);

public sealed record MalnutritionRow(string Woreda, string Zone, int ChildrenScreened, int Severe, int Moderate, decimal? GamRate,
    int PlwScreened, int PlwModerate, decimal? PlwMamRate);

/// <summary>Livestock-to-grain terms of trade: kilograms of grain one animal buys.</summary>
public sealed record TermsOfTrade(string Animal, string Grain, decimal Current, decimal? Previous)
{
    public decimal? Change => Previous > 0 ? Math.Round((Current - Previous.Value) / Previous.Value * 100, 1) : null;
}

/// <summary>The week's key indicators for a set of woredas (a region, a zone or a woreda).</summary>
public sealed class WeeklyIndicatorSummary
{
    public int WoredasReported { get; init; }
    public List<IndicatorKpi> Kpis { get; } = [];
    public List<AffectedByCause> PeopleByCause { get; } = [];
    public List<MalnutritionRow> Malnutrition { get; } = [];
    public MalnutritionRow? MalnutritionTotal { get; set; }
    public List<AidLine> Aid { get; } = [];
    public long AidRecipients { get; set; }
    public long AnimalsMovedOut { get; set; }
    public long AnimalsMovedIn { get; set; }
    public List<TermsOfTrade> TermsOfTrade { get; } = [];
    public List<WoredaPriority> Priorities { get; } = [];
    public List<RecommendedAction> Actions { get; } = [];
    public bool HasData => WoredasReported > 0;
}
