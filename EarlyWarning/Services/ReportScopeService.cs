using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Services;

public enum ReportScopeLevel { Woreda, Zone, Region }

/// <summary>The administrative boundary a user may report on, derived from their assigned location.</summary>
public sealed class ReportScope
{
    public ReportScopeLevel Level { get; init; }
    public Guid LocationId { get; init; }
    public string LocationName { get; init; } = "";
    public string? ZoneName { get; init; }
    public string? RegionName { get; init; }
    /// <summary>Every woreda inside the boundary (whether or not it has reported).</summary>
    public IReadOnlyList<Guid> WoredaIds { get; init; } = [];
    /// <summary>Woreda users see their own drafts; zone and region users only see submitted reports.</summary>
    public bool IncludeDrafts => Level == ReportScopeLevel.Woreda;
}

public sealed record ReportScopeResult(ReportScope? Scope, string? Error);

public interface IReportScopeService
{
    /// <summary>Resolves the user's report boundary from their assigned location (never from request input).</summary>
    Task<ReportScopeResult> ResolveAsync(ApplicationUser user);

    /// <summary>
    /// Weekly submissions inside the scope, with Woreda and Woreda.Parent loaded.
    /// Returns at most one submission per woreda and week (the most recently actioned).
    /// </summary>
    Task<List<WeeklyReportSubmission>> GetSubmissionsAsync(ReportScope scope);
}

public sealed class ReportScopeService(EarlyWarningDbContext db) : IReportScopeService
{
    public async Task<ReportScopeResult> ResolveAsync(ApplicationUser user)
    {
        if (user.LocationId == Guid.Empty)
            return new(null, "ለተጠቃሚው የተመደበ አድራሻ (ወረዳ/ዞን/ክልል) የለም። እባክዎ የሥርዓቱን አስተዳዳሪ ያነጋግሩ።");

        var location = await db.Locations.AsNoTracking()
            .Include(x => x.Parent).ThenInclude(x => x!.Parent)
            .FirstOrDefaultAsync(x => x.Id == user.LocationId);
        if (location == null)
            return new(null, "ለተጠቃሚው የተመደበው አድራሻ በመረጃ ቋቱ ውስጥ አልተገኘም። እባክዎ የሥርዓቱን አስተዳዳሪ ያነጋግሩ።");

        switch (location.Level)
        {
            case LocationLevel.ወረዳ:
                return new(new ReportScope
                {
                    Level = ReportScopeLevel.Woreda,
                    LocationId = location.Id,
                    LocationName = Name(location),
                    ZoneName = location.Parent is { } zone ? Name(zone) : null,
                    RegionName = location.Parent?.Parent is { } woredaRegion ? Name(woredaRegion) : null,
                    WoredaIds = [location.Id]
                }, null);

            case LocationLevel.ዞን:
                return new(new ReportScope
                {
                    Level = ReportScopeLevel.Zone,
                    LocationId = location.Id,
                    LocationName = Name(location),
                    ZoneName = Name(location),
                    RegionName = location.Parent is { } zoneRegion ? Name(zoneRegion) : null,
                    WoredaIds = await db.Locations.AsNoTracking()
                        .Where(x => x.Level == LocationLevel.ወረዳ && x.ParentId == location.Id)
                        .Select(x => x.Id).ToListAsync()
                }, null);

            case LocationLevel.ክልል:
                return new(new ReportScope
                {
                    Level = ReportScopeLevel.Region,
                    LocationId = location.Id,
                    LocationName = Name(location),
                    RegionName = Name(location),
                    WoredaIds = await db.Locations.AsNoTracking()
                        .Where(x => x.Level == LocationLevel.ወረዳ && x.Parent != null && x.Parent.ParentId == location.Id)
                        .Select(x => x.Id).ToListAsync()
                }, null);

            default:
                return new(null, "ይህ ሪፖርት ለወረዳ፣ ለዞን ወይም ለክልል ደረጃ ተጠቃሚዎች ብቻ ነው። የእርስዎ የተመደበ አድራሻ ደረጃ ለዚህ ሪፖርት አይፈቀድም።");
        }
    }

    public async Task<List<WeeklyReportSubmission>> GetSubmissionsAsync(ReportScope scope)
    {
        if (scope.WoredaIds.Count == 0) return [];
        var woredaIds = scope.WoredaIds.ToList();
        var query = db.WeeklyReportSubmissions.AsNoTracking()
            .Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .Where(x => woredaIds.Contains(x.WoredaId));
        if (!scope.IncludeDrafts)
            query = query.Where(x => x.Status != WeeklySubmissionStatus.Draft);

        return (await query.ToListAsync())
            .GroupBy(x => (x.WoredaId, Start: x.StartDate.Date, End: x.EndDate.Date))
            .Select(g => g.OrderByDescending(x => x.LastActionAt).ThenByDescending(x => x.CreatedAt).First())
            .ToList();
    }

    private static string Name(Locations location) => location.LocationAmharicName ?? location.LocationName ?? "";
}
