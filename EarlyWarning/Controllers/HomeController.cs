using EarlyWarning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using System.Diagnostics;

namespace EarlyWarning.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EarlyWarningDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        private readonly IWeeklyAggregationService _aggregation;

        public HomeController(ILogger<HomeController> logger, EarlyWarningDbContext db, UserManager<ApplicationUser> users, IWeeklyAggregationService aggregation)
        {
            _logger = logger;
            _db = db; _users = users; _aggregation = aggregation;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User); if (user == null) return Challenge();
            var query = _db.WeeklyReportSubmissions.Include(x => x.Woreda).ThenInclude(x => x.Parent).AsNoTracking();
            if (User.IsInRole("Data Encoder")) query = query.Where(x => x.WoredaId == user.LocationId && x.Status != WeeklySubmissionStatus.Draft);
            else if (User.IsInRole("ZoneAdmin")) query = query.Where(x => x.Woreda.ParentId == user.LocationId && (x.Status == WeeklySubmissionStatus.ZoneApproved || x.Status == WeeklySubmissionStatus.RegionApproved || x.Status == WeeklySubmissionStatus.RegionRejectedToZone));
            else if (User.IsInRole("RegionAdmin")) query = query.Where(x => x.Woreda.Parent != null && x.Woreda.Parent.ParentId == user.LocationId && x.Status == WeeklySubmissionStatus.RegionApproved);
            else query = query.Where(x => false);
            var latest = await query.MaxAsync(x => (DateTime?)x.ReportingWeekStart);
            var reports = latest == null ? [] : await query.Where(x => x.ReportingWeekStart == latest.Value).ToListAsync();
            WeeklyAggregateViewModel aggregate;
            if (reports.Count == 0) aggregate = new WeeklyAggregateViewModel();
            else if (User.IsInRole("RegionAdmin"))
                aggregate = await _aggregation.GetSnapshotAsync(user.LocationId, user.UserLocationLevl, reports[0].StartDate, reports[0].EndDate) ?? await _aggregation.BuildAsync(reports);
            else aggregate = await _aggregation.BuildAsync(reports);
            var scopeName = await _db.Locations.Where(x => x.Id == user.LocationId).Select(x => x.LocationAmharicName ?? x.LocationName ?? "").FirstOrDefaultAsync() ?? "";
            return View(new WeeklyWorkflowIndexViewModel { ScopeName = scopeName, Reports = reports.Select(x => new WeeklyWorkflowRowViewModel { Id=x.Id,WoredaId=x.WoredaId,Woreda=x.Woreda.LocationAmharicName??x.Woreda.LocationName??"",Zone=x.Woreda.Parent?.LocationAmharicName??x.Woreda.Parent?.LocationName??"",StartDate=x.StartDate,EndDate=x.EndDate,Status=x.Status,LastActionAt=x.LastActionAt,RowVersion=x.RowVersion }).ToList(), Aggregate=aggregate });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
