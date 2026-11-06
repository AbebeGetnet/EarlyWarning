using EarlyWarning.Data;
using EarlyWarning.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Services;

/// <summary>Where a woreda sits in the hierarchy (Woreda → Zone), used to count affected locations per level.</summary>
public sealed record WoredaPlace(Guid WoredaId, string Woreda, Guid? ZoneId, string Zone);

public static class AffectedLocations
{
    public static async Task<Dictionary<Guid, WoredaPlace>> LoadPlacesAsync(EarlyWarningDbContext db, IEnumerable<Guid> woredaIds)
    {
        var ids = woredaIds.Distinct().ToList();
        var rows = await db.Locations.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new
            {
                x.Id, x.LocationAmharicName, x.LocationName, x.ParentId,
                ZoneAmharic = x.Parent != null ? x.Parent.LocationAmharicName : null,
                ZoneName = x.Parent != null ? x.Parent.LocationName : null
            })
            .ToListAsync();
        return rows.ToDictionary(x => x.Id, x => new WoredaPlace(x.Id, x.LocationAmharicName ?? x.LocationName ?? "-",
            x.ParentId, x.ParentId == null ? "-" : x.ZoneAmharic ?? x.ZoneName ?? "-"));
    }

    /// <summary>
    /// Kebeles = sum of the affected kebeles reported by the woredas. A woreda is affected when its kebeles are
    /// greater than zero and is counted once; a zone is affected when it holds at least one affected woreda and is
    /// counted once. Rows of the same woreda are added together before deciding.
    /// </summary>
    public static AffectedLocationSummaryViewModel Summarize(string eventName, IEnumerable<(Guid WoredaId, int Kebeles)> rows,
        IReadOnlyDictionary<Guid, WoredaPlace> places)
    {
        var affected = rows
            .GroupBy(x => x.WoredaId)
            .Select(g => (WoredaId: g.Key, Kebeles: g.Sum(x => Math.Max(0, x.Kebeles))))
            .Where(x => x.Kebeles > 0)
            .Select(x =>
            {
                var place = places.GetValueOrDefault(x.WoredaId) ?? new WoredaPlace(x.WoredaId, "-", null, "-");
                return new AffectedWoredaViewModel { ZoneId = place.ZoneId, Zone = place.Zone, WoredaId = x.WoredaId, Woreda = place.Woreda, Kebeles = x.Kebeles };
            })
            .OrderBy(x => x.Zone).ThenBy(x => x.Woreda)
            .ToList();

        return new AffectedLocationSummaryViewModel
        {
            Event = eventName,
            Kebeles = affected.Sum(x => x.Kebeles),
            Woredas = affected.Count,
            Zones = affected.Where(x => x.ZoneId.HasValue).Select(x => x.ZoneId).Distinct().Count(),
            Locations = affected
        };
    }

    /// <summary>
    /// Combines per-zone summaries of the same event into one higher-level summary. Each zone summary covers
    /// different woredas, so kebeles and woredas add up; zones are recounted from the affected woredas.
    /// </summary>
    public static AffectedLocationSummaryViewModel Combine(string eventName, IEnumerable<AffectedLocationSummaryViewModel> parts)
    {
        var locations = parts.SelectMany(x => x.Locations)
            .GroupBy(x => x.WoredaId)
            .Select(g => new AffectedWoredaViewModel { ZoneId = g.First().ZoneId, Zone = g.First().Zone, WoredaId = g.Key, Woreda = g.First().Woreda, Kebeles = g.Sum(x => x.Kebeles) })
            .OrderBy(x => x.Zone).ThenBy(x => x.Woreda)
            .ToList();
        return new AffectedLocationSummaryViewModel
        {
            Event = eventName,
            Kebeles = locations.Sum(x => x.Kebeles),
            Woredas = locations.Count,
            Zones = locations.Where(x => x.ZoneId.HasValue).Select(x => x.ZoneId).Distinct().Count(),
            Locations = locations
        };
    }

    /// <summary>The events that track affected kebeles, in report order, with each woreda row's kebele count.</summary>
    public static class Events
    {
        public const string Flood = "በከፍተኛ ዝናብ/ጎርፍ የተጎዱ";
        public const string Drought = "በድርቅ የተጎዱ";
        public const string NoRain = "ዝናብ ያላገኙ";
        public const string PastureShortage = "የእንስሳት የግጦሽ ችግር";
        public const string AnimalWaterShortage = "የእንስሳት የውሃ ችግር";
        public const string HumanWaterShortage = "የሰው መጠጥ ውሃ ችግር";
        public const string Epidemic = "የሰው ወረርሽኝ በሽታ";
    }

    /// <summary>Number of distinct kebeles stored as a JSON id list (flood / drought selections).</summary>
    public static int KebeleListCount(List<Guid> ids) => ids.Distinct().Count();

    // ---- Display: each level shows the count of the level below it in brackets.

    /// <summary>Affected woredas grouped under their zone, in report order. Each woreda appears once.</summary>
    public static List<IGrouping<string, AffectedWoredaViewModel>> ByZone(AffectedLocationSummaryViewModel summary) =>
        summary.Locations.GroupBy(x => x.Zone).OrderBy(g => g.Key).ToList();

    /// <summary>"Woreda A (5 ቀበሌ)"</summary>
    public static string WoredaWithKebeles(AffectedWoredaViewModel woreda) => $"{woreda.Woreda} ({Count(woreda.Kebeles)} ቀበሌ)";

    /// <summary>"Woreda A (5 ቀበሌ)፣ Woreda B (3 ቀበሌ)"</summary>
    public static string WoredaList(IEnumerable<AffectedWoredaViewModel> woredas) => string.Join("፣ ", woredas.Select(WoredaWithKebeles));

    /// <summary>"Zone A ዞን (3 ወረዳ)"</summary>
    public static string ZoneWithWoredas(string zone, int woredas) => $"{zone} ዞን ({Count(woredas)} ወረዳ)";

    /// <summary>"Zone A ዞን (3 ወረዳ)፣ Zone C ዞን (2 ወረዳ)"</summary>
    public static string ZoneList(AffectedLocationSummaryViewModel summary) =>
        string.Join("፣ ", ByZone(summary).Select(g => ZoneWithWoredas(g.Key, g.Count())));

    /// <summary>"ድምር፡ 3 የተጎዱ ወረዳዎች እና 10 የተጎዱ ቀበሌዎች"</summary>
    public static string Totals(IEnumerable<AffectedWoredaViewModel> woredas)
    {
        var list = woredas.ToList();
        return $"ድምር፡ {Count(list.Count)} የተጎዱ ወረዳዎች እና {Count(list.Sum(x => x.Kebeles))} የተጎዱ ቀበሌዎች";
    }

    private static string Count(int value) => value.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
}
