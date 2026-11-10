using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using EarlyWarning.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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
    private readonly IWeeklySubmissionFormLoader _formLoader;
    private readonly ITimeLimitedDataProtector _approvalTokens;
    public WeeklyWorkflowController(EarlyWarningDbContext db, UserManager<ApplicationUser> users, IWeeklyAggregationService aggregation,
        IWeeklySubmissionFormLoader formLoader, IDataProtectionProvider dataProtection)
    {
        _db = db; _users = users; _aggregation = aggregation; _formLoader = formLoader;
        _approvalTokens = dataProtection.CreateProtector("WeeklyWorkflow.DetailsApproval").ToTimeLimitedDataProtector();
    }

    public async Task<IActionResult> Index([FromServices] IReportScopeService scopes, [FromServices] IWeeklyMatrixReportService matrixReport,
        string? mode = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (mode == "approval" && User.IsInRole("RegionAdmin"))
            return RedirectToAction(nameof(RegionalQueue));

        // የተጠቃለለ ሪፖርት: the selected week as the standard narrative report.
        if (mode == null)
        {
            var (scope, error, weeks, week, weekReports) = await ResolveWeekAsync(scopes, user, startDate, endDate);
            var summary = new WeeklyWorkflowIndexViewModel { ScopeName = scope?.LocationName ?? "", ErrorMessage = error, AvailableWeeks = weeks };
            if (scope != null && week != null && weekReports.Count > 0)
            {
                summary.StartDate = week.StartDate;
                summary.EndDate = week.EndDate;
                var grid = await matrixReport.BuildAsync(scope, weekReports, week.StartDate, week.EndDate);
                summary.Narrative = WeeklyNarrativeReportBuilder.Build(grid, scope.LocationName, scope.Level);
            }
            return View(summary);
        }
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
        // Review / approval lists: the woreda reports only (the old aggregate shown here is replaced by the narrative report).
        ViewBag.WorkflowMode = mode;
        return View(new WeeklyWorkflowIndexViewModel
        {
            ScopeName = await _db.Locations.Where(x => x.Id == user.LocationId).Select(x => x.LocationAmharicName ?? x.LocationName ?? "").FirstAsync(),
            Reports = reports.Select(ToRow).ToList()
        });
    }

    // Page 2: the same standard report broken down — one per zone for region users, one per woreda for zone users.
    [Authorize(Roles = "Data Encoder,ZoneAdmin,RegionAdmin")]
    public async Task<IActionResult> ReportPageTwo([FromServices] IReportScopeService scopes, [FromServices] IWeeklyMatrixReportService matrixReport,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var (scope, error, weeks, week, reports) = await ResolveWeekAsync(scopes, user, startDate, endDate);
        var model = new WeeklyReportPageTwoViewModel { ErrorMessage = error, AvailableWeeks = weeks, ScopeName = scope?.LocationName ?? "" };
        if (scope == null || week == null || reports.Count == 0) return View(model);

        model.StartDate = week.StartDate;
        model.EndDate = week.EndDate;
        // One query pass with a column per woreda, then split: per zone (region) or per woreda (zone/woreda).
        var grid = await matrixReport.BuildAsync(scope, reports, week.StartDate, week.EndDate, woredaColumns: true);
        model.BreakdownWord = scope.Level == ReportScopeLevel.Region ? "ዞን" : "ወረዳ";
        model.Reports = scope.Level == ReportScopeLevel.Region
            ? WeeklyNarrativeReportBuilder.BuildPerZone(grid)
            : grid.Units.Select((unit, index) => (unit, index)).Where(x => !x.unit.IsTotal)
                .Select(x => WeeklyNarrativeReportBuilder.Build(grid, x.unit.Name, ReportScopeLevel.Woreda, [x.index]))
                .ToList();
        return View(model);
    }

    /// <summary>
    /// The user's report boundary, the weeks inside it (newest first) and the submissions of the selected week
    /// (the requested one if it is inside the boundary, otherwise the newest). Same rules as the form report.
    /// </summary>
    private async Task<(ReportScope? Scope, string? Error, List<WeeklyFormReportWeek> Weeks, WeeklyFormReportWeek? Week, List<WeeklyReportSubmission> Reports)>
        ResolveWeekAsync(IReportScopeService scopes, ApplicationUser user, DateTime? startDate, DateTime? endDate)
    {
        var (scope, error) = await scopes.ResolveAsync(user);
        if (scope == null) return (null, error, [], null, []);
        var scoped = await scopes.GetSubmissionsAsync(scope);
        var weeks = scoped.Select(x => new WeeklyFormReportWeek(x.StartDate.Date, x.EndDate.Date)).Distinct()
            .OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate).ToList();
        var week = startDate.HasValue && endDate.HasValue
            ? weeks.FirstOrDefault(x => x.StartDate == startDate.Value.Date && x.EndDate == endDate.Value.Date) ?? weeks.FirstOrDefault()
            : weeks.FirstOrDefault();
        var reports = week == null ? [] : scoped.Where(x => x.StartDate.Date == week.StartDate && x.EndDate.Date == week.EndDate).ToList();
        return (scope, null, weeks, week, reports);
    }

    // Weekly report in the layout of the paper form. The boundary (woreda / zone / region) comes only from the
    // signed-in user's assigned location; the only request input is the week, which must be one inside that boundary.
    public async Task<IActionResult> FormReport([FromServices] IWeeklyFormReportService formReport, [FromServices] IReportScopeService scopes,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var model = new WeeklyFormReportViewModel { PreparedBy = $"{user.FirstName} {user.LastName}".Trim() };
        var (scope, error) = await scopes.ResolveAsync(user);
        if (scope == null)
        {
            model.ErrorMessage = error;
            return View(model);
        }

        model.ScopeLevel = scope.Level;
        model.ScopeName = scope.LocationName;
        model.ZoneName = scope.ZoneName;
        model.RegionName = scope.RegionName;
        model.WoredasInScope = scope.WoredaIds.Count;

        var scoped = await scopes.GetSubmissionsAsync(scope);
        model.AvailableWeeks = scoped.Select(x => new WeeklyFormReportWeek(x.StartDate.Date, x.EndDate.Date)).Distinct()
            .OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate).ToList();

        var week = startDate.HasValue && endDate.HasValue
            ? model.AvailableWeeks.FirstOrDefault(x => x.StartDate == startDate.Value.Date && x.EndDate == endDate.Value.Date)
            : model.AvailableWeeks.FirstOrDefault();
        if (week == null) return View(model);

        var reports = scoped.Where(x => x.StartDate.Date == week.StartDate && x.EndDate.Date == week.EndDate).ToList();
        model.StartDate = week.StartDate;
        model.EndDate = week.EndDate;
        model.WoredaCount = reports.Select(x => x.WoredaId).Distinct().Count();
        model.Zones = reports.Select(x => x.Woreda.Parent?.LocationAmharicName ?? x.Woreda.Parent?.LocationName ?? "-").Distinct().OrderBy(x => x).ToList();
        model.Sections = await formReport.BuildAsync(reports, scope.Level);
        return View(model);
    }

    // Report menu → Excel export: the weeks inside the user's boundary, each with a download button.
    public async Task<IActionResult> ExcelExport([FromServices] IReportScopeService scopes)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var (scope, error) = await scopes.ResolveAsync(user);
        if (scope == null) return View(new WeeklyExcelExportViewModel { ErrorMessage = error });

        var weeks = (await scopes.GetSubmissionsAsync(scope))
            .GroupBy(x => (Start: x.StartDate.Date, End: x.EndDate.Date))
            .Select(g => new WeeklyExcelExportWeek(g.Key.Start, g.Key.End, g.Select(x => x.WoredaId).Distinct().Count()))
            .OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate)
            .ToList();
        return View(new WeeklyExcelExportViewModel { ScopeName = scope.LocationName, WoredasInScope = scope.WoredaIds.Count, Weeks = weeks });
    }

    // The same week as FormReport, as an Excel workbook in the layout of the regional weekly summary
    // (topics × zones for region users, topics × woredas for zone and woreda users).
    public async Task<IActionResult> FormReportExcel([FromServices] IWeeklyMatrixReportService matrixReport, [FromServices] IReportScopeService scopes,
        DateTime startDate, DateTime endDate)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var (scope, _) = await scopes.ResolveAsync(user);
        if (scope == null) return RedirectToAction(nameof(FormReport));

        // Only a week inside the user's boundary (and visible at their level) can be exported.
        var reports = (await scopes.GetSubmissionsAsync(scope))
            .Where(x => x.StartDate.Date == startDate.Date && x.EndDate.Date == endDate.Date).ToList();
        if (reports.Count == 0) return RedirectToAction(nameof(FormReport));

        var report = await matrixReport.BuildAsync(scope, reports, startDate.Date, endDate.Date);
        var start = Extensions.EthiopianDate.FromGregorian(startDate);
        var end = Extensions.EthiopianDate.FromGregorian(endDate);
        var fileName = $"ሳምንታዊ ሪፖርት {scope.LocationName} {start.Day:00}-{start.Month:00}-{start.Year} እስከ {end.Day:00}-{end.Month:00}-{end.Year}.xlsx";
        return File(WeeklyMatrixExcelWriter.Write(report), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        var report = await ScopedReports(user).Include(x => x.Woreda).ThenInclude(x => x.Parent)
            .Include(x => x.History).ThenInclude(x => x.User).ThenInclude(x => x.Location).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (report == null) return NotFound();
        var singleReports = new List<WeeklyReportSubmission> { report };
        var zoneWindowClosed = ZoneDecisionWindowClosed(report);
        var canZone = User.IsInRole("ZoneAdmin") && !zoneWindowClosed &&
            report.Status is WeeklySubmissionStatus.SubmittedToZone or WeeklySubmissionStatus.RegionRejectedToZone;
        var canRegion = User.IsInRole("RegionAdmin") && report.Status == WeeklySubmissionStatus.ZoneApproved;
        return View(new WeeklyWorkflowDetailsViewModel
        {
            Report = ToRow(report),
            Form = await _formLoader.LoadAsync(report),
            CanZoneApprove = canZone,
            CanRegionApprove = canRegion,
            ZoneDecisionWindowClosed = User.IsInRole("ZoneAdmin") && zoneWindowClosed,
            ApprovalToken = canZone || canRegion ? IssueApprovalToken(report.Id, user.Id) : null,
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

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Act(Guid id, string decision, string? comment, byte[] rowVersion, string? approvalToken)
    {
        var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
        var report = await ScopedReports(user).FirstOrDefaultAsync(x => x.Id == id); if (report == null) return NotFound();
        // Decisions are only accepted from the record's details page, which issues the token.
        if (!IsValidApprovalToken(approvalToken, report.Id, user.Id))
        {
            TempData["Error"] = "ውሳኔ መስጠት የሚቻለው የሪፖርቱን ሙሉ ዝርዝር ገጽ ከፍተው ከገመገሙ በኋላ ብቻ ነው።";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (User.IsInRole("ZoneAdmin") && ZoneDecisionWindowClosed(report))
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
        else if (next == WeeklySubmissionStatus.RegionApproved)
        {
            var regionalReports = await ScopedReports(user).AsNoTracking()
                .Where(x => x.StartDate == report.StartDate && x.EndDate == report.EndDate && x.Status == WeeklySubmissionStatus.RegionApproved)
                .ToListAsync();
            await _aggregation.SaveSnapshotAsync(user.LocationId, user.UserLocationLevl, report.StartDate, report.EndDate, next, user.Id, regionalReports);
        }
        TempData["Success"] = next switch
        {
            WeeklySubmissionStatus.ZoneApproved => "ሪፖርቱ በዞን ጸድቆ ለክልል ተልኳል።",
            WeeklySubmissionStatus.RegionApproved => "ሪፖርቱ በክልል ጸድቋል።",
            _ => "ሪፖርቱ ውድቅ ተደርጎ ተመልሷል።"
        };
        return RedirectToAction(nameof(Details), new { id });
    }

    // Zone decisions close at the end of Thursday of the reporting week.
    private static bool ZoneDecisionWindowClosed(WeeklyReportSubmission report) =>
        DateTime.Today.Date > report.ReportingWeekStart.Date.AddDays(3);

    private string IssueApprovalToken(Guid reportId, string userId) =>
        _approvalTokens.Protect($"{reportId:N}|{userId}", TimeSpan.FromHours(4));

    private bool IsValidApprovalToken(string? token, Guid reportId, string userId)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        try { return _approvalTokens.Unprotect(token) == $"{reportId:N}|{userId}"; }
        catch (CryptographicException) { return false; }
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
