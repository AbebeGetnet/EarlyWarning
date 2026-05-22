using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.AnimalPrice;
using EarlyWarning.Enums;
using EarlyWarning.Models.PriceofGrain;
using System.ComponentModel.DataAnnotations;
using EarlyWarning.ViewModels.GrainViewModel;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using System.Security.Claims;

namespace EarlyWarning.Controllers.AnimalPriceController
{
    public class AnimalPricePerUnitsController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnimalPricePerUnitsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AnimalPricePerUnits
        //public async Task<IActionResult> Index()
        //{
        //    var earlyWarningDbContext = _context.AnimalPricePerUnits.Include(a => a.AnimalType).Include(a => a.Woreda);
        //    return View(await earlyWarningDbContext.ToListAsync());
        //}
        // GET: AnimalPricePerUnit/Index
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, Guid? woredaId, Guid? animalTypeId)
        {
            var query = _context.AnimalPricePerUnits
                .Include(a => a.AnimalType)
                .Include(a => a.Woreda)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.EndDate <= endDate.Value);
            if (woredaId.HasValue && woredaId.Value != Guid.Empty)
                query = query.Where(a => a.WoredaId == woredaId.Value);
            if (animalTypeId.HasValue && animalTypeId.Value != Guid.Empty)
                query = query.Where(a => a.AnimalTypeId == animalTypeId.Value);

            var reports = await query.OrderByDescending(a => a.ReportDate).ToListAsync();

            // 🆕 Calculate previous price for each report
            var reportsWithPreviousPrice = new List<AnimalPriceReportViewModel>();

            foreach (var report in reports)
            {
                // Get previous week's price for the same animal type and woreda
                var previousPrice = await _context.AnimalPricePerUnits
                    .Where(a => a.AnimalTypeId == report.AnimalTypeId
                             && a.WoredaId == report.WoredaId
                             && a.EndDate < report.StartDate)
                    .OrderByDescending(a => a.EndDate)
                    .Select(a => a.WeeklyPrice)
                    .FirstOrDefaultAsync();

                decimal? priceDifference = null;
                decimal? priceChangePercentage = null;

                if (previousPrice > 0)
                {
                    priceDifference = report.WeeklyPrice - previousPrice;
                    priceChangePercentage = (priceDifference.Value / previousPrice) * 100;
                }

                reportsWithPreviousPrice.Add(new AnimalPriceReportViewModel
                {
                    AnimalPricePerUnit = report,
                    PreviousPrice = previousPrice,
                    PriceDifference = priceDifference,
                    PriceChangePercentage = priceChangePercentage
                });
            }

            ViewBag.Woredas = await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .Select(x => new { x.Id, x.LocationAmharicName })
                .ToListAsync();

            ViewBag.AnimalTypes = await _context.AnimalTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AnimalTypeName)
                .ToListAsync();

            return View(reportsWithPreviousPrice);
        }
        // GET: AnimalPricePerUnits/Details/5
        // GET: AnimalPricePerUnit/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPrice = await _context.AnimalPricePerUnits
                .Include(a => a.AnimalType)
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.AnimalPricePerUnitId == id);

            if (animalPrice == null)
            {
                return NotFound();
            }

            return View(animalPrice);
        }

        
        // Helper method
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: AnimalPricePerUnit/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var animalPricePerUnit = new AnimalPricePerUnit();

            // Pre-populate from query string parameters (coming from AnimalSupplies redirect)
            if (startDate.HasValue)
                animalPricePerUnit.StartDate = startDate.Value;
            else
                animalPricePerUnit.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                animalPricePerUnit.EndDate = endDate.Value;
            else
                animalPricePerUnit.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                animalPricePerUnit.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                animalPricePerUnit.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                animalPricePerUnit.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                animalPricePerUnit.ReportedBy = currentUser?.Id;
            }

            animalPricePerUnit.ReportDate = DateTime.Now;
            animalPricePerUnit.ApprovalStatus = ApprovalStatus.በሂደት_ላይ;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                animalPricePerUnit.WoredaId
            );

            // Animal types dropdown
            ViewBag.AnimalTypes = await _context.AnimalTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AnimalTypeName)
                .ToListAsync();

            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            // Get woreda name for display
            var woreda = await _context.Locations.FindAsync(animalPricePerUnit.WoredaId);
            ViewBag.WoredaName = woreda?.LocationAmharicName ?? "ያልተመረጠ";

            return View(animalPricePerUnit);
        }

        // POST: AnimalPricePerUnit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( AnimalPricePerUnit animalPricePerUnit,
            List<AnimalPriceItem> AnimalPrices)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", animalPricePerUnit.WoredaId);

            ViewBag.AnimalTypes = await _context.AnimalTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AnimalTypeName)
                .ToListAsync();

            // Remove validation for properties that will be set per item
            ModelState.Remove("AnimalTypeId");
            ModelState.Remove("WeeklyPrice");
            ModelState.Remove("WeeklyMarketStatus");
            ModelState.Remove("PriceDifference");
            ModelState.Remove("PriceChangePercentage");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                animalPricePerUnit.ReportedBy = userId;
                animalPricePerUnit.WoredaId = currentUser.LocationId;
                if (AnimalPrices == null || !AnimalPrices.Any())
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ የእንስሳት ዋጋ ይመዝግቡ");
                    return View(animalPricePerUnit);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (animalPricePerUnit.ReportDate == default)
                    {
                        animalPricePerUnit.ReportDate = DateTime.Now;
                    }

                    // Save each animal price entry
                    foreach (var item in AnimalPrices)
                    {
                        var animalPrice = new AnimalPricePerUnit
                        {
                            AnimalPricePerUnitId = Guid.NewGuid(),
                            StartDate = animalPricePerUnit.StartDate,
                            EndDate = animalPricePerUnit.EndDate,
                            WoredaId = animalPricePerUnit.WoredaId,
                            AnimalTypeId = item.AnimalTypeId,
                            WeeklyPrice = item.WeeklyPrice,
                            WeeklyMarketStatus = item.WeeklyMarketStatus,
                            Notes = animalPricePerUnit.Notes,
                            ReportDate = animalPricePerUnit.ReportDate,
                            ReportedBy = animalPricePerUnit.ReportedBy,
                            ApprovalStatus = animalPricePerUnit.ApprovalStatus
                        };

                        // Calculate price difference (get previous week's price)
                        var previousPrice = await _context.AnimalPricePerUnits
                            .Where(a => a.AnimalTypeId == item.AnimalTypeId
                                     && a.WoredaId == animalPricePerUnit.WoredaId
                                     && a.EndDate < animalPricePerUnit.StartDate)
                            .OrderByDescending(a => a.EndDate)
                            .FirstOrDefaultAsync();

                        if (previousPrice != null)
                        {
                            animalPrice.PriceDifference = animalPrice.WeeklyPrice - previousPrice.WeeklyPrice;
                            if (previousPrice.WeeklyPrice > 0)
                            {
                                animalPrice.PriceChangePercentage = (animalPrice.PriceDifference / previousPrice.WeeklyPrice) * 100;
                            }
                        }

                        _context.AnimalPricePerUnits.Add(animalPrice);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"{AnimalPrices.Count} የእንስሳት ዋጋ ሪፖርቶች በሚገባ ተመዝግበዋል";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = animalPricePerUnit.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = animalPricePerUnit.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = animalPricePerUnit.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "WeeklyAccidents", new
                    {
                        startDate = animalPricePerUnit.StartDate.ToString("yyyy-MM-dd"),
                        endDate = animalPricePerUnit.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = animalPricePerUnit.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(animalPricePerUnit);
        }
        // GET: AnimalPricePerUnit/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPrice = await _context.AnimalPricePerUnits.FindAsync(id);
            if (animalPrice == null)
            {
                return NotFound();
            }

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", animalPrice.WoredaId);

            ViewBag.AnimalTypes = await _context.AnimalTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AnimalTypeName)
                .ToListAsync();

            return View(animalPrice);
        }

        // POST: AnimalPricePerUnit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AnimalPricePerUnitId,StartDate,EndDate,AnimalTypeId,WeeklyPrice,WeeklyMarketStatus,PriceDifference,PriceChangePercentage,WoredaId,Notes,ReportDate,ReportedBy,ApprovalStatus")] AnimalPricePerUnit animalPricePerUnit)
        {
            if (id != animalPricePerUnit.AnimalPricePerUnitId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalculate price difference if weekly price changed
                    var previousPrice = await _context.AnimalPricePerUnits.Include(x=>x.AnimalType)
                        .Where(a => a.AnimalTypeId == animalPricePerUnit.AnimalTypeId
                                 && a.WoredaId == animalPricePerUnit.WoredaId
                                 && a.EndDate < animalPricePerUnit.StartDate
                                 && a.AnimalPricePerUnitId != id)
                        .OrderByDescending(a => a.EndDate)
                        .FirstOrDefaultAsync();

                    if (previousPrice != null)
                    {
                        animalPricePerUnit.PriceDifference = animalPricePerUnit.WeeklyPrice - previousPrice.WeeklyPrice;
                        if (previousPrice.WeeklyPrice > 0)
                        {
                            animalPricePerUnit.PriceChangePercentage = (animalPricePerUnit.PriceDifference / previousPrice.WeeklyPrice) * 100;
                        }
                    }

                    _context.Update(animalPricePerUnit);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "የእንስሳት ዋጋ ሪፖርት በሚገባ ተሻሽሏል";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalPricePerUnitExists(animalPricePerUnit.AnimalPricePerUnitId))
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

            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", animalPricePerUnit.WoredaId);

            ViewBag.AnimalTypes = await _context.AnimalTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.AnimalTypeName)
                .ToListAsync();

            return View(animalPricePerUnit);
        }

        // GET: AnimalPricePerUnits/Delete/5
        // GET: AnimalPricePerUnit/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPrice = await _context.AnimalPricePerUnits
                .Include(a => a.AnimalType)
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.AnimalPricePerUnitId == id);

            if (animalPrice == null)
            {
                return NotFound();
            }

            return View(animalPrice);
        }

        // POST: AnimalPricePerUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalPrice = await _context.AnimalPricePerUnits.FindAsync(id);
            if (animalPrice != null)
            {
                _context.AnimalPricePerUnits.Remove(animalPrice);
                await _context.SaveChangesAsync();
                TempData["Success"] = "የእንስሳት ዋጋ ሪፖርት በሚገባ ተሰርዟል";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AnimalPricePerUnitExists(Guid id)
        {
            return _context.AnimalPricePerUnits.Any(e => e.AnimalPricePerUnitId == id);
        }
    }

    // ViewModel for multiple animal price entries
    public class AnimalPriceItem
    {
        [Required(ErrorMessage = "የእንስሳት ዓይነት መምረጥ ያስፈልጋል")]
        public Guid AnimalTypeId { get; set; }

        [Required(ErrorMessage = "የእንስሳት ዋጋ ያስፈልጋል")]
        [Range(0, 100000)]
        public decimal WeeklyPrice { get; set; }

        [Required(ErrorMessage = "የዋጋ ሁኔታ መምረጥ ያስፈልጋል")]
        public WeeklyStatus WeeklyMarketStatus { get; set; }

        public string? PriceNote { get; set; }
    }
}

