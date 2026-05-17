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
using EarlyWarning.ViewModels.GrainViewModel;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using EarlyWarning.Models.PriceofGrain;
using System.Security.Claims;

namespace EarlyWarning.Controllers.MigrationController
{
    public class MigrationsController : Controller
    {
        private readonly EarlyWarningDbContext _context; 
        private readonly UserManager<ApplicationUser> _userManager;

        public MigrationsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        // GET: Migration/Create
       
        // GET: Migrations
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = await _context.Migrations
        .AsNoTracking()
        .Include(m => m.Woreda)
        .Include(m => m.MigrationDetails)
            .ThenInclude(d => d.OriginLocation)
        .OrderByDescending(m => m.ReportDate)
        .ToListAsync();
            return View(earlyWarningDbContext);
        }

        // GET: Migrations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migration = await _context.Migrations
                .Include(m => m.Woreda).Include(m => m.MigrationDetails)
                    .ThenInclude(d => d.OriginLocation)
                .FirstOrDefaultAsync(m => m.MigrationId == id);
            if (migration == null)
            {
                return NotFound();
            }

            return View(migration);
        }
        // Helper method to get Monday of current week
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: Migration/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var migration = new Migration();

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

        // POST: Migration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
             Models.Migration.Migration migration,
            List<MigrationDetailViewModel> MigrationDetails)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", migration.WoredaId);

            var locations = await _context.Locations
                .Where(x => x.Level == LocationLevel.ቀበሌ && x.IsActive)
                .OrderBy(x => x.LocationAmharicName)
                .ToListAsync();
            ViewBag.Locations = locations;

            // Remove validation for navigation properties
            ModelState.Remove("MigrationDetails");
            ModelState.Remove("Woreda");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                migration.ReportedBy = userId;
                migration.WoredaId = currentUser.LocationId;
                if (migration.HasMigration && (MigrationDetails == null || !MigrationDetails.Any()))
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ የስደት ዝርዝር ያስገቡ");
                    return View(migration);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    migration.MigrationId = Guid.NewGuid();
                    if (migration.ReportDate == default)
                    {
                        migration.ReportDate = DateTime.Now;
                    }

                    _context.Migrations.Add(migration);
                    await _context.SaveChangesAsync();

                    if (migration.HasMigration && MigrationDetails != null && MigrationDetails.Any())
                    {
                        foreach (var detail in MigrationDetails)
                        {
                            // Calculate total migrants to ensure there is at least one
                            var totalMigrants = detail.MaleHouseholdHeads + detail.FemaleHouseholdHeads +
                                               detail.MaleFamilyMembers + detail.FemaleFamilyMembers +
                                               detail.MaleChildren + detail.FemaleChildren +
                                               detail.MaleYouth + detail.FemaleYouth +
                                               detail.MaleElderly + detail.FemaleElderly +
                                               detail.MaleDisabled + detail.FemaleDisabled;

                            // Only save if there is at least one migrant OR we have origin info
                            if (totalMigrants > 0)
                            {
                                var migrationDetail = new MigrationDetail
                                {
                                    MigrationDetailId = Guid.NewGuid(),
                                    MigrationId = migration.MigrationId,
                                    OriginLocationId = detail.OriginLocationId,
                                    MigrationReason = detail.MigrationReason ?? string.Empty,
                                    MaleHouseholdHeads = detail.MaleHouseholdHeads,
                                    FemaleHouseholdHeads = detail.FemaleHouseholdHeads,
                                    MaleFamilyMembers = detail.MaleFamilyMembers,
                                    FemaleFamilyMembers = detail.FemaleFamilyMembers,
                                    MaleChildren = detail.MaleChildren,
                                    FemaleChildren = detail.FemaleChildren,
                                    MaleYouth = detail.MaleYouth,
                                    FemaleYouth = detail.FemaleYouth,
                                    MaleElderly = detail.MaleElderly,
                                    FemaleElderly = detail.FemaleElderly,
                                    MaleDisabled = detail.MaleDisabled,
                                    FemaleDisabled = detail.FemaleDisabled,
                                    Notes = detail.Notes
                                };
                                _context.MigrationDetails.Add(migrationDetail);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = $"የስደት ሪፖርት በሚገባ ተመዝግቧል. {MigrationDetails.Count} የስደት ዝርዝሮች ተመዝግበዋል.";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = migration.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = migration.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = migration.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "Deaths", new
                    {
                        startDate = migration.StartDate.ToString("yyyy-MM-dd"),
                        endDate = migration.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = migration.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(migration);
        }


        // GET: Migrations/Edit/5
        // GET: Migration/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migration = await _context.Migrations
                .Include(m => m.MigrationDetails)
                .FirstOrDefaultAsync(m => m.MigrationId == id);

            if (migration == null)
            {
                return NotFound();
            }

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", migration.WoredaId);

            var locations = await _context.Locations
                .Where(x => x.IsActive)
                .OrderBy(x => x.LocationAmharicName)
                .ToListAsync();
            ViewBag.Locations = locations;

            return View(migration);
        }

        // POST: Migration/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("MigrationId,StartDate,EndDate,HasMigration,WoredaId,GeneralNotes,ReportDate,ReportedBy,ApprovalStatus")] Models.Migration.Migration migration,
            List<MigrationDetailViewModel> MigrationDetails)
        {
            if (id != migration.MigrationId)
            {
                return NotFound();
            }

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", migration.WoredaId);

            var locations = await _context.Locations
                .Where(x => x.IsActive)
                .OrderBy(x => x.LocationAmharicName)
                .ToListAsync();
            ViewBag.Locations = locations;

            ModelState.Remove("MigrationDetails");
            ModelState.Remove("Woreda");

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.Update(migration);
                    await _context.SaveChangesAsync();

                    // Remove existing details
                    var existingDetails = await _context.MigrationDetails
                        .Where(d => d.MigrationId == id)
                        .ToListAsync();
                    _context.MigrationDetails.RemoveRange(existingDetails);

                    // Add new details
                    if (migration.HasMigration && MigrationDetails != null && MigrationDetails.Any())
                    {
                        foreach (var detail in MigrationDetails)
                        {
                            var totalMigrants = detail.MaleHouseholdHeads + detail.FemaleHouseholdHeads +
                                               detail.MaleFamilyMembers + detail.FemaleFamilyMembers +
                                               detail.MaleChildren + detail.FemaleChildren +
                                               detail.MaleYouth + detail.FemaleYouth +
                                               detail.MaleElderly + detail.FemaleElderly +
                                               detail.MaleDisabled + detail.FemaleDisabled;

                            if (totalMigrants > 0)
                            {
                                var migrationDetail = new MigrationDetail
                                {
                                    MigrationDetailId = Guid.NewGuid(),
                                    MigrationId = migration.MigrationId,
                                    OriginLocationId = detail.OriginLocationId,
                                    MigrationReason = detail.MigrationReason ?? string.Empty,
                                    MaleHouseholdHeads = detail.MaleHouseholdHeads,
                                    FemaleHouseholdHeads = detail.FemaleHouseholdHeads,
                                    MaleFamilyMembers = detail.MaleFamilyMembers,
                                    FemaleFamilyMembers = detail.FemaleFamilyMembers,
                                    MaleChildren = detail.MaleChildren,
                                    FemaleChildren = detail.FemaleChildren,
                                    MaleYouth = detail.MaleYouth,
                                    FemaleYouth = detail.FemaleYouth,
                                    MaleElderly = detail.MaleElderly,
                                    FemaleElderly = detail.FemaleElderly,
                                    MaleDisabled = detail.MaleDisabled,
                                    FemaleDisabled = detail.FemaleDisabled,
                                    Notes = detail.Notes
                                };
                                _context.MigrationDetails.Add(migrationDetail);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የስደት ሪፖርት በሚገባ ተሻሽሏል";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስተካከል አልተቻለም: " + ex.Message);
                }
            }

            return View(migration);
        }


        // GET: Migrations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migration = await _context.Migrations
                .Include(m => m.Woreda)
                .FirstOrDefaultAsync(m => m.MigrationId == id);
            if (migration == null)
            {
                return NotFound();
            }

            return View(migration);
        }

        // POST: Migrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var migration = await _context.Migrations.FindAsync(id);
            if (migration != null)
            {
                _context.Migrations.Remove(migration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MigrationExists(Guid id)
        {
            return _context.Migrations.Any(e => e.MigrationId == id);
        }
    }
}
