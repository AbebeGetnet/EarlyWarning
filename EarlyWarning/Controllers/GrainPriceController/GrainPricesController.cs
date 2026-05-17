using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models.PriceofGrain;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using System.Security.Claims;

namespace EarlyWarning.Controllers.GrainPriceController
{
    public class GrainPricesController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GrainPricesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GrainPrices
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.GrainPrices
                .Include(g => g.Woreda)
                .Include(g => g.GrainPriceDetails)
                    .ThenInclude(d => d.GrainPriceIncreaseDecrease);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: GrainPrices/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPrice = await _context.GrainPrices
                .Include(g => g.Woreda)
                .Include(g => g.GrainPriceDetails)
                    .ThenInclude(d => d.GrainPriceIncreaseDecrease)
                .FirstOrDefaultAsync(m => m.GrainPriceId == id);
            if (grainPrice == null)
            {
                return NotFound();
            }

            return View(grainPrice);
        }
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: GrainPrices/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var grainPricePerQuintal = new GrainPricePerQuintal();

            // Pre-populate from query string parameters (coming from GrainSupply redirect)
            if (startDate.HasValue)
                grainPricePerQuintal.StartDate = startDate.Value;
            else
                grainPricePerQuintal.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                grainPricePerQuintal.EndDate = endDate.Value;
            else
                grainPricePerQuintal.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                grainPricePerQuintal.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                grainPricePerQuintal.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                grainPricePerQuintal.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                grainPricePerQuintal.ReportedBy = currentUser?.Id;
            }

            grainPricePerQuintal.ReportDate = DateTime.Now;
            grainPricePerQuintal.ApprovalStatus = ApprovalStatus.በሂደት_ላይ;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                grainPricePerQuintal.WoredaId
            );

            // Load reasons
            ViewBag.IncreaseReasons = await _context.GrainPriceIncreaseDecreases
                .Where(x => x.ReasonType == ReasonType.Increase)
                .OrderBy(x => x.ReasonName)
                .ToListAsync();

            ViewBag.DecreaseReasons = await _context.GrainPriceIncreaseDecreases
                .Where(x => x.ReasonType == ReasonType.Decrease)
                .OrderBy(x => x.ReasonName)
                .ToListAsync();

            // Load grain types
            ViewBag.GrainTypes = await _context.GrainTypes
                .Where(g => g.IsActive)
                .OrderBy(g => g.GrainName)
                .ToListAsync();

            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            return View(grainPricePerQuintal);
        }

        // POST: GrainPrices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( GrainPrice grainPrice, List<Guid> SelectedReasonIds)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(_context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToList(), "LocationId", "LocationAmharicName", grainPrice.WoredaId);

            // Load reasons for ViewBag
            ViewBag.IncreaseReasons = await _context.GrainPriceIncreaseDecreases
                .Where(r => r.ReasonType == ReasonType.Increase)

                .ToListAsync();

            ViewBag.DecreaseReasons = await _context.GrainPriceIncreaseDecreases
                .Where(r => r.ReasonType == ReasonType.Decrease)

                .ToListAsync();
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                grainPrice.ReportedBy = userId;
                grainPrice.WoredaId = currentUser.LocationId;
                // Check if reasons are selected when status is Increased or Decreased
                if ((grainPrice.GrainSupplyStatus == SupplyStatus.Increased ||
                     grainPrice.GrainSupplyStatus == SupplyStatus.Decreased) &&
                    (SelectedReasonIds == null || !SelectedReasonIds.Any()))
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ ምክንያት ይምረጡ");
                    return View(grainPrice);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (grainPrice.ReportDate == default)
                    {
                        grainPrice.ReportDate = DateTime.Now;
                    }

                    // Generate new ID
                    grainPrice.GrainPriceId = Guid.NewGuid();

                    // Save the main report
                    _context.GrainPrices.Add(grainPrice);
                    await _context.SaveChangesAsync();

                    // Save selected reasons
                    if (SelectedReasonIds != null && SelectedReasonIds.Any())
                    {
                        foreach (var reasonId in SelectedReasonIds)
                        {
                            var detail = new GrainPriceDetail
                            {
                                GrainPriceDetailId = Guid.NewGuid(),
                                GrainPriceId = grainPrice.GrainPriceId,
                                GrainPriceIncreaseDecreaseId = reasonId
                            };
                            _context.GrainPriceDetails.Add(detail);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የእህል አቅርቦት ሪፖርት በሚገባ ተመዝግቧል";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = grainPrice.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = grainPrice.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = grainPrice.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "AnimalPricePerUnits", new
                    {
                        startDate = grainPrice.StartDate.ToString("yyyy-MM-dd"),
                        endDate = grainPrice.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = grainPrice.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }
            return View(grainPrice);
        }

        // GET: GrainPrices/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPrice = await _context.GrainPrices.FindAsync(id);
            if (grainPrice == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", grainPrice.WoredaId);
            return View(grainPrice);
        }

        // POST: GrainPrices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainPriceId,StartDate,EndDate,GrainSupplyStatus,WoredaId,AdditionalNotes,ReportDate,ReportedBy,ApprovalStatus")] GrainPrice grainPrice)
        {
            if (id != grainPrice.GrainPriceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grainPrice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrainPriceExists(grainPrice.GrainPriceId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", grainPrice.WoredaId);
            return View(grainPrice);
        }

        // GET: GrainPrices/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPrice = await _context.GrainPrices
                .Include(g => g.Woreda)
                .FirstOrDefaultAsync(m => m.GrainPriceId == id);
            if (grainPrice == null)
            {
                return NotFound();
            }

            return View(grainPrice);
        }

        // POST: GrainPrices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var grainPrice = await _context.GrainPrices.FindAsync(id);
            if (grainPrice != null)
            {
                _context.GrainPrices.Remove(grainPrice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrainPriceExists(Guid id)
        {
            return _context.GrainPrices.Any(e => e.GrainPriceId == id);
        }
    }
}
