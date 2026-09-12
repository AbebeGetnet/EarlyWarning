using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using EarlyWarning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Controllers;

[Authorize(Roles = "Data Encoder,ZoneAdmin,RegionAdmin")]
public class WeeklyWorkflowController : Controller
{
    private readonly EarlyWarningDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IWeeklyAggregationService _aggregation;
    public WeeklyWorkflowController(EarlyWarningDbContext db, UserManager<ApplicationUser> users, IWeeklyAggregationService aggregation) { _db = db; _users = users; _aggregation = aggregation; }

    public async Task<IActionResult> Index(string? mode = null)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (mode == "approval" && User.IsInRole("RegionAdmin"))
            return RedirectToAction(nameof(RegionalQueue));
        var reports = await ScopedReports(user).Include(x => x.Woreda).ThenInclude(x => x.Parent).AsNoTracking()
            .OrderByDescending(x => x.EndDate).ThenBy(x => x.Woreda.LocationName).ToListAsync();
        if (mode == "approval")
        {
            reports = User.IsInRole("ZoneAdmin")
                ? reports.Where(x => x.Status is WeeklySubmissionStatus.SubmittedToZone or WeeklySubmissionStatus.RegionRejectedToZone).ToList()
                : User.IsInRole("RegionAdmin")
                    ? reports.Where(x => x.Status == WeeklySubmissionStatus.ZoneApproved).ToList()
                    : [];
        }
        ViewBag.WorkflowMode = mode;
        var aggregateReports = reports;
        if (mode == "approval" && User.IsInRole("ZoneAdmin"))
            aggregateReports = await ScopedReports(user).Include(x => x.Woreda).ThenInclude(x => x.Parent).AsNoTracking()
                .Where(x => x.Status == WeeklySubmissionStatus.ZoneApproved || x.Status == WeeklySubmissionStatus.RegionApproved || x.Status == WeeklySubmissionStatus.RegionRejectedToZone)
                .ToListAsync();
        var ids = aggregateReports.Select(x => x.WoredaId).Distinct().ToList();
        var keys = aggregateReports.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
        var aggregate = await BuildAggregate(ids, keys, aggregateReports);
        return View(new WeeklyWorkflowIndexViewModel
        {
            ScopeName = await _db.Locations.Where(x => x.Id == user.LocationId).Select(x => x.LocationAmharicName ?? x.LocationName ?? "").FirstAsync(),
            Reports = reports.Select(ToRow).ToList(), Aggregate = aggregate
        });
    }

    [Authorize(Roles = "Data Encoder,ZoneAdmin,RegionAdmin")]
    public async Task<IActionResult> ReportPageTwo()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var scoped = await ScopedReports(user)
            .Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .AsNoTracking()
            .ToListAsync();
        var latest = scoped.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate).FirstOrDefault();
        if (latest == null) return View(new WeeklyReportPageTwoViewModel());

        var reports = scoped.Where(x => x.StartDate.Date == latest.StartDate.Date && x.EndDate.Date == latest.EndDate.Date).ToList();
        var model = new WeeklyReportPageTwoViewModel { StartDate = latest.StartDate, EndDate = latest.EndDate };
        foreach (var group in reports.Where(x => x.Woreda.Parent != null).GroupBy(x => new
                 {
                     x.Woreda.ParentId,
                     Name = x.Woreda.Parent!.LocationAmharicName ?? x.Woreda.Parent.LocationName ?? ""
                 }).OrderBy(x => x.Key.Name))
        {
            model.Zones.Add(new WeeklyReportPageTwoZoneViewModel
            {
                Zone = group.Key.Name,
                Aggregate = await _aggregation.BuildAsync(group.ToList())
            });
        }
        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        var report = await ScopedReports(user).Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .Include(x => x.History).ThenInclude(x => x.User).ThenInclude(x => x.Location).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (report == null) return NotFound();
        var singleReports = new List<WeeklyReportSubmission> { report };
        return View(new WeeklyWorkflowDetailsViewModel
        {
            Report = ToRow(report),
            Aggregate = await BuildAggregate([report.WoredaId], [(report.WoredaId, report.StartDate.Date, report.EndDate.Date)], singleReports),
            History = report.History.OrderByDescending(x => x.PerformedAt).Select(x => new WeeklyWorkflowHistoryViewModel
            { User = $"{x.User.FirstName} {x.User.LastName}", Location = x.User.Location.LocationAmharicName ?? x.User.Location.LocationName ?? "", Action = x.Action,
              PreviousStatus = x.PreviousStatus, NewStatus = x.NewStatus, Comment = x.Comment, PerformedAt = x.PerformedAt }).ToList()
        });
    }

    [Authorize(Roles = "RegionAdmin")]
    public async Task<IActionResult> RegionalQueue()
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        var reports = await ScopedReports(user).Include(x => x.Woreda).ThenInclude(x => x.Parent).AsNoTracking()
            .Where(x => x.Status == WeeklySubmissionStatus.ZoneApproved)
            .OrderByDescending(x => x.StartDate).ToListAsync();
        var groups = reports.GroupBy(x => new { ZoneId = x.Woreda.ParentId!.Value, x.StartDate, x.EndDate }).ToList();
        var rows = new List<ZoneWeeklySubmissionViewModel>();
        foreach (var group in groups)
        {
            var items = group.ToList();
            var keys = items.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
            rows.Add(new ZoneWeeklySubmissionViewModel
            {
                ZoneId = group.Key.ZoneId,
                Zone = items[0].Woreda.Parent?.LocationAmharicName ?? items[0].Woreda.Parent?.LocationName ?? "",
                StartDate = group.Key.StartDate, EndDate = group.Key.EndDate, WoredaCount = items.Count,
                Woredas = items.Select(ToRow).ToList(),
                Aggregate = await BuildAggregate(items.Select(x => x.WoredaId).ToList(), keys, items)
            });
        }
        var region = await _db.Locations.Where(x => x.Id == user.LocationId).Select(x => x.LocationAmharicName ?? x.LocationName ?? "").FirstAsync();
        return View(new ZoneWeeklyQueueViewModel { Region = region, Submissions = rows });
    }

    [Authorize(Roles = "RegionAdmin")]
    public async Task<IActionResult> ZoneDetails(Guid zoneId, DateTime startDate, DateTime endDate)
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        var reports = await ScopedReports(user).Include(x => x.Woreda).ThenInclude(x => x.Parent).AsNoTracking()
            .Where(x => x.Woreda.ParentId == zoneId && x.StartDate == startDate && x.EndDate == endDate && x.Status == WeeklySubmissionStatus.ZoneApproved)
            .ToListAsync();
        if (reports.Count == 0) return NotFound();
        var keys = reports.Select(x => (x.WoredaId, x.StartDate.Date, x.EndDate.Date)).ToHashSet();
        return View(new ZoneWeeklySubmissionViewModel
        {
            ZoneId = zoneId, Zone = reports[0].Woreda.Parent?.LocationAmharicName ?? reports[0].Woreda.Parent?.LocationName ?? "",
            StartDate = startDate, EndDate = endDate, WoredaCount = reports.Count, Woredas = reports.Select(ToRow).ToList(),
            Aggregate = await BuildAggregate(reports.Select(x => x.WoredaId).ToList(), keys, reports)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "RegionAdmin")]
    public async Task<IActionResult> ActZone(Guid zoneId, DateTime startDate, DateTime endDate, string decision, string? comment)
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        if (decision == "Reject" && string.IsNullOrWhiteSpace(comment))
        { TempData["Error"] = "የዞን ሪፖርት ውድቅ ሲደረግ አስተያየት ግዴታ ነው።"; return RedirectToAction(nameof(ZoneDetails), new { zoneId, startDate, endDate }); }
        var reports = await ScopedReports(user).Where(x => x.Woreda.ParentId == zoneId && x.StartDate == startDate && x.EndDate == endDate && x.Status == WeeklySubmissionStatus.ZoneApproved).ToListAsync();
        if (reports.Count == 0) return NotFound();
        var next = decision == "Approve" ? WeeklySubmissionStatus.RegionApproved : WeeklySubmissionStatus.RegionRejectedToZone;
        var action = decision == "Approve" ? WeeklyWorkflowAction.RegionApprove : WeeklyWorkflowAction.RegionReject;
        if (decision is not ("Approve" or "Reject")) return BadRequest();
        var now = DateTime.UtcNow;
        foreach (var report in reports)
        {
            report.Status = next; report.LastActionAt = now;
            _db.WeeklyReportWorkflowHistories.Add(new WeeklyReportWorkflowHistory
            {
                WeeklyReportSubmissionId = report.Id, UserId = user.Id, ActorLocationId = user.LocationId,
                ActorLocationLevel = user.UserLocationLevl, Action = action, PreviousStatus = WeeklySubmissionStatus.ZoneApproved,
                NewStatus = next, Comment = comment?.Trim(), PerformedAt = now
            });
        }
        await _db.SaveChangesAsync();
        if (next == WeeklySubmissionStatus.RegionApproved)
        {
            var regionalReports = await ScopedReports(user).AsNoTracking()
                .Where(x => x.StartDate == startDate && x.EndDate == endDate && x.Status == WeeklySubmissionStatus.RegionApproved)
                .ToListAsync();
            await _aggregation.SaveSnapshotAsync(user.LocationId, user.UserLocationLevl, startDate, endDate, next, user.Id, regionalReports);
        }
        TempData["Success"] = decision == "Approve" ? "የዞኑ የሳምንት ድምር ጸድቋል።" : "የዞኑ ሪፖርት ለዞን ዳግም ግምገማ ተመልሷል።";
        return RedirectToAction(nameof(RegionalQueue));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Act(Guid id, string decision, string? comment, byte[] rowVersion)
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        var report = await ScopedReports(user).FirstOrDefaultAsync(x => x.Id == id); if (report == null) return NotFound();
        if (User.IsInRole("ZoneAdmin") && DateTime.Today.Date > report.ReportingWeekStart.Date.AddDays(3))
        {
            TempData["Error"] = "የዞን ፍቃድ/ውድቅ ጊዜው ሐሙስ ማብቂያ ላይ አልፏል። ሪፖርቱን ማየት ይቻላል፣ ውሳኔ መስጠት ግን አይቻልም።";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (decision.Contains("Reject", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(comment))
        { TempData["Error"] = "ውድቅ ሲደረግ ምክንያት/አስተያየት ግዴታ ነው።"; return RedirectToAction(nameof(Details), new { id }); }

        var previous = report.Status;
        WeeklySubmissionStatus next; WeeklyWorkflowAction action;
        if (User.IsInRole("ZoneAdmin") && decision == "Approve" && previous is WeeklySubmissionStatus.SubmittedToZone or WeeklySubmissionStatus.RegionRejectedToZone)
        { next = WeeklySubmissionStatus.ZoneApproved; action = previous == WeeklySubmissionStatus.RegionRejectedToZone ? WeeklyWorkflowAction.ZoneResubmit : WeeklyWorkflowAction.ZoneApprove; }
        else if (User.IsInRole("ZoneAdmin") && decision == "Reject" && previous is WeeklySubmissionStatus.SubmittedToZone or WeeklySubmissionStatus.RegionRejectedToZone)
        { next = WeeklySubmissionStatus.ZoneRejectedToWoreda; action = WeeklyWorkflowAction.ZoneReject; }
        else if (User.IsInRole("RegionAdmin") && decision == "Approve" && previous == WeeklySubmissionStatus.ZoneApproved)
        { next = WeeklySubmissionStatus.RegionApproved; action = WeeklyWorkflowAction.RegionApprove; }
        else if (User.IsInRole("RegionAdmin") && decision == "Reject" && previous == WeeklySubmissionStatus.ZoneApproved)
        { next = WeeklySubmissionStatus.RegionRejectedToZone; action = WeeklyWorkflowAction.RegionReject; }
        else { TempData["Error"] = "ይህ እርምጃ ለሪፖርቱ አሁን ሁኔታ አይፈቀድም።"; return RedirectToAction(nameof(Details), new { id }); }

        _db.Entry(report).Property(x => x.RowVersion).OriginalValue = rowVersion;
        report.Status = next; report.LastActionAt = DateTime.UtcNow;
        _db.WeeklyReportWorkflowHistories.Add(new WeeklyReportWorkflowHistory { WeeklyReportSubmissionId = id, UserId = user.Id,
            ActorLocationId = user.LocationId, ActorLocationLevel = user.UserLocationLevl, Action = action, PreviousStatus = previous,
            NewStatus = next, Comment = comment?.Trim(), PerformedAt = report.LastActionAt });
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { TempData["Error"] = "ሌላ ተጠቃሚ ሪፖርቱን ቀድሞ ቀይሮታል፤ እንደገና ይክፈቱ።"; return RedirectToAction(nameof(Details), new { id }); }
        if (next == WeeklySubmissionStatus.ZoneApproved)
        {
            var zoneReports = await ScopedReports(user).AsNoTracking().Where(x => x.StartDate == report.StartDate && x.EndDate == report.EndDate &&
                (x.Status == WeeklySubmissionStatus.ZoneApproved || x.Status == WeeklySubmissionStatus.RegionApproved || x.Status == WeeklySubmissionStatus.RegionRejectedToZone)).ToListAsync();
            await _aggregation.SaveSnapshotAsync(user.LocationId, user.UserLocationLevl, report.StartDate, report.EndDate, next, user.Id, zoneReports);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private IQueryable<WeeklyReportSubmission> ScopedReports(ApplicationUser user)
    {
        var query = _db.WeeklyReportSubmissions.AsQueryable();
        if (User.IsInRole("Data Encoder")) return query.Where(x => x.WoredaId == user.LocationId && x.CreatedById == user.Id);
        if (User.IsInRole("ZoneAdmin")) return query.Where(x => x.Status != WeeklySubmissionStatus.Draft && x.Woreda.ParentId == user.LocationId);
        return query.Where(x => x.Status != WeeklySubmissionStatus.Draft && x.Woreda.Parent != null && x.Woreda.Parent.ParentId == user.LocationId);
    }

    private static WeeklyWorkflowRowViewModel ToRow(WeeklyReportSubmission x) => new()
    { Id = x.Id, WoredaId = x.WoredaId, Woreda = x.Woreda.LocationAmharicName ?? x.Woreda.LocationName ?? "", Zone = x.Woreda.Parent?.LocationAmharicName ?? x.Woreda.Parent?.LocationName ?? "", StartDate = x.StartDate, EndDate = x.EndDate, Status = x.Status, LastActionAt = x.LastActionAt, RowVersion = x.RowVersion };

    private async Task<WeeklyAggregateViewModel> BuildAggregate(List<Guid> woredaIds, HashSet<(Guid, DateTime, DateTime)> keys, List<WeeklyReportSubmission> reports)
    {
        return await _aggregation.BuildAsync(reports);
    }

}
