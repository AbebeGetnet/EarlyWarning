using System.Text;
using ClosedXML.Excel;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Services;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Tests;

public class LocationImportTests : IDisposable
{
    // Seeded by EarlyWarningDbContext.HasData (country "Ethiopia").
    private static readonly Guid Ethiopia = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string Header = "Id,ParentId,LocationName,LocationAmharicName,LocationCode,PhoneNumber,Level,CardHeaderTitle,IsActive,Latitude,Longitude";

    private readonly EarlyWarningDbContext _db;
    private readonly LocationImportService _importer;

    public LocationImportTests()
    {
        _db = new EarlyWarningDbContext(new DbContextOptionsBuilder<EarlyWarningDbContext>()
            .UseInMemoryDatabase($"location-import-{Guid.NewGuid()}").Options);
        _db.Database.EnsureCreated();
        _importer = new LocationImportService(_db);
    }

    public void Dispose() => _db.Dispose();

    private static string Hierarchy() => string.Join("\n",
        Header,
        $"R-T,{Ethiopia},Test Region,ሙከራ ክልል,TR1,+251-1,ክልል,Test Region,true,11.5936,37.3908",
        "Z-T,R-T,Test Zone,ሙከራ ዞን,TZ1,,ዞን,,true,12.6030,37.4521",
        "W-T,Z-T,Test Woreda,ሙከራ ወረዳ,TW1,,ወረዳ,,true,13.1333,37.9000",
        "K-T,W-T,\"Kebele 01, North\",ቀበሌ 01,TK1,,ቀበሌ,,true,,");

    private Task<LocationImportResult> ImportCsv(string csv, bool commit) =>
        _importer.ImportAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "locations.csv", commit);

    [Fact]
    public async Task Csv_ImportsSelfReferencingHierarchy_WithCoordinates()
    {
        var result = await ImportCsv(Hierarchy(), commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        Assert.True(result.Committed);
        Assert.Equal(4, result.ToInsert);

        var region = await _db.Locations.SingleAsync(x => x.LocationCode == "TR1");
        var zone = await _db.Locations.SingleAsync(x => x.LocationCode == "TZ1");
        var woreda = await _db.Locations.SingleAsync(x => x.LocationCode == "TW1");
        var kebele = await _db.Locations.SingleAsync(x => x.LocationCode == "TK1");
        Assert.Equal(Ethiopia, region.ParentId);
        Assert.Equal(region.Id, zone.ParentId);
        Assert.Equal(zone.Id, woreda.ParentId);
        Assert.Equal(woreda.Id, kebele.ParentId);
        Assert.Equal(LocationLevel.ወረዳ, woreda.Level);
        Assert.Equal(13.1333m, woreda.Latitude);
        Assert.Equal(37.9m, woreda.Longitude);
        Assert.Null(kebele.Latitude);                      // coordinates are optional
        Assert.Equal("Kebele 01, North", kebele.LocationName); // quoted CSV field with a comma
    }

    [Fact]
    public async Task ValidateOnly_SavesNothing()
    {
        var result = await ImportCsv(Hierarchy(), commit: false);

        Assert.False(result.HasErrors);
        Assert.False(result.Committed);
        Assert.Equal(4, result.ToInsert);
        Assert.False(await _db.Locations.AnyAsync(x => x.LocationCode == "TR1"));
    }

    [Fact]
    public async Task ReImport_UpdatesExistingRows_InsteadOfDuplicating()
    {
        await ImportCsv(Hierarchy(), commit: true);
        var count = await _db.Locations.CountAsync();

        var result = await ImportCsv(Hierarchy().Replace("Test Zone", "Renamed Zone"), commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        Assert.Equal(0, result.ToInsert);
        Assert.Equal(4, result.ToUpdate);
        Assert.Equal(count, await _db.Locations.CountAsync());
        Assert.Equal("Renamed Zone", (await _db.Locations.SingleAsync(x => x.LocationCode == "TZ1")).LocationName);
    }

    [Fact]
    public async Task ParentAtSameOrLowerLevel_IsRejected_AndNothingIsSaved()
    {
        // Woreda pointing at a kebele (a lower level) — an upside-down link.
        var csv = Hierarchy().Replace("W-T,Z-T,", "W-T,K-T,");

        var result = await ImportCsv(csv, commit: true);

        Assert.True(result.HasErrors);
        Assert.False(result.Committed);
        Assert.Contains(result.Issues, x => x.Row == 4 && x.Severity == ImportIssueSeverity.Error);
        Assert.False(await _db.Locations.AnyAsync(x => x.LocationCode == "TR1")); // all-or-nothing
    }

    [Fact]
    public async Task SkippingALevel_IsAllowed_ForCityAdministrations()
    {
        // A kebele directly under a zone-level city administration (exists in real data).
        var result = await ImportCsv(Hierarchy() + "\nK-CITY,Z-T,City Kebele,ከተማ ቀበሌ,TKC,,ቀበሌ,,true,,", commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        var zone = await _db.Locations.SingleAsync(x => x.LocationCode == "TZ1");
        Assert.Equal(zone.Id, (await _db.Locations.SingleAsync(x => x.LocationCode == "TKC")).ParentId);
    }

    [Fact]
    public async Task MissingAmharicName_StaysEmpty_AndMissingEnglishNameFallsBackToAmharic()
    {
        var csv = string.Join("\n", Header,
            $"R-E,{Ethiopia},English Only,,EO1,,ክልል,,true,,",
            "Z-A,R-E,,አማርኛ ብቻ,AO1,,ዞን,,true,,");

        var result = await ImportCsv(csv, commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        var englishOnly = await _db.Locations.SingleAsync(x => x.LocationCode == "EO1");
        Assert.Equal("English Only", englishOnly.LocationName);
        Assert.Null(englishOnly.LocationAmharicName);
        var amharicOnly = await _db.Locations.SingleAsync(x => x.LocationCode == "AO1");
        Assert.Equal("አማርኛ ብቻ", amharicOnly.LocationAmharicName);
        Assert.Equal("አማርኛ ብቻ", amharicOnly.LocationName); // LocationName is NOT NULL in the database
    }

    [Fact]
    public async Task UnknownParent_IsRejected()
    {
        var result = await ImportCsv(Hierarchy().Replace("K-T,W-T,", "K-T,NOPE,"), commit: false);

        Assert.Contains(result.Issues, x => x.Row == 5 && x.Message.Contains("NOPE"));
    }

    [Fact]
    public async Task ParentCanBeAnExistingDatabaseLocation()
    {
        await ImportCsv(Hierarchy(), commit: true);
        var woreda = await _db.Locations.SingleAsync(x => x.LocationCode == "TW1");

        var result = await ImportCsv($"{Header}\nK2,{woreda.Id},Kebele 02,ቀበሌ 02,TK2,,ቀበሌ,,true,13.15,37.91", commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        Assert.Equal(woreda.Id, (await _db.Locations.SingleAsync(x => x.LocationCode == "TK2")).ParentId);
    }

    [Fact]
    public async Task DuplicateCodesAndIds_InTheFile_AreRejected()
    {
        var csv = Hierarchy() + "\nZ-T,R-T,Another Zone,ሌላ ዞን,TZ1,,ዞን,,true,,";

        var result = await ImportCsv(csv, commit: false);

        Assert.Contains(result.Issues, x => x.Row == 6 && x.Message.Contains("Z-T"));
        Assert.Contains(result.Issues, x => x.Row == 6 && x.Message.Contains("TZ1"));
    }

    [Fact]
    public async Task Coordinates_AreValidated()
    {
        var csv = string.Join("\n", Header,
            $"A,{Ethiopia},A,ሀ,CA,,ክልል,,true,12.5,",          // latitude without longitude
            $"B,{Ethiopia},B,ለ,CB,,ክልል,,true,95,37",          // latitude out of range
            $"C,{Ethiopia},C,ሐ,CC,,ክልል,,true,37.5,12.5");     // swapped: valid range but outside Ethiopia → warning

        var result = await ImportCsv(csv, commit: false);

        Assert.Contains(result.Issues, x => x.Row == 2 && x.Severity == ImportIssueSeverity.Error);
        Assert.Contains(result.Issues, x => x.Row == 3 && x.Severity == ImportIssueSeverity.Error);
        Assert.Contains(result.Issues, x => x.Row == 4 && x.Severity == ImportIssueSeverity.Warning);
        Assert.DoesNotContain(result.Issues, x => x.Row == 4 && x.Severity == ImportIssueSeverity.Error);
    }

    [Fact]
    public async Task MissingRequiredColumns_AreReported()
    {
        var result = await ImportCsv("Id,ParentId,LocationName\nX,,Somewhere", commit: false);

        Assert.True(result.HasErrors);
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public async Task Excel_ImportsTheSameAsCsv_WithNumericCellsAndHeaderAliases()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Locations");
        // Header aliases (Code, Name, lat, lon) and numeric Excel cells.
        object[][] rows =
        [
            ["Id", "ParentId", "Name", "LocationAmharicName", "Code", "Level", "lat", "lon"],
            ["R-X", Ethiopia.ToString(), "Excel Region", "ኤክሴል ክልል", "XR1", "Region", 11.25, 37.75],
            ["Z-X", "R-X", "Excel Zone", "ኤክሴል ዞን", "XZ1", 2, 12.5, 37.5],
        ];
        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cell(r + 1, c + 1).Value = XLCellValue.FromObject(rows[r][c]);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var result = await _importer.ImportAsync(stream, "locations.xlsx", commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        var zone = await _db.Locations.SingleAsync(x => x.LocationCode == "XZ1");
        Assert.Equal(LocationLevel.ዞን, zone.Level);
        Assert.Equal(12.5m, zone.Latitude);
        Assert.Equal((await _db.Locations.SingleAsync(x => x.LocationCode == "XR1")).Id, zone.ParentId);
    }

    [Fact]
    public async Task Export_RoundTripsThroughImport_AsUpdatesOnly()
    {
        await ImportCsv(Hierarchy(), commit: true);
        var total = await _db.Locations.CountAsync();

        var exported = await _importer.ExportCsvAsync();
        var result = await _importer.ImportAsync(new MemoryStream(exported), "export.csv", commit: false);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        Assert.Equal(0, result.ToInsert);
        Assert.Equal(total, result.ToUpdate);
    }

    [Fact]
    public async Task ExcelTemplate_FilledLikeAUser_ImportsCorrectly()
    {
        await ImportCsv(Hierarchy(), commit: true);
        var woreda = await _db.Locations.SingleAsync(x => x.LocationCode == "TW1");

        using var workbook = new XLWorkbook(new MemoryStream(await _importer.BuildTemplateExcelAsync()));
        Assert.Equal("Locations", workbook.Worksheets.First().Name);             // the importer reads the first sheet
        var existing = workbook.Worksheet("ያሉ አድራሻዎች (Existing)");
        Assert.Equal(await _db.Locations.CountAsync(), existing.RangeUsed()!.RowCount() - 1);
        Assert.Contains(existing.Column(1).CellsUsed(), c => c.GetString() == woreda.Id.ToString());

        // Fill the sheet: one new kebele under the existing woreda (ParentId copied from the Existing sheet).
        var sheet = workbook.Worksheet("Locations");
        sheet.Cell("A2").Value = "K-NEW";
        sheet.Cell("B2").Value = woreda.Id.ToString();
        sheet.Cell("D2").Value = "አዲስ ቀበሌ";
        sheet.Cell("E2").Value = "009";                                         // stays text: leading zeros kept
        sheet.Cell("G2").Value = "ቀበሌ";
        sheet.Cell("J2").Value = 13.15;
        sheet.Cell("K2").Value = 37.91;
        using var filled = new MemoryStream();
        workbook.SaveAs(filled);
        filled.Position = 0;

        var result = await _importer.ImportAsync(filled, "template.xlsx", commit: true);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        var kebele = await _db.Locations.SingleAsync(x => x.LocationCode == "009");
        Assert.Equal(woreda.Id, kebele.ParentId);
        Assert.Equal(13.15m, kebele.Latitude);
    }

    [Fact]
    public async Task EmptyExcelTemplate_ReportsNoRows_AndCanOmitExistingSheet()
    {
        var bytes = await _importer.BuildTemplateExcelAsync(includeExistingLocations: false);
        using (var workbook = new XLWorkbook(new MemoryStream(bytes)))
            Assert.DoesNotContain(workbook.Worksheets, w => w.Name.StartsWith("ያሉ አድራሻዎች"));

        var result = await _importer.ImportAsync(new MemoryStream(bytes), "template.xlsx", commit: false);

        Assert.True(result.HasErrors);
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public async Task Template_IsValidAgainstSeededCountry()
    {
        var result = await _importer.ImportAsync(new MemoryStream(_importer.BuildTemplateCsv()), "template.csv", commit: false);

        Assert.False(result.HasErrors, string.Join(" | ", result.Issues.Select(x => x.Message)));
        Assert.Equal(4, result.TotalRows);
    }
}
