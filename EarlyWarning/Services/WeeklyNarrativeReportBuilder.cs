using EarlyWarning.Extensions;
using EarlyWarning.ViewModels;
using System.Globalization;

namespace EarlyWarning.Services;

/// <summary>
/// Turns the weekly grid (<see cref="WeeklyMatrixReport"/>, the same data as the Excel report) into the standard
/// narrative report. Incident cells hold one "• place፡ …" line per incident; those lines become the bullets, and
/// columns with data but no incident produce the closing "በሌሎች … የለም" sentence.
/// </summary>
public static class WeeklyNarrativeReportBuilder
{
    private const string Bullet = "• ";

    /// <param name="matrix">The week's grid.</param>
    /// <param name="scopeName">Region, zone or woreda the narrative is about.</param>
    /// <param name="level">Its level; decides the header and wording.</param>
    /// <param name="columns">Columns to include (default: every non-total column). When a total column exists and
    /// no subset is given, totals and price averages come from it; otherwise they are worked out from the columns.</param>
    public static WeeklyNarrativeReport Build(WeeklyMatrixReport matrix, string scopeName, ReportScopeLevel level, IReadOnlyList<int>? columns = null)
    {
        var units = columns ?? Enumerable.Range(0, matrix.Units.Count).Where(i => !matrix.Units[i].IsTotal).ToList();
        var totalIndex = matrix.Units.FindIndex(x => x.IsTotal);
        int? totalColumn = columns == null && totalIndex >= 0 ? totalIndex : null;
        if (totalColumn == null && units.Count == 1) totalColumn = units[0];
        var unitWord = matrix.UnitWord;
        var unitsPlural = unitWord == "ዞን" ? "ዞኖች" : "ወረዳዎች";
        string UnitName(int i) => $"{matrix.Units[i].Name} {unitWord}";
        var scopePhrase = level switch
        {
            ReportScopeLevel.Region => $"በ{scopeName} ክልል",
            ReportScopeLevel.Zone => $"በ{scopeName} ዞን",
            _ => $"በ{scopeName} ወረዳ"
        };
        var others = units.Count > 1 ? $"በሌሎች መረጃ በተገኘባቸው {unitsPlural}" : scopePhrase;

        var reportedRow = matrix.Rows.FirstOrDefault(r => r.Kind == MatrixRowKind.Text && r.Label == "ሪፖርት የላኩ ወረዳዎች");
        var report = new WeeklyNarrativeReport
        {
            Organization = level switch
            {
                ReportScopeLevel.Region => $"የ{scopeName} ክልል አደጋ ስጋት ስራ አመራር ኮሚሽን",
                ReportScopeLevel.Zone => $"የ{scopeName} ዞን አደጋ ስጋት ስራ አመራር ኮሚሽን ተጠሪ ጽ/ቤት",
                _ => $"የ{scopeName} ወረዳ አደጋ ስጋት ስራ አመራር ጽ/ቤት"
            },
            Directorate = level == ReportScopeLevel.Region ? "የቅድሚያ ማስጠንቀቂያና ምላሽ ዳይሬክቶሬት" : null,
            Period = $"ሪፖርቱ የሚሸፍነው ጊዜ፡ {EthiopianDate.FromGregorian(matrix.StartDate)} — {EthiopianDate.FromGregorian(matrix.EndDate)} ዓ/ም",
            ScopeName = scopeName,
            StartDate = matrix.StartDate,
            EndDate = matrix.EndDate,
            UnitWord = unitWord,
            WoredasInScope = columns == null ? matrix.WoredasInScope : units.Sum(i => CountWoredas(matrix, i)),
            WoredasReported = columns == null ? matrix.WoredasReported : units.Sum(i => CountReported(reportedRow, i))
        };

        MatrixRow? Row(string number) => matrix.Rows.FirstOrDefault(r => r.Kind == MatrixRowKind.Text && r.Number == number);
        string? CellText(MatrixRow? row, int column) => row == null || column >= row.Cells.Count ? null : row.Cells[column].Text;
        NarrativeSection Section(string number, string title, int level = 1, string key = "")
        {
            var section = new NarrativeSection { Number = number, Title = title, Level = level, Key = key };
            report.Sections.Add(section);
            return section;
        }

        // Bullets for every incident line, then the closing sentence for columns that had data but no incident.
        void Incidents(NarrativeSection section, string rowNumber, string closing, string? noneAnywhere = null)
        {
            var row = Row(rowNumber);
            var items = new List<string>();
            int withData = 0, clean = 0;
            foreach (var i in units)
            {
                var text = CellText(row, i);
                if (!HasData(text)) continue;
                withData++;
                var lines = Items(text);
                if (lines.Count == 0) clean++;
                items.AddRange(lines);
            }
            section.IncidentCount = items.Count;
            if (withData == 0) { section.Blocks.Add(Paragraph("መረጃ አልተገኘም።")); return; }
            if (items.Count > 0) section.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: items));
            if (clean > 0)
                section.Blocks.Add(Paragraph(items.Count == 0 ? noneAnywhere ?? $"{scopePhrase} {closing}" : $"{others} {closing}"));
        }

        // ------------------------------------------------------------------ 1. Rain
        var rain = Section("1", "የዝናብ ሁኔታ", key: "rain");
        int Count(string number, int column) => Row(number) is { } row && column < row.Cells.Count ? row.Cells[column].Count ?? 0 : 0;
        foreach (var i in units)
            report.RainRows.Add(new NarrativeRainRow(matrix.Units[i].Name, Count("1.1", i), Count("1.2", i), Count("1.3", i), Count("1.4", i)));
        int full = report.RainRows.Sum(x => x.Full), partial = report.RainRows.Sum(x => x.Partial),
            none = report.RainRows.Sum(x => x.None), noData = report.RainRows.Sum(x => x.NoData);
        rain.Blocks.Add(Paragraph(
            $"ባሳለፍነው ሳምንት {scopePhrase} የነበረው የዝናብ ሁኔታ ሲታይ ከ{unitsPlural} በተሰበሰበው መረጃ መሰረት ዝናብ ሙሉ በሙሉ ያገኙ {N(full)} ወረዳዎች፣ " +
            $"በከፊል ያገኙ {N(partial)} ወረዳዎች፣ ዝናብ ያላገኙ {N(none)} ወረዳዎች፣ በተለያየ ምክንያት መረጃ ያልተገኘላቸው ደግሞ {N(noData)} ወረዳዎች ሲሆኑ " +
            $"በድምሩ {N(full + partial + none + noData)} ወረዳዎች ናቸው።"));
        rain.IncidentCount = none;
        if (report.RainRows.Count > 1)
        {
            static string Cell(int value) => value == 0 ? "" : N(value);
            rain.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Table,
                Headers: ["ተ/ቁ", unitWord, "ሙሉ በሙሉ ዝናብ ያገኙ ወረዳዎች", "በከፊል ዝናብ ያገኙ ወረዳዎች", "ዝናብ ያላገኙ ወረዳዎች", "መረጃ ያልተገኘላቸው ወረዳዎች", "ድምር"],
                Rows: report.RainRows.Select((x, n) => (IReadOnlyList<string>)new List<string> { N(n + 1), x.Unit, Cell(x.Full), Cell(x.Partial), Cell(x.None), Cell(x.NoData), N(x.Total) }).ToList(),
                Footer: ["", "ድምር", N(full), N(partial), N(none), N(noData), N(full + partial + none + noData)]));
        }
        var amounts = units.Select(i => (Unit: UnitName(i), Text: CellText(Row("1.5"), i))).Where(x => HasData(x.Text)).ToList();
        if (amounts.Count > 0)
        {
            rain.Blocks.Add(Paragraph("የዝናቡ መጠንና ስርጭት፡"));
            rain.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: amounts.GroupBy(x => x.Text!)
                .Select(g => $"{string.Join("፣ ", g.Select(x => x.Unit))}፡ {g.Key}").ToList()));
        }
        var hazards = units.SelectMany(i => Items(CellText(Row("1.6"), i)).Concat(Items(CellText(Row("1.7"), i)))).ToList();
        if (hazards.Count > 0)
        {
            rain.Blocks.Add(Paragraph("በከፍተኛ ዝናብ/ጎርፍና በድርቅ የተጎዱ ቀበሌዎች፡"));
            rain.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: hazards));
        }

        // ------------------------------------------------------------------ 2. Farming and crops
        var farming = Section("2", "የእርሻ ሥራ እንቅስቃሴ", key: "farming");
        (decimal Meher, decimal Belg) Farm(string number)
        {
            var row = matrix.Rows.FirstOrDefault(r => r.Kind == MatrixRowKind.Pair && r.Number == number);
            if (row == null) return (0, 0);
            var cells = totalColumn is { } t ? new List<MatrixCell> { row.Cells[t] } : units.Select(i => row.Cells[i]).ToList();
            return (cells.Sum(c => c.Left ?? 0), cells.Sum(c => c.Right ?? 0));
        }
        var plan = Farm("2.1"); var ploughed = Farm("2.2"); var sown = Farm("2.3"); var harvested = Farm("2.4"); var residual = Farm("2.5");
        report.Meher = new NarrativeFarming { Plan = plan.Meher, Ploughed = ploughed.Meher, Sown = sown.Meher, Harvested = harvested.Meher, ResidualMoisture = residual.Meher };
        report.Belg = new NarrativeFarming { Plan = plan.Belg, Ploughed = ploughed.Belg, Sown = sown.Belg, Harvested = harvested.Belg, ResidualMoisture = residual.Belg };
        string FarmingSentence(string season, NarrativeFarming f)
        {
            var text = $"የ{season} እርሻ ሥራ እንቅስቃሴን በተመለከተ {scopePhrase} አጠቃላይ ዕቅድ {N(f.Plan)} ሄ/ር ሲሆን ከዚህ ውስጥ {N(f.Ploughed)} ሄ/ር" +
                       (f.PloughedPercent is { } p ? $" ({N(p)}%)" : "") + " ማረስ ተችሏል።";
            if (f.Sown > 0) text += $" ከታረሰው ውስጥ {N(f.Sown)} ሄ/ር" + (f.SownPercent is { } s ? $" ({N(s)}%)" : "") + " በዘር መሸፈን ተችሏል።";
            if (f.ResidualMoisture > 0) text += $" በቀሪ እርጥበት {N(f.ResidualMoisture)} ሄ/ር ተዘርቷል።";
            if (f.Harvested > 0) text += $" {N(f.Harvested)} ሄ/ር ታጭዶ ተሰብስቧል።";
            return text;
        }
        if (report.Meher.HasData) farming.Blocks.Add(Paragraph(FarmingSentence("መኸር", report.Meher)));
        if (report.Belg.HasData) farming.Blocks.Add(Paragraph(FarmingSentence("በልግ", report.Belg)));
        if (!report.Meher.HasData && !report.Belg.HasData) farming.Blocks.Add(Paragraph("የእርሻ ሥራ መረጃ አልተገኘም።"));
        var farmingNotes = units.SelectMany(i => Items(CellText(Row("2.6"), i))).ToList();
        if (farmingNotes.Count > 0) farming.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: farmingNotes));

        var crop = Section("2.1", "የሰብል ሁኔታ", 2, "crop");
        var cropLines = units.Select(i => (Unit: UnitName(i), Text: CellText(Row("3.1"), i))).Where(x => HasData(x.Text)).ToList();
        if (cropLines.Count == 0) crop.Blocks.Add(Paragraph("መረጃ አልተገኘም።"));
        else
        {
            crop.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: cropLines.GroupBy(x => Summary(x.Text))
                .Select(g => $"{string.Join("፣ ", g.Select(x => x.Unit))}፡ {g.Key}").ToList()));
            var notes = cropLines.SelectMany(x => Items(x.Text)).ToList();
            if (notes.Count > 0) crop.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: notes));
        }
        Incidents(Section("2.2", "የሰብል ተባይና በሽታ", 2, "crop-pest"), "3.2", "የሰብል ተባይና በሽታ ክስተት የለም።");

        // ------------------------------------------------------------------ 3. Livestock
        Section("3", "የእንስሳት ሁኔታ");
        Incidents(Section("3.1", "የእንስሳት መጠጥ ውሃ እና የግጦሽ ሁኔታ", 2, "livestock-water"), "4.1", "በዚህ ሳምንት የታየ የእንስሳት መጠጥ ውሃ እና የግጦሽ አቅርቦት ችግር የለም።");
        Incidents(Section("3.2", "የእንስሳት የሰውነት አቋም ሁኔታ", 2, "body-condition"), "4.2", "የእንስሳት የሰውነት አቋም መካከለኛ በሚል ደረጃ ላይ ይገኛል።");
        Incidents(Section("3.3", "የእንስሳት ጤና ሁኔታ", 2, "animal-health"), "4.3", "ባሳለፍነው ሳምንት በወረርሽኝ መልክ የተከሰተ የእንስሳት የጤና ችግር የለም።");

        // ------------------------------------------------------------------ 4–6. People
        Incidents(Section("4", "የሰው ጤና ሁኔታ", key: "human-health"), "5.1", "በወረርሽኝ መልክ የተከሰተ የሰው በሽታ የለም።");
        Incidents(Section("5", "የሰው መጠጥ ውሃ ሁኔታ", key: "human-water"), "4.4", "የሰው መጠጥ ውሃ እጥረት የለም።");
        Incidents(Section("6", "የትምህርት ሁኔታ", key: "education"), "6.1", "በዚህ ሳምንት ያቋረጡ ተማሪዎችና የተዘጋ ትምህርት ቤት የለም።");

        // ------------------------------------------------------------------ 7. Market
        Section("7", "የገበያ ሁኔታ");
        void Supply(NarrativeSection section, string rowNumber, string subject)
        {
            var lines = units.Select(i => (Unit: UnitName(i), Text: CellText(Row(rowNumber), i))).Where(x => HasData(x.Text)).ToList();
            if (lines.Count == 0) { section.Blocks.Add(Paragraph("መረጃ አልተገኘም።")); return; }
            var groups = lines.GroupBy(x => Summary(x.Text)).ToList();
            if (groups.Count == 1)
                section.Blocks.Add(Paragraph($"መረጃ በተገኘባቸው {(lines.Count > 1 ? $"በሁሉም {unitsPlural}" : lines[0].Unit)} {subject} ከባለፈው ሳምንት ጋር ሲነጻጸር {groups[0].Key}።"));
            else
                section.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets,
                    Items: groups.Select(g => $"{string.Join("፣ ", g.Select(x => x.Unit))}፡ {subject} {g.Key}").ToList()));
            var reasons = lines.SelectMany(x => Items(x.Text)).ToList();
            if (reasons.Count > 0) section.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Bullets, Items: reasons));
        }
        Supply(Section("7.1", "የእህል አቅርቦት ሁኔታ", 2, "grain-supply"), "7.2", "የእህል አቅርቦት");
        Supply(Section("7.2", "የእንስሳት አቅርቦት ሁኔታ", 2, "animal-supply"), "7.3", "የእንስሳት አቅርቦት");
        var prices = Section("7.3", "የእህልና የእንስሳት ዋጋ (አማካይ)", 2, "prices");
        void PriceTable(string sectionNumber, string typeHeader, string unitText)
        {
            var start = matrix.Rows.FindIndex(r => r.Kind == MatrixRowKind.Section && r.Number == sectionNumber);
            if (start < 0) return;
            var rows = new List<IReadOnlyList<string>>();
            for (var r = start + 1; r < matrix.Rows.Count && matrix.Rows[r].Kind == MatrixRowKind.Pair; r++)
            {
                var row = matrix.Rows[r];
                decimal? Average(Func<MatrixCell, decimal?> pick)
                {
                    if (totalColumn is { } t) return pick(row.Cells[t]);
                    var values = units.Select(i => pick(row.Cells[i])).Where(v => v > 0).Select(v => v!.Value).ToList();
                    return values.Count == 0 ? null : Math.Round(values.Average(), 0);
                }
                var previous = Average(c => c.Left);
                var current = Average(c => c.Right);
                if (previous == null && current == null) continue;
                var change = previous > 0 && current > 0 ? (current - previous) / previous * 100 : null;
                rows.Add([row.Label, previous.HasValue ? N(previous.Value) : "-", current.HasValue ? N(current.Value) : "-",
                    change.HasValue ? $"{(change > 0 ? "+" : "")}{N(Math.Round(change.Value, 1))}%" : "-"]);
            }
            if (rows.Count > 0)
                prices.Blocks.Add(new NarrativeBlock(NarrativeBlockKind.Table,
                    Headers: [typeHeader, $"ያለፈው ሳምንት ({unitText})", $"በዚህ ሳምንት ({unitText})", "ለውጥ"], Rows: rows));
        }
        PriceTable("7.4", "የእህል አይነት", "ብር/ኩንታል");
        PriceTable("7.5", "የእንስሳት አይነት", "ብር/በአንድ");
        if (prices.Blocks.Count == 0) prices.Blocks.Add(Paragraph("የዋጋ መረጃ አልተገኘም።"));

        // ------------------------------------------------------------------ 8–11. Migration, hazards, aid, other
        Incidents(Section("8", "ስደት / ሞት", key: "migration-death"), "9.1", "በሳምንቱ ስደት/ሞት የለም።");
        Incidents(Section("9", "በሳምንቱ የተከሰቱ አደጋዎች", key: "hazards"), "8.1", "በሳምንቱ የተከሰተ አደጋ የለም።");
        Incidents(Section("10", "በእርዳታ አቅርቦትና ስርጭት", key: "aid"), "10.1", "በእርዳታ አቅርቦትና ስርጭት ላይ የታየ ችግር የለም።");
        Incidents(Section("11", "ሌሎች ችግሮች", key: "other"), "11.1", "ሌላ ያልተገለፀ ችግር የለም።");

        return report;
    }

    /// <summary>Splits a region-wide grid with woreda columns (see <c>woredaColumns</c>) into one narrative per zone.</summary>
    public static List<WeeklyNarrativeReport> BuildPerZone(WeeklyMatrixReport woredaGrid) =>
        woredaGrid.Units.Select((unit, index) => (unit, index)).Where(x => !x.unit.IsTotal)
            .GroupBy(x => x.unit.Group).OrderBy(g => g.Key)
            .Select(g => Build(woredaGrid, g.Key, ReportScopeLevel.Zone, g.Select(x => x.index).ToList()))
            .ToList();

    internal static bool HasData(string? text) =>
        !string.IsNullOrWhiteSpace(text) && text != WeeklyMatrixReportService.NotReported && text != WeeklyMatrixReportService.NoData;

    /// <summary>The incident lines of a cell ("• …"), without the bullet.</summary>
    internal static List<string> Items(string? text) => string.IsNullOrEmpty(text) ? [] :
        text.Split('\n').Where(x => x.StartsWith(Bullet, StringComparison.Ordinal)).Select(x => x[Bullet.Length..].Trim()).ToList();

    /// <summary>The non-bullet lines of a cell (its summary, e.g. a status).</summary>
    internal static string Summary(string? text) => string.IsNullOrEmpty(text) ? "" :
        string.Join(" ", text.Split('\n').Where(x => !x.StartsWith(Bullet, StringComparison.Ordinal)).Select(x => x.Trim())).Trim();

    private static NarrativeBlock Paragraph(string text) => new(NarrativeBlockKind.Paragraph, Text: text);

    private static int CountWoredas(WeeklyMatrixReport matrix, int column) =>
        matrix.Rows.Where(r => r.Kind == MatrixRowKind.Text && r.Number is "1.1" or "1.2" or "1.3" or "1.4")
            .Sum(r => column < r.Cells.Count ? r.Cells[column].Count ?? 0 : 0);

    private static int CountReported(MatrixRow? row, int column) =>
        row != null && column < row.Cells.Count ? row.Cells[column].Count ?? 0 : 0;

    private static string N(decimal value) => value.ToString(value % 1 == 0 ? "#,0" : "#,0.#", CultureInfo.InvariantCulture);
    private static string N(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);
}
