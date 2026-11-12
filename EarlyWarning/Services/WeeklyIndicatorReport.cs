using EarlyWarning.ViewModels;
using static EarlyWarning.Services.WeeklyIndicators;

namespace EarlyWarning.Services;

/// <summary>
/// Adds the week's key indicators (<see cref="WeeklyIndicatorSummary"/>) to the on-screen reports: the narrative
/// report (summary, per zone/woreda, dashboard) and the paper-form report. The Excel workbook is written from
/// the report grid only and is not affected.
/// </summary>
public static class WeeklyIndicatorReport
{
    private static readonly string[] AidHeaders = ["የእርዳታ አይነት", "መለኪያ", "የተጠየቀ", "የቀረበ", "የተሰራጨ", "ከተጠየቀው የቀረበ", "ከቀረበው የተሰራጨ", "ያልተሸፈነ ፍላጎት"];
    private static readonly string[] KpiHeaders = ["ዘርፍ", "አመልካች", "ውጤት", "ማብራሪያ", "ደረጃ"];
    private static readonly string[] PriorityHeaders = ["ተ/ቁ", "ወረዳ", "ዞን", "ደረጃ", "ነጥብ", "ዋና ዋና ችግሮች"];
    private static readonly string[] ActionHeaders = ["ተ/ቁ", "ዘርፍ", "የሚመከር እርምጃ", "ቅድሚያ", "የሚመለከታቸው ወረዳዎች"];
    private static readonly string[] CauseHeaders = ["ምክንያት", "የተጎዱ ሰዎች", "ወረዳዎች"];

    private const string ScoreNote = "ደረጃው የሚሰላው ከእያንዳንዱ ወረዳ ሪፖርት በተገኙ ችግሮች ክብደት ድምር ነው፤ ሕይወትን የሚያሰጉ (የሰው ወረርሽኝ፣ ሞት፣ GAM ≥15%) 3፣ " +
        "ከባድ (ድርቅ/ዝናብ ማጣት፣ ጎርፍ፣ የእንስሳት በሽታ ወይም አቋም መውረድ፣ የመጠጥ ውሃ እጥረት፣ አደጋ፣ ስደት) 2፣ ሌሎች 1 ነጥብ አላቸው። " +
        "ድምሩ 7 እና በላይ — አስቸኳይ ምላሽ፤ 4–6 ወይም ሕይወትን የሚያሰጋ ችግር — ከፍተኛ ስጋት፤ 1–3 — ክትትል።";
    private const string CauseNote = "አንድ ሰው በአንድ በላይ ምክንያት ሊቆጠር ስለሚችል ድምሩ አልተሰጠም፤ ቁጥሩ አባወራ/እማወራና ቤተሰብ ነው (አደጋ አባወራ/እማወራ ብቻ)።";
    private const string NutritionNote = "መጠኑ ከሳምንታዊ ልየታ (screening) የተገኘ ስለሆነ አመላካች ነው። የዓለም ጤና ድርጅት የGAM ደረጃ፡ <5% ዝቅተኛ፣ 5–10% መካከለኛ፣ 10–15% ከፍተኛ፣ ≥15% በጣም ከፍተኛ።";

    // ------------------------------------------------------------------ narrative report

    public static void Append(WeeklyNarrativeReport report, WeeklyIndicatorSummary s)
    {
        if (!s.HasData) return;

        // At a glance: first section of the report.
        var glance = new NarrativeSection { Number = "", Title = "ዋና ዋና አመልካቾች (በጨረፍታ)", Key = "kpi" };
        glance.Blocks.Add(Paragraph(Headline(s)));
        glance.Blocks.Add(Table(KpiHeaders, s.Kpis.Select(k => Row(k.Group, k.Label, k.Value, k.Note, LevelName(k.Level)))));
        if (s.PeopleByCause.Count > 0)
        {
            glance.Blocks.Add(Paragraph("በችግሩ የተጎዱ ሰዎች በምክንያት፡"));
            glance.Blocks.Add(Table(CauseHeaders, s.PeopleByCause.Select(c => Row(c.Cause, N(c.People), N(c.Woredas)))));
            glance.Blocks.Add(Paragraph(CauseNote));
        }
        report.Sections.Insert(0, glance);

        // Market: livestock-to-grain terms of trade.
        if (s.TermsOfTrade.Count > 0 && Find(report, "prices") is { } prices)
        {
            prices.Blocks.Add(Paragraph("የእንስሳትና የእህል ልውውጥ (አንድ እንስሳ ሲሸጥ የሚገዛው እህል)፡"));
            prices.Blocks.Add(Table(["እንስሳ", "እህል", "ባለፈው ሳምንት (ኪ.ግ)", "በዚህ ሳምንት (ኪ.ግ)", "ለውጥ"],
                s.TermsOfTrade.Select(t => Row(t.Animal, t.Grain, t.Previous.HasValue ? N(t.Previous.Value) : "-", N(t.Current), t.Change.HasValue ? Signed(t.Change.Value) + "%" : "-"))));
        }

        // Migration: animals.
        if (s.AnimalsMovedOut + s.AnimalsMovedIn > 0 && Find(report, "migration-death") is { } migration)
            migration.Blocks.Add(Paragraph($"የእንስሳት ስደት፡ ከወረዳዎቹ የወጡ {N(s.AnimalsMovedOut)} እንስሳት፣ ወደ ወረዳዎቹ የገቡ {N(s.AnimalsMovedIn)} እንስሳት።"));

        // Aid: recipients and quantities.
        if (Find(report, "aid") is { } aid)
        {
            if (s.AidRecipients > 0) aid.Blocks.Add(Paragraph($"የዕለት እርዳታ ተጠቃሚዎች፡ {N(s.AidRecipients)} ሰው (አባወራ/እማወራና ቤተሰብ)።"));
            if (s.Aid.Count > 0)
            {
                aid.Blocks.Add(Paragraph("በሳምንቱ የተጠየቀ፣ የቀረበና የተሰራጨ እርዳታ፡"));
                aid.Blocks.Add(Table(AidHeaders, s.Aid.Select(AidRow)));
            }
        }

        var next = report.Sections.Where(x => x.Level == 1 && int.TryParse(x.Number, out _)).Select(x => int.Parse(x.Number)).DefaultIfEmpty(0).Max() + 1;

        // Nutrition.
        var nutrition = new NarrativeSection { Number = (next++).ToString(), Title = "የስነ-ምግብ ሁኔታ (ልየታ)", Key = "nutrition" };
        if (s.MalnutritionTotal is not { } total) nutrition.Blocks.Add(Paragraph("የስነ-ምግብ ልየታ መረጃ አልተገኘም።"));
        else
        {
            nutrition.Blocks.Add(Paragraph(NutritionSentence(total)));
            if (s.Malnutrition.Count > 1)
                nutrition.Blocks.Add(Table(["ወረዳ", "ዞን", "የተለዩ ህጻናት", "ከፍተኛ", "መካከለኛ", "GAM %", "የተለዩ እናቶች", "የተገኘባቸው", "MAM %"],
                    s.Malnutrition.Select(m => Row(m.Woreda, m.Zone, N(m.ChildrenScreened), N(m.Severe), N(m.Moderate), Rate(m.GamRate),
                        N(m.PlwScreened), N(m.PlwModerate), Rate(m.PlwMamRate))),
                    Row("ድምር", "", N(total.ChildrenScreened), N(total.Severe), N(total.Moderate), Rate(total.GamRate),
                        N(total.PlwScreened), N(total.PlwModerate), Rate(total.PlwMamRate))));
            nutrition.Blocks.Add(Paragraph(NutritionNote));
        }
        report.Sections.Add(nutrition);

        // Priorities.
        var priorities = new NarrativeSection { Number = (next++).ToString(), Title = "ቅድሚያ ትኩረት የሚያስፈልጋቸው ወረዳዎች", Key = "priorities" };
        if (s.Priorities.Count == 0) priorities.Blocks.Add(Paragraph("በዚህ ሳምንት ልዩ ትኩረት የሚያስፈልገው ወረዳ አልተገኘም።"));
        else
        {
            priorities.Blocks.Add(Table(PriorityHeaders, s.Priorities.Select((p, i) => Row(N(i + 1), p.Woreda, p.Zone, LevelName(p.Level), N(p.Score), string.Join("፤ ", p.Flags)))));
            priorities.Blocks.Add(Paragraph(ScoreNote));
        }
        report.Sections.Add(priorities);

        // Actions.
        var actions = new NarrativeSection { Number = (next++).ToString(), Title = "ለምላሽ ዕቅድ የሚመከሩ እርምጃዎች", Key = "actions" };
        if (s.Actions.Count == 0) actions.Blocks.Add(Paragraph("ከመደበኛ ክትትል ውጪ የሚመከር ተጨማሪ እርምጃ የለም።"));
        else actions.Blocks.Add(Table(ActionHeaders, s.Actions.Select((a, i) => Row(N(i + 1), a.Sector, a.Action, LevelName(a.Level),
            a.Woredas.Count == 0 ? "ሁሉም" : string.Join("፣ ", a.Woredas)))));
        report.Sections.Add(actions);
    }

    // ------------------------------------------------------------------ paper-form report

    public static IEnumerable<FormReportSection> FormSections(WeeklyIndicatorSummary s, int firstNumber)
    {
        if (!s.HasData) yield break;
        var number = firstNumber;

        var nutrition = new FormReportSection { Number = (number++).ToString(), Title = "የስነ-ምግብ እጥረት ልየታ" };
        var t = s.MalnutritionTotal;
        nutrition.Items.Add(Heading($"{nutrition.Number}.1", "ከ5 ዓመት በታች ያሉ ህጻናት"));
        nutrition.Items.Add(Line("•", "ልየታ የተደረገላቸው", N(t?.ChildrenScreened ?? 0)));
        nutrition.Items.Add(Line("•", "በከፍተኛ ደረጃ የምግብ እጥረት የተገኘባቸው", N(t?.Severe ?? 0)));
        nutrition.Items.Add(Line("•", "በመካከለኛ ደረጃ የምግብ እጥረት የተገኘባቸው", N(t?.Moderate ?? 0)));
        nutrition.Items.Add(Line("•", "GAM መጠን", t?.GamRate is { } gam ? $"{N(gam)}% — {GamBand(gam).Name}" : "-"));
        nutrition.Items.Add(Heading($"{nutrition.Number}.2", "እርጉዝና የሚያጠቡ እናቶች"));
        nutrition.Items.Add(Line("•", "ልየታ የተደረገላቸው", N(t?.PlwScreened ?? 0)));
        nutrition.Items.Add(Line("•", "በመካከለኛ ደረጃ የምግብ እጥረት የተገኘባቸው", N(t?.PlwModerate ?? 0)));
        nutrition.Items.Add(Line("•", "MAM መጠን", Rate(t?.PlwMamRate)));
        if (s.Malnutrition.Count > 1)
            nutrition.Items.Add(FormTable(["ወረዳ", "ዞን", "የተለዩ ህጻናት", "ከፍተኛ", "መካከለኛ", "GAM %", "የተለዩ እናቶች", "የተገኘባቸው", "MAM %"],
                s.Malnutrition.Select(m => new List<string> { m.Woreda, m.Zone, N(m.ChildrenScreened), N(m.Severe), N(m.Moderate), Rate(m.GamRate),
                    N(m.PlwScreened), N(m.PlwModerate), Rate(m.PlwMamRate) })));
        yield return nutrition;

        var kpis = new FormReportSection { Number = (number++).ToString(), Title = "ዋና ዋና አመልካቾች" };
        kpis.Items.Add(FormTable([.. KpiHeaders], s.Kpis.Select(k => new List<string> { k.Group, k.Label, k.Value, k.Note, LevelName(k.Level) })));
        if (s.PeopleByCause.Count > 0)
        {
            kpis.Items.Add(Heading("", "በችግሩ የተጎዱ ሰዎች በምክንያት"));
            kpis.Items.Add(FormTable([.. CauseHeaders], s.PeopleByCause.Select(c => new List<string> { c.Cause, N(c.People), N(c.Woredas) })));
        }
        if (s.TermsOfTrade.Count > 0)
        {
            kpis.Items.Add(Heading("", "የእንስሳትና የእህል ልውውጥ (ኪ.ግ እህል በአንድ እንስሳ)"));
            kpis.Items.Add(FormTable(["እንስሳ", "እህል", "ባለፈው ሳምንት", "በዚህ ሳምንት", "ለውጥ"],
                s.TermsOfTrade.Select(x => new List<string> { x.Animal, x.Grain, x.Previous.HasValue ? N(x.Previous.Value) : "-", N(x.Current), x.Change.HasValue ? Signed(x.Change.Value) + "%" : "-" })));
        }
        if (s.Aid.Count > 0)
        {
            kpis.Items.Add(Heading("", "የእርዳታ ፍላጎት ሽፋንና ስርጭት"));
            kpis.Items.Add(FormTable([.. AidHeaders], s.Aid.Select(a => AidRow(a).ToList())));
        }
        yield return kpis;

        var priorities = new FormReportSection { Number = (number++).ToString(), Title = "ቅድሚያ ትኩረት የሚያስፈልጋቸው ወረዳዎች" };
        priorities.Items.Add(FormTable([.. PriorityHeaders], s.Priorities.Select((p, i) => new List<string> { N(i + 1), p.Woreda, p.Zone, LevelName(p.Level), N(p.Score), string.Join("፤ ", p.Flags) })));
        priorities.Items.Add(new FormReportItem { Kind = FormReportItemKind.ZoneText, Label = "ማብራሪያ", Indent = 1, ZoneTexts = [new("", ScoreNote)] });
        yield return priorities;

        var actions = new FormReportSection { Number = (number++).ToString(), Title = "ለምላሽ ዕቅድ የሚመከሩ እርምጃዎች" };
        actions.Items.Add(FormTable([.. ActionHeaders], s.Actions.Select((a, i) => new List<string> { N(i + 1), a.Sector, a.Action, LevelName(a.Level), a.Woredas.Count == 0 ? "ሁሉም" : string.Join("፣ ", a.Woredas) })));
        yield return actions;
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>"በዚህ ሳምንት ከ12 ወረዳዎች 2 አስቸኳይ ምላሽ፣ 3 ከፍተኛ ስጋት …".</summary>
    internal static string Headline(WeeklyIndicatorSummary s)
    {
        int Count(AlertLevel level) => s.Priorities.Count(x => x.Level == level);
        var text = $"ሪፖርት ከላኩ {N(s.WoredasReported)} ወረዳዎች ውስጥ {N(Count(AlertLevel.Emergency))} ወረዳዎች አስቸኳይ ምላሽ፣ " +
                   $"{N(Count(AlertLevel.Alarm))} ከፍተኛ ስጋት፣ {N(Count(AlertLevel.Watch))} ክትትል የሚያስፈልጋቸው ሲሆኑ " +
                   $"{N(s.WoredasReported - s.Priorities.Count)} ወረዳዎች በመደበኛ ሁኔታ ላይ ይገኛሉ።";
        var top = s.Actions.Where(x => x.Level >= AlertLevel.Alarm).Take(3).Select(x => x.Sector).Distinct().ToList();
        if (top.Count > 0) text += $" ቅድሚያ የሚያስፈልጋቸው ዘርፎች፡ {string.Join("፣ ", top)}።";
        return text;
    }

    private static string NutritionSentence(MalnutritionRow t)
    {
        var parts = new List<string>();
        if (t.ChildrenScreened > 0)
            parts.Add($"ልየታ ከተደረገላቸው {N(t.ChildrenScreened)} ከ5 ዓመት በታች ህጻናት ውስጥ {N(t.Severe)} በከፍተኛ፣ {N(t.Moderate)} በመካከለኛ ደረጃ የምግብ እጥረት ተገኝቶባቸዋል" +
                      (t.GamRate is { } gam ? $" (GAM {N(gam)}% — {GamBand(gam).Name})" : "") + "።");
        if (t.PlwScreened > 0)
            parts.Add($"ልየታ ከተደረገላቸው {N(t.PlwScreened)} እርጉዝና የሚያጠቡ እናቶች ውስጥ {N(t.PlwModerate)} የምግብ እጥረት ተገኝቶባቸዋል" +
                      (t.PlwMamRate is { } m ? $" (MAM {N(m)}%)" : "") + "።");
        return string.Join(" ", parts);
    }

    private static IReadOnlyList<string> AidRow(AidLine a) => Row(a.Item, a.Unit, N(a.Requested), N(a.Provided), N(a.Distributed),
        Rate(a.Coverage), Rate(a.DistributionRate), a.Gap > 0 ? N(a.Gap) : "-");

    private static NarrativeSection? Find(WeeklyNarrativeReport report, string key) => report.Sections.FirstOrDefault(x => x.Key == key);
    private static NarrativeBlock Paragraph(string text) => new(NarrativeBlockKind.Paragraph, Text: text);
    private static NarrativeBlock Table(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows, IReadOnlyList<string>? footer = null) =>
        new(NarrativeBlockKind.Table, Headers: headers, Rows: rows.ToList(), Footer: footer);
    private static IReadOnlyList<string> Row(params string[] cells) => cells;

    private static FormReportItem Heading(string number, string label) => new() { Kind = FormReportItemKind.Heading, Number = number, Label = label };
    private static FormReportItem Line(string number, string label, string value) =>
        new() { Kind = FormReportItemKind.Line, Number = number, Label = label, Value = value, Indent = 1 };
    private static FormReportItem FormTable(List<string> headers, IEnumerable<List<string>> rows) =>
        new() { Kind = FormReportItemKind.Table, Indent = 1, TableHeaders = headers, TableRows = rows.ToList() };

    private static string Rate(decimal? value) => value.HasValue ? N(value.Value) + "%" : "-";
    private static string Signed(decimal value) => (value > 0 ? "+" : "") + N(value);
}
