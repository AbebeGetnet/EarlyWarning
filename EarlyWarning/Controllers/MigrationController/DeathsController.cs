using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.Migration;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Claims;

namespace EarlyWarning.Controllers.MigrationController
{
    public class DeathsController : Controller
    {
        private readonly EarlyWarningDbContext _context; 
        private readonly UserManager<ApplicationUser> _userManager;

        public DeathsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;            
            _userManager = userManager;
        }

        // GET: Deaths
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.Deaths.Include(d => d.Woreda).Include(m => m.DeathDetails);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: Deaths/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var death = await _context.Deaths
                .Include(d => d.Woreda)
                .FirstOrDefaultAsync(m => m.DeathId == id);
            if (death == null)
            {
                return NotFound();
            }

            return View(death);
        }
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: Deaths/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var migration = new Death();

            // Pre-populate from query string parameters (coming from WeeklyAccidents redirect)
            if (startDate.HasValue)
                migration.StartDate = startDate.Value;
            else
                migration.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                migration.EndDate = endDate.Value;
            else
                migration.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                migration.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                migration.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                migration.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                migration.ReportedBy = currentUser?.Id;
            }

            migration.ReportDate = DateTime.Now;
            migration.ApprovalStatus = ApprovalStatus.በሂደት_ላይ;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                migration.WoredaId
            );

            // Get all kebeles for migration details (where people are migrating from/to)
            ViewBag.Kebeles = await _context.Locations
                .Where(x => x.Level == LocationLevel.ቀበሌ && x.IsActive)
                .OrderBy(x => x.LocationAmharicName)
                .ToListAsync();

            // Get migration reasons (if you have a reasons table)


            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            // Get woreda name for display
            var woreda = await _context.Locations.FindAsync(migration.WoredaId);
            ViewBag.WoredaName = woreda?.LocationAmharicName ?? "ያልተመረጠ";

            return View(migration);
        }

        // POST: Death/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Death death,
            List<DeathDetailItem> DeathDetails)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag in case of error
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", death.WoredaId);

            // Remove validation for DeathDetails
            ModelState.Remove("DeathDetails");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                death.ReportedBy = userId;
                death.WoredaId = currentUser.LocationId;
                // Check if death exists but no details
                if (death.HasDeath && (DeathDetails == null || !DeathDetails.Any()))
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ የሞት መረጃ ይመዝግቡ");
                    return View(death);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (death.ReportDate == default)
                    {
                        death.ReportDate = DateTime.Now;
                    }

                    // Generate new ID
                    death.DeathId = Guid.NewGuid();

                    // Save the main death report
                    _context.Deaths.Add(death);
                    await _context.SaveChangesAsync();

                    // Save death details
                    if (death.HasDeath && DeathDetails != null && DeathDetails.Any())
                    {
                        foreach (var item in DeathDetails)
                        {
                            // Calculate total deaths for validation
                            var totalDeaths = item.MaleHouseholdHeads + item.FemaleHouseholdHeads +
                                             item.MaleFamilyMembers + item.FemaleFamilyMembers +
                                             item.MaleChildren + item.FemaleChildren +
                                             item.MaleYouth + item.FemaleYouth +
                                             item.MaleElderly + item.FemaleElderly +
                                             item.MaleDisabled + item.FemaleDisabled;

                            // Only save if there is at least one death
                            if (totalDeaths > 0)
                            {
                                var detail = new DeathDetail
                                {
                                    DeathDetailId = Guid.NewGuid(),
                                    DeathId = death.DeathId,
                                    DeathReason = item.DeathReason ?? string.Empty,
                                    MaleHouseholdHeads = item.MaleHouseholdHeads,
                                    FemaleHouseholdHeads = item.FemaleHouseholdHeads,
                                    MaleFamilyMembers = item.MaleFamilyMembers,
                                    FemaleFamilyMembers = item.FemaleFamilyMembers,
                                    MaleChildren = item.MaleChildren,
                                    FemaleChildren = item.FemaleChildren,
                                    MaleYouth = item.MaleYouth,
                                    FemaleYouth = item.FemaleYouth,
                                    MaleElderly = item.MaleElderly,
                                    FemaleElderly = item.FemaleElderly,
                                    MaleDisabled = item.MaleDisabled,
                                    FemaleDisabled = item.FemaleDisabled,
                                    Notes = item.Notes
                                };
                                _context.DeathDetails.Add(detail);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የሞት ሪፖርት በሚገባ ተመዝግቧል";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = death.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = death.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = death.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "AssistanceRecipients", new
                    {
                        startDate = death.StartDate.ToString("yyyy-MM-dd"),
                        endDate = death.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = death.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(death);
        }

        // GET: Deaths/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var death = await _context.Deaths
                .Include(d => d.DeathDetails)
                .FirstOrDefaultAsync(d => d.DeathId == id);

            if (death == null)
            {
                return NotFound();
            }

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", death.WoredaId);

            return View(death);
        }

        // POST: Death/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("DeathId,StartDate,EndDate,HasDeath,WoredaId,ReportDate,ReportedBy,ApprovalStatus")] Death death,
            List<DeathDetailItem> DeathDetails)
        {
            if (id != death.DeathId)
            {
                return NotFound();
            }

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", death.WoredaId);

            ModelState.Remove("DeathDetails");

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Update main death report
                    _context.Update(death);
                    await _context.SaveChangesAsync();

                    // Remove existing details
                    var existingDetails = await _context.DeathDetails
                        .Where(d => d.DeathId == id)
                        .ToListAsync();
                    _context.DeathDetails.RemoveRange(existingDetails);

                    // Add new details
                    if (death.HasDeath && DeathDetails != null && DeathDetails.Any())
                    {
                        foreach (var item in DeathDetails)
                        {
                            var totalDeaths = item.MaleHouseholdHeads + item.FemaleHouseholdHeads +
                                             item.MaleFamilyMembers + item.FemaleFamilyMembers +
                                             item.MaleChildren + item.FemaleChildren +
                                             item.MaleYouth + item.FemaleYouth +
                                             item.MaleElderly + item.FemaleElderly +
                                             item.MaleDisabled + item.FemaleDisabled;

                            if (totalDeaths > 0)
                            {
                                var detail = new DeathDetail
                                {
                                    DeathDetailId = Guid.NewGuid(),
                                    DeathId = death.DeathId,
                                    DeathReason = item.DeathReason ?? string.Empty,
                                    MaleHouseholdHeads = item.MaleHouseholdHeads,
                                    FemaleHouseholdHeads = item.FemaleHouseholdHeads,
                                    MaleFamilyMembers = item.MaleFamilyMembers,
                                    FemaleFamilyMembers = item.FemaleFamilyMembers,
                                    MaleChildren = item.MaleChildren,
                                    FemaleChildren = item.FemaleChildren,
                                    MaleYouth = item.MaleYouth,
                                    FemaleYouth = item.FemaleYouth,
                                    MaleElderly = item.MaleElderly,
                                    FemaleElderly = item.FemaleElderly,
                                    MaleDisabled = item.MaleDisabled,
                                    FemaleDisabled = item.FemaleDisabled,
                                    Notes = item.Notes
                                };
                                _context.DeathDetails.Add(detail);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የሞት ሪፖርት በሚገባ ተሻሽሏል";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስተካከል አልተቻለም: " + ex.Message);
                }
            }

            return View(death);
        }

        // GET: Deaths/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var death = await _context.Deaths
                .Include(d => d.Woreda)
                .FirstOrDefaultAsync(m => m.DeathId == id);
            if (death == null)
            {
                return NotFound();
            }

            return View(death);
        }

        // POST: Deaths/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var death = await _context.Deaths.FindAsync(id);
            if (death != null)
            {
                _context.Deaths.Remove(death);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeathExists(Guid id)
        {
            return _context.Deaths.Any(e => e.DeathId == id);
        }
    }
    public class DeathDetailItem
    {
        public string? DeathReason { get; set; }
        public int MaleHouseholdHeads { get; set; }
        public int FemaleHouseholdHeads { get; set; }
        public int MaleFamilyMembers { get; set; }
        public int FemaleFamilyMembers { get; set; }
        public int MaleChildren { get; set; }
        public int FemaleChildren { get; set; }
        public int MaleYouth { get; set; }
        public int FemaleYouth { get; set; }
        public int MaleElderly { get; set; }
        public int FemaleElderly { get; set; }
        public int MaleDisabled { get; set; }
        public int FemaleDisabled { get; set; }
        public string? Notes { get; set; }
    }
}
