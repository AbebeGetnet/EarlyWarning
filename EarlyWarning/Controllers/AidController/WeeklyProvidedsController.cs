using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.Aid;
using EarlyWarning.Enums;
using EarlyWarning.Models.Migration;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;

namespace EarlyWarning.Controllers.AidController
{
    public class WeeklyProvidedsController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;



        public WeeklyProvidedsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: WeeklyProvideds
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.WeeklyProvideds.Include(w => w.Woreda).Include(w => w.WeeklyProvidedDetail)
                .ThenInclude(d => d.SupplyType);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: WeeklyProvideds/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weeklyProvided = await _context.WeeklyProvideds
                .Include(w => w.Woreda)
                .FirstOrDefaultAsync(m => m.WeeklyProvidedId == id);
            if (weeklyProvided == null)
            {
                return NotFound();
            }

            return View(weeklyProvided);
        }
        // Helper method to get Monday of current week
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: WeeklyProvided/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var weeklyProvided = new WeeklyProvided();

            // Pre-populate from query string parameters (coming from Migration redirect)
            if (startDate.HasValue)
                weeklyProvided.StartDate = startDate.Value;
            else
                weeklyProvided.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                weeklyProvided.EndDate = endDate.Value;
            else
                weeklyProvided.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                weeklyProvided.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                weeklyProvided.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                weeklyProvided.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                weeklyProvided.ReportedBy = currentUser?.Id;
            }

            weeklyProvided.ReportDate = DateTime.Now;
            weeklyProvided.ReportStatus = ApprovalStatus.በሂደት_ላይ;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                weeklyProvided.WoredaId
            );

            // Supply Types for multiple selection
            ViewBag.SupplyTypes = await _context.SupplyTypes
                .Where(s => s.IsActive)
                .OrderBy(s => s.SupplyTypeName)
                .ToListAsync();

            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            // Get woreda name for display
            var woreda = await _context.Locations.FindAsync(weeklyProvided.WoredaId);
            ViewBag.WoredaName = woreda?.LocationAmharicName ?? "ያልተመረጠ";

            return View(weeklyProvided);
        }

        // POST: WeeklyProvided/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( WeeklyProvided weeklyProvided,
            List<WeeklyProvidedDetailViewModel> Details)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", weeklyProvided.WoredaId);

            ViewBag.SupplyTypes = await _context.SupplyTypes
                .Where(s => s.IsActive)
                .OrderBy(s => s.SupplyTypeName)
                .ToListAsync();

            // Remove validation for navigation properties
            ModelState.Remove("WeeklyProvidedDetail");
            ModelState.Remove("Woreda");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                weeklyProvided.ReportedBy = userId;
                weeklyProvided.WoredaId = currentUser.LocationId;
                if (Details == null || !Details.Any())
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ የእርዳታ ዝርዝር ያስገቡ");
                    return View(weeklyProvided);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (weeklyProvided.ReportDate == default)
                    {
                        weeklyProvided.ReportDate = DateTime.Now;
                    }

                    weeklyProvided.WeeklyProvidedId = Guid.NewGuid();
                    _context.WeeklyProvideds.Add(weeklyProvided);
                    await _context.SaveChangesAsync();

                    // Save each detail
                    foreach (var detail in Details)
                    {
                        var weeklyDetail = new WeeklyProvidedDetail
                        {
                            WeeklyProvidedDetailId = Guid.NewGuid(),
                            WeeklyProvidedId = weeklyProvided.WeeklyProvidedId,
                            SupplyTypeId = detail.SupplyTypeId,
                            Measurement = detail.Measurement,
                            ProvidedQuantity = detail.ProvidedQuantity,
                            DistributedQuantity = detail.DistributedQuantity,
                            Donor = detail.Donor
                        };

                        // Calculate percentage
                        if (detail.ProvidedQuantity > 0)
                        {
                            weeklyDetail.PercentageOfDistributedFromProvided =
                                (detail.DistributedQuantity / detail.ProvidedQuantity) * 100;
                        }

                        _context.WeeklyProvidedDetails.Add(weeklyDetail);
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["Success"] = $"የሳምንታዊ እርዳታ ሪፖርት በሚገባ ተመዝግቧል. {Details.Count} ዝርዝሮች ተመዝግበዋል.";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = weeklyProvided.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = weeklyProvided.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = weeklyProvided.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "OtherProblems", new
                    {
                        startDate = weeklyProvided.StartDate.ToString("yyyy-MM-dd"),
                        endDate = weeklyProvided.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = weeklyProvided.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(weeklyProvided);
        }

        // GET: WeeklyProvideds/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weeklyProvided = await _context.WeeklyProvideds.FindAsync(id);
            if (weeklyProvided == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", weeklyProvided.WoredaId);
            return View(weeklyProvided);
        }

        // POST: WeeklyProvideds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("WeeklyProvidedId,StartDate,EndDate,WoredaId,IfProblem,Notes,ReportDate,ReportedBy,ReportStatus")] WeeklyProvided weeklyProvided)
        {
            if (id != weeklyProvided.WeeklyProvidedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(weeklyProvided);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WeeklyProvidedExists(weeklyProvided.WeeklyProvidedId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", weeklyProvided.WoredaId);
            return View(weeklyProvided);
        }

        // GET: WeeklyProvideds/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weeklyProvided = await _context.WeeklyProvideds
                .Include(w => w.Woreda)
                .FirstOrDefaultAsync(m => m.WeeklyProvidedId == id);
            if (weeklyProvided == null)
            {
                return NotFound();
            }

            return View(weeklyProvided);
        }

        // POST: WeeklyProvideds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var weeklyProvided = await _context.WeeklyProvideds.FindAsync(id);
            if (weeklyProvided != null)
            {
                _context.WeeklyProvideds.Remove(weeklyProvided);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WeeklyProvidedExists(Guid id)
        {
            return _context.WeeklyProvideds.Any(e => e.WeeklyProvidedId == id);
        }
    }
    public class WeeklyProvidedDetailViewModel
    {
        public Guid SupplyTypeId { get; set; }
        public Measurement Measurement { get; set; }
        public decimal ProvidedQuantity { get; set; }
        public decimal DistributedQuantity { get; set; }
        public string? Donor { get; set; }
    }
}
