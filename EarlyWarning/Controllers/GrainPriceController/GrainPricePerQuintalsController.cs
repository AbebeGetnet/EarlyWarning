using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Enums;
using EarlyWarning.ViewModels.GrainViewModel;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using System.Security.Claims;

namespace EarlyWarning.Controllers.GrainPriceController
{
    public class GrainPricePerQuintalsController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GrainPricePerQuintalsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GrainPricePerQuintals
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, Guid? woredaId, Guid? grainTypeId)
        {
            var query = _context.GrainPricePerQuintals
                .Include(g => g.GrainType)
                .Include(g => g.Woreda)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(g => g.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(g => g.EndDate <= endDate.Value);
            if (woredaId.HasValue && woredaId.Value != Guid.Empty)
                query = query.Where(g => g.WoredaId == woredaId.Value);
            if (grainTypeId.HasValue && grainTypeId.Value != Guid.Empty)
                query = query.Where(g => g.GrainTypeId == grainTypeId.Value);

            var reports = await query.OrderByDescending(g => g.ReportDate).ToListAsync();

            // 🆕 Get all previous prices with a simpler approach
            var reportsWithPreviousPrice = new List<GrainPriceReportViewModel>();

            foreach (var report in reports)
            {
                // Get previous price for each report individually
                var previousPrice = await _context.GrainPricePerQuintals
                    .Where(g => g.GrainTypeId == report.GrainTypeId
                             && g.WoredaId == report.WoredaId
                             && g.EndDate < report.StartDate)
                    .OrderByDescending(g => g.EndDate)
                    .Select(g => g.WeeklyPrice)
                    .FirstOrDefaultAsync();

                decimal? priceDifference = null;
                decimal? priceChangePercentage = null;

                if (previousPrice > 0)
                {
                    priceDifference = report.WeeklyPrice - previousPrice;
                    priceChangePercentage = (priceDifference.Value / previousPrice) * 100;
                }

                reportsWithPreviousPrice.Add(new GrainPriceReportViewModel
                {
                    GrainPricePerQuintal = report,
                    PreviousPrice = previousPrice,
                    PriceDifference = priceDifference,
                    PriceChangePercentage = priceChangePercentage
                });
            }

            ViewBag.Woredas = await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .Select(x => new { WoredaId = x.Id, LocationAmharicName = x.LocationAmharicName })
                .ToListAsync();

            ViewBag.GrainTypes = await _context.GrainTypes
                .Where(g => g.IsActive)
                .OrderBy(g => g.GrainName)
                .ToListAsync();

            return View(reportsWithPreviousPrice);
        }
        // GET: GrainPricePerQuintals/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPricePerQuintal = await _context.GrainPricePerQuintals
                .Include(g => g.GrainType)
                .Include(g => g.Woreda)
                .FirstOrDefaultAsync(m => m.GrainPricePerQuintalId == id);
            if (grainPricePerQuintal == null)
            {
                return NotFound();
            }

            return View(grainPricePerQuintal);
        }

        // GET: GrainPricePerQuintals/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var grainPricePerQuintal = new GrainPricePerQuintal();

            // Pre-populate from query string parameters (coming from GrainSupply redirect)
            if (startDate.HasValue)
                grainPricePerQuintal.StartDate = startDate.Value;
            else
                grainPricePerQuintal.StartDate = DateTime.Now; // Fallback

            if (endDate.HasValue)
                grainPricePerQuintal.EndDate = endDate.Value;
            else
                grainPricePerQuintal.EndDate = DateTime.Now; // Fallback

            if (woredaId.HasValue)
                grainPricePerQuintal.WoredaId = woredaId.Value;
            else
            {
                // Fallback to current user's location
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

            // Set up the view data
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                grainPricePerQuintal.WoredaId
            );

            ViewBag.GrainTypes = await _context.GrainTypes
                .Where(g => g.IsActive)
                .OrderBy(g => g.GrainName)
                .ToListAsync();

            // Show continuation message if redirected from GrainSupply
            if (TempData["IsRedirectedFromGrainSupply"] != null)
                ViewBag.ShowContinuationMessage = true;

            // Store date range info for display
            ViewBag.DateRangeMessage = $"ሪፖርት ለሳምንቱ: {grainPricePerQuintal.StartDate:dd/MM/yyyy} - {grainPricePerQuintal.EndDate:dd/MM/yyyy}";

            return View(grainPricePerQuintal);
        }

        // POST: GrainPricePerQuintals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( GrainPricePerQuintal grainPricePerQuintal,
            List<GrainPriceItem> GrainPrices)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["WoredaId"] = new SelectList(await _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToListAsync(), "Id", "LocationAmharicName", grainPricePerQuintal.WoredaId);

            ViewBag.GrainTypes = await _context.GrainTypes
                .Where(g => g.IsActive)
                .OrderBy(g => g.GrainName)
                .ToListAsync();

            // Remove validation for properties that will be set per item
            ModelState.Remove("GrainTypeId");
            ModelState.Remove("WeeklyPrice");
            ModelState.Remove("WeeklyMarketStatus");
            ModelState.Remove("PriceDifference");
            ModelState.Remove("PriceChangePercentage");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                grainPricePerQuintal.ReportedBy = userId;
                grainPricePerQuintal.WoredaId = currentUser.LocationId;
                if (GrainPrices == null || !GrainPrices.Any())
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ እህል ይመዝግቡ");
                    return View(grainPricePerQuintal);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (grainPricePerQuintal.ReportDate == default)
                    {
                        grainPricePerQuintal.ReportDate = DateTime.Now;
                    }

                    // Save each grain price entry
                    foreach (var item in GrainPrices)
                    {
                        var grainPrice = new GrainPricePerQuintal
                        {
                            GrainPricePerQuintalId = Guid.NewGuid(),
                            StartDate = grainPricePerQuintal.StartDate,
                            EndDate = grainPricePerQuintal.EndDate,
                            WoredaId = grainPricePerQuintal.WoredaId,
                            GrainTypeId = item.GrainTypeId,
                            WeeklyPrice = item.WeeklyPrice,
                            WeeklyMarketStatus = item.WeeklyMarketStatus,
                            Notes = grainPricePerQuintal.Notes,
                            ReportDate = grainPricePerQuintal.ReportDate,
                            ReportedBy = grainPricePerQuintal.ReportedBy,
                            ApprovalStatus = grainPricePerQuintal.ApprovalStatus
                        };

                        // Calculate price difference (get previous week's price)
                        var previousPrice = await _context.GrainPricePerQuintals
                            .Where(g => g.GrainTypeId == item.GrainTypeId
                                     && g.WoredaId == grainPricePerQuintal.WoredaId
                                     && g.EndDate < grainPricePerQuintal.StartDate)
                            .OrderByDescending(g => g.EndDate)
                            .FirstOrDefaultAsync();

                        if (previousPrice != null)
                        {
                            grainPrice.PriceDifference = grainPrice.WeeklyPrice - previousPrice.WeeklyPrice;
                            if (previousPrice.WeeklyPrice > 0)
                            {
                                grainPrice.PriceChangePercentage = (grainPrice.PriceDifference / previousPrice.WeeklyPrice) * 100;
                            }
                        }

                        _context.GrainPricePerQuintals.Add(grainPrice);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"{GrainPrices.Count} የእህል ዋጋ ሪፖርቶች በሚገባ ተመዝግበዋል";
                    // 🆕 REDIRECT TO EPIDEMIC DISEASE CREATE WITH DATA
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = grainPricePerQuintal.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = grainPricePerQuintal.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = grainPricePerQuintal.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "AnimalSupplies", new
                    {
                        startDate = grainPricePerQuintal.StartDate.ToString("yyyy-MM-dd"),
                        endDate = grainPricePerQuintal.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = grainPricePerQuintal.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(grainPricePerQuintal);
        }

        // GET: GrainPricePerQuintals/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPricePerQuintal = await _context.GrainPricePerQuintals.FindAsync(id);
            if (grainPricePerQuintal == null)
            {
                return NotFound();
            }
            ViewData["GrainTypeId"] = new SelectList(_context.GrainTypes, "GrainTypeId", "GrainName", grainPricePerQuintal.GrainTypeId);
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", grainPricePerQuintal.WoredaId);
            return View(grainPricePerQuintal);
        }

        // POST: GrainPricePerQuintals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainPricePerQuintalId,StartDate,EndDate,GrainTypeId,WeeklyPrice,WeeklyMarketStatus,PriceDifference,PriceChangePercentage,WoredaId,Notes,ReportDate,ReportedBy,ApprovalStatus")] GrainPricePerQuintal grainPricePerQuintal)
        {
            if (id != grainPricePerQuintal.GrainPricePerQuintalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grainPricePerQuintal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrainPricePerQuintalExists(grainPricePerQuintal.GrainPricePerQuintalId))
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
            ViewData["GrainTypeId"] = new SelectList(_context.GrainTypes, "GrainTypeId", "GrainName", grainPricePerQuintal.GrainTypeId);
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", grainPricePerQuintal.WoredaId);
            return View(grainPricePerQuintal);
        }

        // GET: GrainPricePerQuintals/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPricePerQuintal = await _context.GrainPricePerQuintals
                .Include(g => g.GrainType)
                .Include(g => g.Woreda)
                .FirstOrDefaultAsync(m => m.GrainPricePerQuintalId == id);
            if (grainPricePerQuintal == null)
            {
                return NotFound();
            }

            return View(grainPricePerQuintal);
        }

        // POST: GrainPricePerQuintals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var grainPricePerQuintal = await _context.GrainPricePerQuintals.FindAsync(id);
            if (grainPricePerQuintal != null)
            {
                _context.GrainPricePerQuintals.Remove(grainPricePerQuintal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrainPricePerQuintalExists(Guid id)
        {
            return _context.GrainPricePerQuintals.Any(e => e.GrainPricePerQuintalId == id);
        }
    }
}
