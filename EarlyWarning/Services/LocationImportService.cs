using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Services;

public enum ImportIssueSeverity { Error, Warning }

public sealed record LocationImportIssue(int Row, ImportIssueSeverity Severity, string Message);

public sealed class LocationImportResult
{
    public int TotalRows { get; set; }
    public int ToInsert { get; set; }
    public int ToUpdate { get; set; }
    /// <summary>True only when the rows were actually saved (commit requested and no errors).</summary>
    public bool Committed { get; set; }
    public List<LocationImportIssue> Issues { get; } = [];
    public bool HasErrors => Issues.Any(x => x.Severity == ImportIssueSeverity.Error);
}

public interface ILocationImportService
{
    /// <summary>
    /// Reads a .xlsx or .csv location list, validates it against the hierarchy (country → region → zone → woreda → kebele)
    /// and, when <paramref name="commit"/> is true and nothing failed, inserts/updates all rows in one SaveChanges
    /// (all-or-nothing). Rows reference their parent through ParentId: another row's Id in the same file, or an
    /// existing location's Id in the database.
    /// </summary>
    Task<LocationImportResult> ImportAsync(Stream file, string fileName, bool commit, CancellationToken ct = default);

    /// <summary>UTF-8 CSV with the expected columns and example rows.</summary>
    byte[] BuildTemplateCsv();

    /// <summary>
    /// Excel template: an empty "Locations" sheet to fill (first sheet = the one imported) with drop-downs and range checks,
    /// an instructions sheet with examples, and optionally a sheet listing every existing location with its Id for ParentId.
    /// </summary>
    Task<byte[]> BuildTemplateExcelAsync(bool includeExistingLocations = true, CancellationToken ct = default);

    /// <summary>UTF-8 CSV of every location in the import format (round-trips through <see cref="ImportAsync"/>).</summary>
    Task<byte[]> ExportCsvAsync(CancellationToken ct = default);
}

public sealed class LocationImportService(EarlyWarningDbContext db) : ILocationImportService
{
    public const int MaxRows = 20_000;

    // Column order used by the template and export.
    private static readonly string[] Columns =
        ["Id", "ParentId", "LocationName", "LocationAmharicName", "LocationCode", "PhoneNumber", "Level", "CardHeaderTitle", "IsActive", "Latitude", "Longitude"];

    // Accepted header spellings (compared after lower-casing and removing spaces, '_' and '-').
    private static readonly Dictionary<string, string> HeaderAliases = BuildAliases(new()
    {
        ["Id"] = ["id", "locationid", "guid"],
        ["ParentId"] = ["parentid", "parent", "parentlocationid"],
        ["LocationName"] = ["locationname", "name", "nameen", "englishname"],
        ["LocationAmharicName"] = ["locationamharicname", "amharicname", "nameam", "የቦታውስም"],
        ["LocationCode"] = ["locationcode", "code", "ኮድ"],
        ["PhoneNumber"] = ["phonenumber", "phone", "ስልክ"],
        ["Level"] = ["level", "ደረጃ"],
        ["CardHeaderTitle"] = ["cardheadertitle", "header", "ሔደር"],
        ["IsActive"] = ["isactive", "active"],
        ["Latitude"] = ["latitude", "lat", "y", "ኬክሮስ"],
        ["Longitude"] = ["longitude", "lon", "lng", "long", "x", "ኬንትሮስ"],
    });

    public async Task<LocationImportResult> ImportAsync(Stream file, string fileName, bool commit, CancellationToken ct = default)
    {
        var result = new LocationImportResult();
        List<Dictionary<string, string>> rawRows;
        try
        {
            rawRows = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".xlsx" => ReadExcel(file),
                ".csv" => ReadCsv(file),
                _ => throw new InvalidDataException("የፋይሉ አይነት .xlsx ወይም .csv መሆን አለበት።")
            };
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or FormatException or InvalidOperationException)
        {
            result.Issues.Add(new(0, ImportIssueSeverity.Error, ex is InvalidDataException ? ex.Message : $"ፋይሉን ማንበብ አልተቻለም፡ {ex.Message}"));
            return result;
        }

        if (rawRows.Count == 0)
        {
            result.Issues.Add(new(0, ImportIssueSeverity.Error, "ፋይሉ ምንም የአድራሻ ረድፍ አልያዘም።"));
            return result;
        }
        if (rawRows.Count > MaxRows)
        {
            result.Issues.Add(new(0, ImportIssueSeverity.Error, $"በአንድ ጊዜ ከ{MaxRows:#,0} ረድፍ በላይ ማስገባት አይቻልም።"));
            return result;
        }
        var headers = rawRows[0].Keys.ToHashSet();
        foreach (var required in new[] { "LocationCode", "Level" })
            if (!headers.Contains(required))
                result.Issues.Add(new(1, ImportIssueSeverity.Error, $"አስፈላጊው ዓምድ '{required}' አልተገኘም።"));
        if (!headers.Contains("LocationName") && !headers.Contains("LocationAmharicName"))
            result.Issues.Add(new(1, ImportIssueSeverity.Error, "'LocationName' ወይም 'LocationAmharicName' ዓምድ ያስፈልጋል።"));
        if (result.HasErrors) return result;

        result.TotalRows = rawRows.Count;
        var rows = rawRows.Select((cells, i) => Parse(cells, i + 2, result)).ToList(); // +2: header is row 1

        // ---- resolve identities: Guid ids are used as-is; other ids get a new Guid; existing rows match by Id, then (Level, Code)
        var existing = await db.Locations.ToListAsync(ct);
        var existingById = existing.ToDictionary(x => x.Id);
        var existingByCode = existing.Where(x => x.Level.HasValue && !string.IsNullOrWhiteSpace(x.LocationCode))
            .GroupBy(x => (x.Level!.Value, Code: x.LocationCode!.Trim().ToUpperInvariant()))
            .ToDictionary(g => g.Key, g => g.First());

        var byKey = new Dictionary<string, ParsedRow>(StringComparer.OrdinalIgnoreCase);
        var byCode = new Dictionary<(LocationLevel, string), ParsedRow>();
        foreach (var row in rows.Where(r => r.Level.HasValue && r.Code != null))
        {
            if (row.Key != null && !byKey.TryAdd(row.Key, row))
                result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, $"Id '{row.Key}' በፋይሉ ውስጥ ከረድፍ {byKey[row.Key].RowNumber} ጋር ተደጋግሟል።"));
            var codeKey = (row.Level!.Value, row.Code!.ToUpperInvariant());
            if (!byCode.TryAdd(codeKey, row))
                result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, $"ኮድ '{row.Code}' ለደረጃ {row.Level} በፋይሉ ውስጥ ከረድፍ {byCode[codeKey].RowNumber} ጋር ተደጋግሟል።"));

            Locations? match = null;
            if (row.Key != null && Guid.TryParse(row.Key, out var guid))
            {
                row.Id = guid;
                existingById.TryGetValue(guid, out match);
            }
            if (existingByCode.TryGetValue(codeKey, out var codeMatch))
            {
                if (match != null && match.Id != codeMatch.Id)
                    result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, $"ኮድ '{row.Code}' በመረጃ ቋቱ ውስጥ ለሌላ አድራሻ ({Name(codeMatch)}) ተመዝግቧል።"));
                else if (match == null && row.Id != Guid.Empty && row.Id != codeMatch.Id)
                    result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, $"ኮድ '{row.Code}' በመረጃ ቋቱ ውስጥ ለሌላ Id ያለው አድራሻ ({Name(codeMatch)}) ተመዝግቧል።"));
                match ??= codeMatch;
            }
            row.Existing = match;
            if (row.Id == Guid.Empty) row.Id = match?.Id ?? Guid.NewGuid();
        }

        // ---- parent links: same-file row first (by its Id), then an existing database location (by Guid)
        foreach (var row in rows.Where(r => r.Level.HasValue))
        {
            var level = row.Level!.Value;
            if (level == LocationLevel.ሀገር)
            {
                if (row.ParentKey != null)
                    result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, "የሀገር ደረጃ አድራሻ ParentId ሊኖረው አይገባም።"));
                continue;
            }
            if (row.ParentKey == null)
            {
                result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, "ParentId ያስፈልጋል (ከሀገር ደረጃ በስተቀር)።"));
                continue;
            }

            LocationLevel? parentLevel = null;
            if (byKey.TryGetValue(row.ParentKey, out var parentRow) && parentRow != row)
            {
                row.ParentId = parentRow.Id;
                parentLevel = parentRow.Level;
            }
            else if (Guid.TryParse(row.ParentKey, out var parentGuid) && existingById.TryGetValue(parentGuid, out var parentLocation))
            {
                row.ParentId = parentGuid;
                parentLevel = parentLocation.Level;
            }
            else
            {
                result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error, $"ParentId '{row.ParentKey}' በፋይሉም ሆነ በመረጃ ቋቱ ውስጥ አልተገኘም።"));
                continue;
            }

            // The parent must sit at a higher level (lower enum value). Skipping a level is allowed because
            // city administrations hold kebeles/woredas directly; strictly higher parents also rule out cycles.
            if (parentLevel == null || parentLevel >= level)
                result.Issues.Add(new(row.RowNumber, ImportIssueSeverity.Error,
                    $"የወላጅ ደረጃ ስህተት ነው፡ የ{level} ወላጅ ከ{level} ከፍ ያለ ደረጃ መሆን አለበት (የተሰጠው ወላጅ {parentLevel?.ToString() ?? "ደረጃ የሌለው"} ነው)።"));
        }

        var valid = rows.Where(r => r.Level.HasValue && r.Code != null).ToList();
        result.ToInsert = valid.Count(r => r.Existing == null);
        result.ToUpdate = valid.Count(r => r.Existing != null);
        if (!commit || result.HasErrors) return result;

        // ---- apply: parents before children; one SaveChanges = one transaction (all-or-nothing)
        foreach (var row in valid.OrderBy(r => r.Level))
        {
            var target = row.Existing ?? new Locations { Id = row.Id };
            // LocationName is required in the database, so it falls back to the Amharic name;
            // LocationAmharicName is optional and stays empty when the file has none.
            target.LocationName = row.Name ?? row.AmharicName;
            target.LocationAmharicName = row.AmharicName;
            target.LocationCode = row.Code;
            target.Level = row.Level;
            target.ParentId = row.Level == LocationLevel.ሀገር ? null : row.ParentId;
            // Optional cells only overwrite when filled, so a partial sheet never wipes existing data.
            if (row.Phone != null || row.Existing == null) target.PhoneNumber = row.Phone ?? "";
            if (row.Header != null) target.CardHeaderTitle = row.Header;
            if (row.IsActive.HasValue || row.Existing == null) target.IsActive = row.IsActive ?? true;
            if (row.Latitude.HasValue) { target.Latitude = row.Latitude; target.Longitude = row.Longitude; }
            if (row.Existing == null) db.Locations.Add(target);
        }
        await db.SaveChangesAsync(ct);
        result.Committed = true;
        return result;
    }

    public byte[] BuildTemplateCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Columns));
        // Ids may be GUIDs or any unique text; ParentId points at another row's Id or an existing location's Id.
        sb.AppendLine("R-AMH,11111111-1111-1111-1111-111111111111,Amhara,አማራ,AMH,+251-58-000000,ክልል,Amhara Region,true,11.5936,37.3908");
        sb.AppendLine("Z-NG,R-AMH,North Gondar,ሰሜን ጎንደር,NG,+251-58-111111,ዞን,North Gondar Zone,true,12.6030,37.4521");
        sb.AppendLine("W-DBK,Z-NG,Debark,ደባርቅ,DBK,+251-58-222222,ወረዳ,Debark Woreda,true,13.1333,37.9000");
        sb.AppendLine("K-DBK01,W-DBK,Kebele 01,ቀበሌ 01,DBK01,,ቀበሌ,,true,13.1400,37.9050");
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> BuildTemplateExcelAsync(bool includeExistingLocations = true, CancellationToken ct = default)
    {
        // (column, required, description) — in import order.
        (string Column, bool Required, string Help)[] columns =
        [
            ("Id", false, "የረድፉ መለያ፦ GUID ወይም ልዩ ጽሑፍ/ቁጥር (ለምሳሌ W-DBK)። ልጆቹ በ ParentId ይጠቅሱታል። ባዶ ከሆነ በ Level + LocationCode ይታወቃል።"),
            ("ParentId", true, "የወላጅ አድራሻ Id፦ በዚሁ ሉህ ያለ ረድፍ Id ወይም በ'ያሉ አድራሻዎች' ሉህ ያለ GUID። ወላጁ ከፍ ያለ ደረጃ መሆን አለበት። ለሀገር ብቻ ባዶ።"),
            ("LocationName", true, "የእንግሊዝኛ ስም (ቢያንስ አንዱ ስም ያስፈልጋል)።"),
            ("LocationAmharicName", false, "የአማርኛ ስም። ከሌለ ባዶ ይተዉት።"),
            ("LocationCode", true, "በደረጃው ውስጥ ልዩ ኮድ (ለምሳሌ AM-AW-ZIG)። ያለ አድራሻ በዚህ ኮድ ከተገኘ ይሻሻላል።"),
            ("PhoneNumber", false, "ስልክ ቁጥር (አማራጭ)።"),
            ("Level", true, "ከዝርዝሩ ይምረጡ፦ ሀገር / ክልል / ዞን / ወረዳ / ቀበሌ።"),
            ("CardHeaderTitle", false, "የሪፖርት ሔደር ጽሑፍ (አማራጭ)።"),
            ("IsActive", false, "true ወይም false (ባዶ = true)።"),
            ("Latitude", false, "ኬክሮስ በዲግሪ (WGS84)፣ ለምሳሌ 12.603000። ከ Longitude ጋር አብሮ።"),
            ("Longitude", false, "ኬንትሮስ በዲግሪ (WGS84)፣ ለምሳሌ 37.452100። ከ Latitude ጋር አብሮ።"),
        ];
        const int lastRow = MaxRows + 1;
        var required = XLColor.FromHtml("#FCE4D6");
        var optional = XLColor.FromHtml("#DDEBF7");

        using var workbook = new XLWorkbook();

        // ---- 1. Locations (imported sheet — must stay first)
        var sheet = workbook.AddWorksheet("Locations");
        for (var i = 0; i < columns.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = columns[i].Column;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = columns[i].Required ? required : optional;
            cell.GetComment().AddText(columns[i].Help);
        }
        // Keep codes and ids as text so Excel never drops leading zeros (e.g. "009") or reformats GUIDs.
        foreach (var col in new[] { "A", "B", "E", "F" }) sheet.Column(col).Style.NumberFormat.Format = "@";
        sheet.Range($"J2:K{lastRow}").Style.NumberFormat.Format = "0.000000";
        var widths = new[] { 18, 38, 24, 24, 18, 16, 10, 22, 10, 13, 13 };
        for (var i = 0; i < widths.Length; i++) sheet.Column(i + 1).Width = widths[i];
        sheet.SheetView.FreezeRows(1);

        // Hidden list source for drop-downs (more reliable than inline lists with Ethiopic text).
        var lists = workbook.AddWorksheet("Lists");
        var levels = new[] { LocationLevel.ሀገር, LocationLevel.ክልል, LocationLevel.ዞን, LocationLevel.ወረዳ, LocationLevel.ቀበሌ };
        for (var i = 0; i < levels.Length; i++) lists.Cell(i + 1, 1).Value = levels[i].ToString();
        lists.Cell(1, 2).Value = "true";
        lists.Cell(2, 2).Value = "false";
        lists.Visibility = XLWorksheetVisibility.VeryHidden;

        var levelRule = sheet.Range($"G2:G{lastRow}").CreateDataValidation();
        levelRule.List(lists.Range("A1:A5"), true);
        levelRule.ErrorTitle = "Level"; levelRule.ErrorMessage = "ከዝርዝሩ ይምረጡ፦ ሀገር / ክልል / ዞን / ወረዳ / ቀበሌ";
        var activeRule = sheet.Range($"I2:I{lastRow}").CreateDataValidation();
        activeRule.List(lists.Range("B1:B2"), true);
        var latRule = sheet.Range($"J2:J{lastRow}").CreateDataValidation();
        latRule.Decimal.Between(-90, 90);
        latRule.ErrorTitle = "Latitude"; latRule.ErrorMessage = "ኬክሮስ ከ -90 እስከ 90 ባለው ዲግሪ (ለኢትዮጵያ ከ3 እስከ 15 ገደማ)።";
        var lonRule = sheet.Range($"K2:K{lastRow}").CreateDataValidation();
        lonRule.Decimal.Between(-180, 180);
        lonRule.ErrorTitle = "Longitude"; lonRule.ErrorMessage = "ኬንትሮስ ከ -180 እስከ 180 ባለው ዲግሪ (ለኢትዮጵያ ከ33 እስከ 48 ገደማ)።";

        // ---- 2. Instructions + examples (kept off the imported sheet so examples are never imported by accident)
        var help = workbook.AddWorksheet("መመሪያ (Instructions)");
        var row = 1;
        help.Cell(row, 1).Value = "የአድራሻ ዝርዝር ማስገቢያ ቅጽ — መመሪያ";
        help.Cell(row, 1).Style.Font.Bold = true; help.Cell(row, 1).Style.Font.FontSize = 14;
        row += 2;
        string[] steps =
        [
            "1. መረጃውን በ 'Locations' ሉህ ውስጥ ይሙሉ (አንድ አድራሻ በአንድ ረድፍ)። የሉሆቹን ስምና ቅደም ተከተል አይቀይሩ።",
            "2. ብርቱካናማ ዓምዶች አስፈላጊ ናቸው፤ ሰማያዊዎቹ አማራጭ ናቸው። መረጃ የሌለውን ሕዋስ ባዶ ይተዉት።",
            "3. እያንዳንዱ ረድፍ ወላጁን በ ParentId ይጠቅሳል፦ በዚሁ ሉህ ያለ ረድፍ Id ወይም በ 'ያሉ አድራሻዎች' ሉህ ያለ GUID።",
            "4. ሁሉንም ደረጃዎች በአንድ ፋይል (ክልል → ዞን → ወረዳ → ቀበሌ) ወይም በየዞኑ/በየወረዳው በተለያዩ ፋይሎች ማስገባት ይቻላል። የረድፎች ቅደም ተከተል ግድ አይደለም።",
            "5. Latitude/Longitude በዲግሪ (WGS84) ይሁኑ። UTM (X/Y በሜትር) ከሆነ መጀመሪያ ወደ ዲግሪ ይቀይሩ።",
            "6. በሥርዓቱ፦ አድራሻ → 'ከፋይል አስገባ' → ፋይሉን ይምረጡ → 'አረጋግጥ ብቻ' → ስህተቶችን ያስተካክሉ → 'አስገባ'።",
            "7. ተመሳሳይ ፋይል ደግሞ ቢገባ ያሉትን ያሻሽላል እንጂ አይደግምም (በ Id ወይም በ Level + LocationCode ይዛመዳል)።",
        ];
        foreach (var step in steps) help.Cell(row++, 1).Value = step;
        row++;
        help.Cell(row, 1).Value = "ዓምድ"; help.Cell(row, 2).Value = "አስፈላጊ?"; help.Cell(row, 3).Value = "መግለጫ";
        help.Range(row, 1, row, 3).Style.Font.Bold = true;
        help.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
        row++;
        foreach (var c in columns)
        {
            help.Cell(row, 1).Value = c.Column;
            help.Cell(row, 2).Value = c.Required ? "አዎ" : "አማራጭ";
            help.Cell(row, 3).Value = c.Help;
            help.Cell(row, 1).Style.Fill.BackgroundColor = c.Required ? required : optional;
            row++;
        }
        row++;
        help.Cell(row, 1).Value = "ምሳሌ (ይህ ምሳሌ አይገባም — መረጃዎን በ 'Locations' ሉህ ያስገቡ)፦";
        help.Cell(row, 1).Style.Font.Bold = true;
        row++;
        for (var i = 0; i < columns.Length; i++)
        {
            help.Cell(row, i + 1).Value = columns[i].Column;
            help.Cell(row, i + 1).Style.Font.Bold = true;
            help.Cell(row, i + 1).Style.Fill.BackgroundColor = columns[i].Required ? required : optional;
        }
        row++;
        string[][] examples =
        [
            ["Z-NG", "‹የክልሉ GUID ከ 'ያሉ አድራሻዎች'›", "North Gondar", "ሰሜን ጎንደር", "AM-NG", "+251-58-111111", "ዞን", "North Gondar Zone", "true", "12.603000", "37.452100"],
            ["W-DBK", "Z-NG", "Debark", "ደባርቅ", "AM-NG-DBK", "", "ወረዳ", "", "true", "13.133300", "37.900000"],
            ["K-DBK01", "W-DBK", "", "ቀበሌ 01", "AM-NG-DBK-01", "", "ቀበሌ", "", "true", "", ""],
        ];
        foreach (var example in examples)
        {
            for (var i = 0; i < example.Length; i++) help.Cell(row, i + 1).Value = example[i];
            row++;
        }
        help.Column(1).Width = 22; help.Column(2).Width = 34; help.Column(3).Width = 90;
        for (var i = 4; i <= columns.Length; i++) help.Column(i).Width = 16;
        help.Column(3).Style.Alignment.WrapText = true;

        // ---- 3. Existing locations (Ids to use as ParentId)
        if (includeExistingLocations)
        {
            var existing = workbook.AddWorksheet("ያሉ አድራሻዎች (Existing)");
            string[] headers = ["Id", "Level", "LocationName", "LocationAmharicName", "LocationCode", "ParentId", "Parent", "Latitude", "Longitude"];
            for (var i = 0; i < headers.Length; i++) existing.Cell(1, i + 1).Value = headers[i];
            var locations = await db.Locations.AsNoTracking().Include(x => x.Parent).ToListAsync(ct);
            var r = 2;
            foreach (var x in locations.OrderBy(x => x.Level).ThenBy(x => x.Parent?.DisplayName).ThenBy(x => x.LocationCode))
            {
                existing.Cell(r, 1).Value = x.Id.ToString();
                existing.Cell(r, 2).Value = x.Level?.ToString() ?? "";
                existing.Cell(r, 3).Value = x.LocationName ?? "";
                existing.Cell(r, 4).Value = x.LocationAmharicName ?? "";
                existing.Cell(r, 5).Value = x.LocationCode ?? "";
                existing.Cell(r, 6).Value = x.ParentId?.ToString() ?? "";
                existing.Cell(r, 7).Value = x.Parent?.DisplayName ?? "";
                if (x.Latitude.HasValue) existing.Cell(r, 8).Value = x.Latitude.Value;
                if (x.Longitude.HasValue) existing.Cell(r, 9).Value = x.Longitude.Value;
                r++;
            }
            existing.Range(1, 1, Math.Max(r - 1, 1), headers.Length).SetAutoFilter();
            existing.Row(1).Style.Font.Bold = true;
            existing.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E2EFDA");
            existing.SheetView.FreezeRows(1);
            var existingWidths = new[] { 38, 8, 24, 24, 18, 38, 22, 12, 12 };
            for (var i = 0; i < existingWidths.Length; i++) existing.Column(i + 1).Width = existingWidths[i];
            existing.Range($"H2:I{Math.Max(r, 2)}").Style.NumberFormat.Format = "0.000000";
        }

        sheet.SetTabActive();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportCsvAsync(CancellationToken ct = default)
    {
        var locations = await db.Locations.AsNoTracking().OrderBy(x => x.Level).ThenBy(x => x.LocationCode).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Columns));
        foreach (var x in locations)
            sb.AppendLine(string.Join(",", new[]
            {
                x.Id.ToString(), x.ParentId?.ToString() ?? "", x.LocationName ?? "", x.LocationAmharicName ?? "", x.LocationCode ?? "",
                x.PhoneNumber ?? "", x.Level?.ToString() ?? "", x.CardHeaderTitle ?? "", x.IsActive ? "true" : "false",
                x.Latitude?.ToString(CultureInfo.InvariantCulture) ?? "", x.Longitude?.ToString(CultureInfo.InvariantCulture) ?? ""
            }.Select(CsvEscape)));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    // ------------------------------------------------------------------ parsing

    private sealed class ParsedRow
    {
        public int RowNumber { get; init; }
        public string? Key { get; init; }
        public string? ParentKey { get; init; }
        public string? Name { get; init; }
        public string? AmharicName { get; init; }
        public string? Code { get; init; }
        public string? Phone { get; init; }
        public LocationLevel? Level { get; init; }
        public string? Header { get; init; }
        public bool? IsActive { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Locations? Existing { get; set; }
    }

    private static ParsedRow Parse(Dictionary<string, string> cells, int rowNumber, LocationImportResult result)
    {
        string? Cell(string column) => cells.TryGetValue(column, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;
        void Error(string message) => result.Issues.Add(new(rowNumber, ImportIssueSeverity.Error, message));
        void Warn(string message) => result.Issues.Add(new(rowNumber, ImportIssueSeverity.Warning, message));

        var name = Cell("LocationName");
        var amharicName = Cell("LocationAmharicName");
        if (name == null && amharicName == null) Error("የአድራሻው ስም (LocationName ወይም LocationAmharicName) ያስፈልጋል።");
        else if (name == null) Warn("LocationName ባዶ ስለሆነ የአማርኛው ስም ለእንግሊዝኛውም ዓምድ ጥቅም ላይ ይውላል።");
        else if (amharicName == null) Warn("LocationAmharicName ባዶ ሆኖ ይቀመጣል።");

        var code = Cell("LocationCode");
        if (code == null) Error("LocationCode ያስፈልጋል።");

        LocationLevel? level = null;
        var levelText = Cell("Level");
        if (levelText == null) Error("Level ያስፈልጋል።");
        else if ((level = ParseLevel(levelText)) == null) Error($"Level '{levelText}' አይታወቅም (ሀገር/ክልል/ዞን/ወረዳ/ቀበሌ ወይም 0-4)።");

        bool? isActive = null;
        var activeText = Cell("IsActive");
        if (activeText != null && (isActive = ParseBool(activeText)) == null) Error($"IsActive '{activeText}' true/false መሆን አለበት።");

        decimal? lat = ParseCoordinate(Cell("Latitude"), "Latitude", -90, 90, Error);
        decimal? lon = ParseCoordinate(Cell("Longitude"), "Longitude", -180, 180, Error);
        if (lat.HasValue != lon.HasValue)
        {
            Error("Latitude እና Longitude አብረው መሰጠት አለባቸው።");
            lat = lon = null;
        }
        else if (lat.HasValue && (lat < 3m || lat > 15.5m || lon < 32.5m || lon > 48.5m))
            Warn($"ነጥቡ ({lat}, {lon}) ከኢትዮጵያ ክልል ውጭ ይመስላል፤ Latitude/Longitude ተቀያይረው እንደሆነ ያረጋግጡ።");

        return new ParsedRow
        {
            RowNumber = rowNumber, Key = Cell("Id"), ParentKey = Cell("ParentId"), Name = name, AmharicName = amharicName,
            Code = code, Phone = Cell("PhoneNumber"), Level = level, Header = Cell("CardHeaderTitle"), IsActive = isActive,
            Latitude = lat, Longitude = lon
        };
    }

    private static decimal? ParseCoordinate(string? text, string column, decimal min, decimal max, Action<string> error)
    {
        if (text == null) return null;
        if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            error($"{column} '{text}' ቁጥር አይደለም (የአስርዮሽ ነጥብ '.' ይጠቀሙ)።");
            return null;
        }
        if (value < min || value > max)
        {
            error($"{column} {value} ከ{min} እስከ {max} መሆን አለበት።");
            return null;
        }
        return Math.Round(value, 6);
    }

    private static LocationLevel? ParseLevel(string text)
    {
        if (int.TryParse(text, out var n)) return Enum.IsDefined(typeof(LocationLevel), n) ? (LocationLevel)n : null;
        if (Enum.TryParse<LocationLevel>(text, out var parsed) && Enum.IsDefined(parsed)) return parsed;
        return text.ToLowerInvariant() switch
        {
            "country" or "national" => LocationLevel.ሀገር,
            "region" => LocationLevel.ክልል,
            "zone" => LocationLevel.ዞን,
            "woreda" or "district" => LocationLevel.ወረዳ,
            "kebele" => LocationLevel.ቀበሌ,
            _ => null
        };
    }

    private static bool? ParseBool(string text) => text.ToLowerInvariant() switch
    {
        "true" or "1" or "yes" or "y" or "አዎ" or "active" => true,
        "false" or "0" or "no" or "n" or "አይ" or "inactive" => false,
        _ => null
    };

    private static List<Dictionary<string, string>> ReadExcel(Stream file)
    {
        using var workbook = new XLWorkbook(file);
        var sheet = workbook.Worksheets.FirstOrDefault() ?? throw new InvalidDataException("የExcel ፋይሉ ምንም ሉህ (sheet) የለውም።");
        var used = sheet.RangeUsed();
        if (used == null) return [];
        var table = used.Rows().Select(r => r.Cells(1, used.ColumnCount()).Select(CellText).ToList()).ToList();
        return ToRecords(table);
    }

    private static string CellText(IXLCell cell)
    {
        var value = cell.Value;
        if (value.IsNumber) return value.GetNumber().ToString(CultureInfo.InvariantCulture);
        if (value.IsBoolean) return value.GetBoolean() ? "true" : "false";
        return cell.GetFormattedString();
    }

    private static List<Dictionary<string, string>> ReadCsv(Stream file)
    {
        using var reader = new StreamReader(file, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        var firstLine = text.Split('\n', 2)[0];
        var delimiter = new[] { ',', ';', '\t' }.OrderByDescending(d => firstLine.Count(c => c == d)).First();
        return ToRecords(ParseCsv(text, delimiter));
    }

    /// <summary>RFC 4180 CSV: quoted fields, doubled quotes, embedded delimiters and line breaks.</summary>
    private static List<List<string>> ParseCsv(string text, char delimiter)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else field.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == delimiter) { row.Add(field.ToString()); field.Clear(); }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                row.Add(field.ToString()); field.Clear();
                rows.Add(row); row = [];
            }
            else field.Append(c);
        }
        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
        return rows;
    }

    /// <summary>First row = headers (mapped through aliases; unknown columns ignored); blank rows skipped.</summary>
    private static List<Dictionary<string, string>> ToRecords(List<List<string>> table)
    {
        if (table.Count == 0) return [];
        var columns = table[0].Select(h => HeaderAliases.GetValueOrDefault(NormalizeHeader(h))).ToList();
        var duplicate = columns.Where(c => c != null).GroupBy(c => c).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null) throw new InvalidDataException($"ዓምድ '{duplicate.Key}' ከአንድ ጊዜ በላይ ተሰጥቷል።");
        if (columns.All(c => c == null)) throw new InvalidDataException("የዓምድ ርዕሶች አልታወቁም፤ የቀረበውን ቅጽ (template) ይጠቀሙ።");

        return table.Skip(1)
            .Where(r => r.Any(v => !string.IsNullOrWhiteSpace(v)))
            .Select(r => columns.Select((c, i) => (c, v: i < r.Count ? r[i] : ""))
                .Where(x => x.c != null).ToDictionary(x => x.c!, x => x.v))
            .ToList();
    }

    private static string NormalizeHeader(string header) =>
        new string(header.Trim().ToLowerInvariant().Where(ch => !char.IsWhiteSpace(ch) && ch != '_' && ch != '-').ToArray());

    private static Dictionary<string, string> BuildAliases(Dictionary<string, string[]> map) =>
        map.SelectMany(kv => kv.Value.Select(alias => (alias, kv.Key))).ToDictionary(x => x.alias, x => x.Key);

    private static string CsvEscape(string value) =>
        value.IndexOfAny([',', '"', '\n', '\r', ';']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;

    private static string Name(Locations x) => x.LocationAmharicName ?? x.LocationName ?? x.Id.ToString();
}
