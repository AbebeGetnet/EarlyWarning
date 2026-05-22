using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.WeeklyAccidents;
using EarlyWarning.Enums;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using EarlyWarning.Models.AnimalPrice;
using System.Security.Claims;

namespace EarlyWarning.Controllers.WeeklyAccidentsController
{
    public class WeeklyAccidentsController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public WeeklyAccidentsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: WeeklyAccidents
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.WeeklyAccidents.Include(w => w.Woreda)
                .Include(w => w.AccidentDetails)           // 🆕 Include AccidentDetails
                .ThenInclude(d => d.AccidentType);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: WeeklyAccidents/Details/5
        // GET: WeeklyAccidents/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weeklyAccidents = await _context.WeeklyAccidents
                .Include(w => w.Woreda)
                .Include(w => w.AccidentDetails)
                .ThenInclude(d => d.AccidentType)
                .FirstOrDefaultAsync(w => w.WeeklyAccidentsId == id);

            if (weeklyAccidents == null)
            {
                return NotFound();
            }

            return View(weeklyAccidents);
        }

        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: WeeklyAccidents/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var weeklyAccident = new WeeklyAccidents();

            // Pre-populate from query string parameters (coming from AnimalPricePerUnits redirect)
            if (startDate.HasValue)
                weeklyAccident.StartDate = startDate.Value;
            else
                weeklyAccident.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                weeklyAccident.EndDate = endDate.Value;
            else
                weeklyAccident.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                weeklyAccident.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                weeklyAccident.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                weeklyAccident.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                weeklyAccident.ReportedBy = currentUser?.Id;
            }

            weeklyAccident.ReportDate = DateTime.Now;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                weeklyAccident.WoredaId
            );

            // Get Accident Types for multiple selection
            ViewBag.AccidentTypes = await _context.AccidentTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccidentName)
                .ToListAsync();

            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            // Get woreda name for display
            var woreda = await _context.Locations.FindAsync(weeklyAccident.WoredaId);
            ViewBag.WoredaName = woreda?.LocationAmharicName ?? "ያልተመረጠ";

            return View(weeklyAccident);
        }

        // POST: WeeklyAccidents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
             WeeklyAccidents weeklyAccidents,
            List<AccidentDetailItem> AccidentDetails)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", weeklyAccidents.WoredaId);

            ViewBag.AccidentTypes = await _context.AccidentTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccidentName)
                .ToListAsync();

            // Remove validation for properties that will be set per item
            ModelState.Remove("AccidentDetails");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                weeklyAccidents.ReportedBy = userId;
                weeklyAccidents.WoredaId = currentUser.LocationId;
                // Check if accident exists but no details
                if (weeklyAccidents.HasAccident && (AccidentDetails == null || !AccidentDetails.Any()))
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ አደጋ ይመዝግቡ");
                    return View(weeklyAccidents);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (weeklyAccidents.ReportDate == default)
                    {
                        weeklyAccidents.ReportDate = DateTime.Now;
                    }

                    // Generate new ID
                    weeklyAccidents.WeeklyAccidentsId = Guid.NewGuid();

                    // Save the main report
                    _context.WeeklyAccidents.Add(weeklyAccidents);
                    await _context.SaveChangesAsync();

                    // Save accident details
                    if (weeklyAccidents.HasAccident && AccidentDetails != null)
                    {
                        foreach (var item in AccidentDetails)
                        {
                            var detail = new AccidentDetail
                            {
                                AccidentDetailId = Guid.NewGuid(),
                                WeeklyAccidentsId = weeklyAccidents.WeeklyAccidentsId,
                                AccidentTypeId = item.AccidentTypeId,
                                DamagedLandInHectares = item.DamagedLandInHectares,
                                DamageRateInPercent = item.DamageRateInPercent,
                                AffectedHouseholdsMale = item.AffectedHouseholdsMale,
                                AffectedHouseholdsFemale = item.AffectedHouseholdsFemale,
                                AffectedChildrenMale = item.AffectedChildrenMale,
                                AffectedChildrenFemale = item.AffectedChildrenFemale,
                                AffectedYouthMale = item.AffectedYouthMale,
                                AffectedYouthFemale = item.AffectedYouthFemale,
                                AffectedElderlyMale = item.AffectedElderlyMale,
                                AffectedElderlyFemale = item.AffectedElderlyFemale,
                                AffectedDisabledMale = item.AffectedDisabledMale,
                                AffectedDisabledFemale = item.AffectedDisabledFemale,
                                Notes = item.Notes
                            };
                            _context.AccidentDetails.Add(detail);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የሳምንታዊ አደጋ ሪፖርት በሚገባ ተመዝግቧል";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = weeklyAccidents.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = weeklyAccidents.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = weeklyAccidents.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "Migrations", new
                    {
                        startDate = weeklyAccidents.StartDate.ToString("yyyy-MM-dd"),
                        endDate = weeklyAccidents.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = weeklyAccidents.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(weeklyAccidents);
        }

        // GET: WeeklyAccidents/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weeklyAccidents = await _context.WeeklyAccidents
                .Include(w => w.AccidentDetails)
                .FirstOrDefaultAsync(w => w.WeeklyAccidentsId == id);

            if (weeklyAccidents == null)
            {
                return NotFound();
            }

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", weeklyAccidents.WoredaId);

            ViewBag.AccidentTypes = await _context.AccidentTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccidentName)
                .ToListAsync();

            return View(weeklyAccidents);
        }

        // POST: WeeklyAccidents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("WeeklyAccidentsId,StartDate,EndDate,HasAccident,WoredaId,Notes,ReportDate,ReportedBy,ApprovalStatus")] WeeklyAccidents weeklyAccidents,
            List<AccidentDetailItem> AccidentDetails)
        {
            if (id != weeklyAccidents.WeeklyAccidentsId)
            {
                return NotFound();
            }

            // Reload ViewBag data
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", weeklyAccidents.WoredaId);

            ViewBag.AccidentTypes = await _context.AccidentTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccidentName)
                .ToListAsync();

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Update main report
                    _context.Update(weeklyAccidents);
                    await _context.SaveChangesAsync();

                    // Remove existing details
                    var existingDetails = await _context.AccidentDetails
                        .Where(d => d.WeeklyAccidentsId == id)
                        .ToListAsync();
                    _context.AccidentDetails.RemoveRange(existingDetails);

                    // Add new details
                    if (weeklyAccidents.HasAccident && AccidentDetails != null)
                    {
                        foreach (var item in AccidentDetails)
                        {
                            var detail = new AccidentDetail
                            {
                                AccidentDetailId = Guid.NewGuid(),
                                WeeklyAccidentsId = weeklyAccidents.WeeklyAccidentsId,
                                AccidentTypeId = item.AccidentTypeId,
                                DamagedLandInHectares = item.DamagedLandInHectares,
                                DamageRateInPercent = item.DamageRateInPercent,
                                AffectedHouseholdsMale = item.AffectedHouseholdsMale,
                                AffectedHouseholdsFemale = item.AffectedHouseholdsFemale,
                                AffectedChildrenMale = item.AffectedChildrenMale,
                                AffectedChildrenFemale = item.AffectedChildrenFemale,
                                AffectedYouthMale = item.AffectedYouthMale,
                                AffectedYouthFemale = item.AffectedYouthFemale,
                                AffectedElderlyMale = item.AffectedElderlyMale,
                                AffectedElderlyFemale = item.AffectedElderlyFemale,
                                AffectedDisabledMale = item.AffectedDisabledMale,
                                AffectedDisabledFemale = item.AffectedDisabledFemale,
                                Notes = item.Notes
                            };
                            _context.AccidentDetails.Add(detail);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የሳምንታዊ አደጋ ሪፖርት በሚገባ ተሻሽሏል";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስተካከል አልተቻለም: " + ex.Message);
                }
            }

            return View(weeklyAccidents);
        }

        

        // GET: WeeklyAccidents/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weeklyAccidents = await _context.WeeklyAccidents
                .Include(w => w.Woreda)
                .Include(w => w.AccidentDetails)
                .ThenInclude(d => d.AccidentType)
                .FirstOrDefaultAsync(w => w.WeeklyAccidentsId == id);

            if (weeklyAccidents == null)
            {
                return NotFound();
            }

            return View(weeklyAccidents);
        }

        // POST: WeeklyAccidents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove details first
                var details = await _context.AccidentDetails
                    .Where(d => d.WeeklyAccidentsId == id)
                    .ToListAsync();
                _context.AccidentDetails.RemoveRange(details);

                // Remove main report
                var weeklyAccidents = await _context.WeeklyAccidents.FindAsync(id);
                if (weeklyAccidents != null)
                {
                    _context.WeeklyAccidents.Remove(weeklyAccidents);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "የሳምንታዊ አደጋ ሪፖርት በሚገባ ተሰርዟል";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "ውሂቡን ለመሰረዝ አልተቻለም: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
     // ViewModel for multiple accident details
    public class AccidentDetailItem
    {
        public Guid AccidentTypeId { get; set; }
        public decimal? DamagedLandInHectares { get; set; }
        public decimal? DamageRateInPercent { get; set; }
        public int AffectedHouseholdsMale { get; set; }
        public int AffectedHouseholdsFemale { get; set; }
        public int AffectedChildrenMale { get; set; }
        public int AffectedChildrenFemale { get; set; }
        public int AffectedYouthMale { get; set; }
        public int AffectedYouthFemale { get; set; }
        public int AffectedElderlyMale { get; set; }
        public int AffectedElderlyFemale { get; set; }
        public int AffectedDisabledMale { get; set; }
        public int AffectedDisabledFemale { get; set; }
        public string? Notes { get; set; }
    }
}

