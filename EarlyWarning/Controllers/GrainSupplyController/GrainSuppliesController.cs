using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.SupplyofGrain;
using EarlyWarning.Enums;
using EarlyWarning.Models.EpidemicDisease;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using System.Security.Claims;

namespace EarlyWarning.Controllers.GrainSupplyController
{
    public class GrainSuppliesController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GrainSuppliesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GrainSupplies
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.GrainSupplys
                .Include(g=>g.GrainSupplyDetails)
                    .ThenInclude(d => d.GrainIncreaseDecrease)
                .Include(g => g.Woreda);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: GrainSupplies/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainSupply = await _context.GrainSupplys
                .Include(g => g.Woreda)
                .Include(g => g.GrainSupplyDetails)
                    .ThenInclude(d => d.GrainIncreaseDecrease)
                .FirstOrDefaultAsync(m => m.GrainSupplyId == id);
            if (grainSupply == null)
            {
                return NotFound();
            }

            return View(grainSupply);
        }
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: GrainSupplies/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var grainSupply = new GrainSupply();

            // Pre-populate from query string parameters (coming from redirect)
            if (startDate.HasValue)
                grainSupply.StartDate = startDate.Value;
            else
                grainSupply.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday); // Optional: default to start of week

            if (endDate.HasValue)
                grainSupply.EndDate = endDate.Value;
            else
                grainSupply.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                grainSupply.WoredaId = woredaId.Value;

            if (!string.IsNullOrEmpty(reportedBy))
                grainSupply.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                grainSupply.ReportedBy = currentUser?.Id;
            }

            grainSupply.ReportDate = DateTime.Now;
            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ )
                    .ToListAsync(),
                "Id",
                "LocationAmharicName"
            );


            // 🔴 VERY IMPORTANT — LOAD REASONS
            ViewBag.IncreaseReasons = await _context.GrainIncreaseDecreases
                .Where(x =>  x.ReasonType == ReasonType.Increase)
                .OrderBy(x => x.ReasonName)
                .ToListAsync();

            ViewBag.DecreaseReasons = await _context.GrainIncreaseDecreases
                .Where(x =>  x.ReasonType == ReasonType.Decrease)
                .OrderBy(x => x.ReasonName)
                .ToListAsync();

            // Store flag in ViewBag to show notification
            if (TempData["IsRedirectedFromWaterIssue"] != null)
                ViewBag.ShowContinuationMessage = true;

            return View(grainSupply);
        }

        // POST: GrainSupplies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( GrainSupply grainSupply, List<Guid> SelectedReasonIds)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(_context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToList(), "Id", "LocationAmharicName", grainSupply.WoredaId);

            // Load reasons for ViewBag
            ViewBag.IncreaseReasons = await _context.GrainIncreaseDecreases
                .Where(r => r.ReasonType == ReasonType.Increase)
                
                .ToListAsync();

            ViewBag.DecreaseReasons = await _context.GrainIncreaseDecreases
                .Where(r => r.ReasonType == ReasonType.Decrease )
                
                .ToListAsync();

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                grainSupply.ReportedBy = userId;
                grainSupply.WoredaId = currentUser.LocationId;
                // Check if reasons are selected when status is Increased or Decreased
                if ((grainSupply.GrainSupplyStatus == SupplyStatus.Increased ||
                     grainSupply.GrainSupplyStatus == SupplyStatus.Decreased) &&
                    (SelectedReasonIds == null || !SelectedReasonIds.Any()))
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ ምክንያት ይምረጡ");
                    return View(grainSupply);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (grainSupply.ReportDate == default)
                    {
                        grainSupply.ReportDate = DateTime.Now;
                    }

                    // Generate new ID
                    grainSupply.GrainSupplyId = Guid.NewGuid();

                    // Save the main report
                    _context.GrainSupplys.Add(grainSupply);
                    await _context.SaveChangesAsync();

                    // Save selected reasons
                    if (SelectedReasonIds != null && SelectedReasonIds.Any())
                    {
                        foreach (var reasonId in SelectedReasonIds)
                        {
                            var detail = new GrainSupplyDetail
                            {
                                GrainSupplyDetailId = Guid.NewGuid(),
                                GrainSupplyId = grainSupply.GrainSupplyId,
                                GrainIncreaseDecreaseId = reasonId
                            };
                            _context.GrainSupplyDetails.Add(detail);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    // ✅ REDIRECT TO GRAIN PRICE PER QUINTAL WITH PRESERVED DATA
                    TempData["Success"] = "የእህል አቅርቦት ሪፖርት በሚገባ ተመዝግቧል";
                    // Check if GrainPricePerQuintals controller exists
                    return RedirectToAction("Create", "GrainPricePerQuintals", new
                    {
                        startDate = grainSupply.StartDate.ToString("yyyy-MM-dd"),
                        endDate = grainSupply.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = grainSupply.WoredaId,
                        reportedBy = grainSupply.ReportedBy
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(grainSupply);
        }

        // GET: GrainSupplies/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainSupply = await _context.GrainSupplys.FindAsync(id);
            if (grainSupply == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", grainSupply.WoredaId);
            return View(grainSupply);
        }

        // POST: GrainSupplies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainSupplyId,StartDate,EndDate,GrainSupplyStatus,WoredaId,AdditionalNotes,ReportDate,ReportedBy,ApprovalStatus")] GrainSupply grainSupply)
        {
            if (id != grainSupply.GrainSupplyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grainSupply);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrainSupplyExists(grainSupply.GrainSupplyId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", grainSupply.WoredaId);
            return View(grainSupply);
        }

        // GET: GrainSupplies/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainSupply = await _context.GrainSupplys
                .Include(g => g.Woreda)
                .FirstOrDefaultAsync(m => m.GrainSupplyId == id);
            if (grainSupply == null)
            {
                return NotFound();
            }

            return View(grainSupply);
        }

        // POST: GrainSupplies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var grainSupply = await _context.GrainSupplys.FindAsync(id);
            if (grainSupply != null)
            {
                _context.GrainSupplys.Remove(grainSupply);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrainSupplyExists(Guid id)
        {
            return _context.GrainSupplys.Any(e => e.GrainSupplyId == id);
        }
    }
}
