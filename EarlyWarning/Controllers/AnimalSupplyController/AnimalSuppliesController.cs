using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.AnimalSupply;
using EarlyWarning.Enums;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using System.Security.Claims;

namespace EarlyWarning.Controllers.AnimalSupplyController
{
    public class AnimalSuppliesController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnimalSuppliesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AnimalSupplies
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.AnimalSupplys
                .Include(g => g.AnimalSupplyDetails)
                    .ThenInclude(d => d.AnimalIncreaseDecrease)
                .Include(a => a.Woreda);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: AnimalSupplies/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalSupply = await _context.AnimalSupplys
                .Include(a => a.Woreda)
                .Include(g => g.AnimalSupplyDetails)
                    .ThenInclude(d => d.AnimalIncreaseDecrease)
                .FirstOrDefaultAsync(m => m.GrainSupplyId == id);
            if (animalSupply == null)
            {
                return NotFound();
            }

            return View(animalSupply);
        }
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: AnimalSupplies/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var animalSupply = new AnimalSupply();

            // Pre-populate from query string parameters (coming from GrainPricePerQuintal redirect)
            if (startDate.HasValue)
                animalSupply.StartDate = startDate.Value;
            else
                animalSupply.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                animalSupply.EndDate = endDate.Value;
            else
                animalSupply.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                animalSupply.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                animalSupply.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                animalSupply.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                animalSupply.ReportedBy = currentUser?.Id;
            }

            animalSupply.ReportDate = DateTime.Now;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                animalSupply.WoredaId
            );

            // Load increase and decrease reasons
            ViewBag.IncreaseReasons = await _context.AnimalIncreaseDecreases
                .Where(x => x.ReasonType == ReasonType.Increase )
                .OrderBy(x => x.ReasonName)
                .ToListAsync();

            ViewBag.DecreaseReasons = await _context.AnimalIncreaseDecreases
                .Where(x => x.ReasonType == ReasonType.Decrease)
                .OrderBy(x => x.ReasonName)
                .ToListAsync();

            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            return View(animalSupply);
        }

        // POST: AnimalSupplies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( AnimalSupply animalSupply, List<Guid> SelectedReasonIds)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload ViewBag data in case of error
            ViewData["WoredaId"] = new SelectList(_context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                .ToList(), "LocationId", "LocationAmharicName", animalSupply.WoredaId);

            // Load reasons for ViewBag
            ViewBag.IncreaseReasons = await _context.AnimalIncreaseDecreases
                .Where(r => r.ReasonType == ReasonType.Increase)

                .ToListAsync();

            ViewBag.DecreaseReasons = await _context.AnimalIncreaseDecreases
                .Where(r => r.ReasonType == ReasonType.Decrease)

                .ToListAsync();

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                animalSupply.ReportedBy = userId;
                animalSupply.WoredaId = currentUser.LocationId;
                // Check if reasons are selected when status is Increased or Decreased
                if ((animalSupply.GrainSupplyStatus == SupplyStatus.Increased ||
                     animalSupply.GrainSupplyStatus == SupplyStatus.Decreased) &&
                    (SelectedReasonIds == null || !SelectedReasonIds.Any()))
                {
                    ModelState.AddModelError("", "እባክዎ ቢያንስ አንድ ምክንያት ይምረጡ");
                    return View(animalSupply);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set report date if not set
                    if (animalSupply.ReportDate == default)
                    {
                        animalSupply.ReportDate = DateTime.Now;
                    }

                    // Generate new ID
                    animalSupply.GrainSupplyId = Guid.NewGuid();

                    // Save the main report
                    _context.AnimalSupplys.Add(animalSupply);
                    await _context.SaveChangesAsync();

                    // Save selected reasons
                    if (SelectedReasonIds != null && SelectedReasonIds.Any())
                    {
                        foreach (var reasonId in SelectedReasonIds)
                        {
                            var detail = new AnimalSupplyDetail
                            {
                                AnimalSupplyDetailId = Guid.NewGuid(),
                                AnimalSupplyId = animalSupply.GrainSupplyId,
                                AnimalIncreaseDecreaseId = reasonId
                            };
                            _context.AnimalSupplyDetails.Add(detail);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Success"] = "የእህል አቅርቦት ሪፖርት በሚገባ ተመዝግቧል";
                    // Store data in TempData to pass to next action
                    TempData["PreviousStartDate"] = animalSupply.StartDate.ToString("yyyy-MM-dd");
                    TempData["PreviousEndDate"] = animalSupply.EndDate.ToString("yyyy-MM-dd");
                    TempData["PreviousWoredaId"] = animalSupply.WoredaId.ToString();
                    TempData["PreviousReportedBy"] = userId;
                    TempData["IsRedirectedFromWaterIssue"] = true;

                    // Redirect to EpidemicDisease Create action
                    return RedirectToAction("Create", "AnimalPricePerUnits", new
                    {
                        startDate = animalSupply.StartDate.ToString("yyyy-MM-dd"),
                        endDate = animalSupply.EndDate.ToString("yyyy-MM-dd"),
                        woredaId = animalSupply.WoredaId,
                        reportedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message);
                }
            }

            return View(animalSupply);
        }

        // GET: AnimalSupplies/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalSupply = await _context.AnimalSupplys.FindAsync(id);
            if (animalSupply == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations.Where(x => x.Level == LocationLevel.ወረዳ), "Id", "LocationAmharicName", animalSupply.WoredaId);
            return View(animalSupply);
        }

        // POST: AnimalSupplies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainSupplyId,StartDate,EndDate,GrainSupplyStatus,WoredaId,AdditionalNotes,ReportDate,ReportedBy,ApprovalStatus")] AnimalSupply animalSupply)
        {
            if (id != animalSupply.GrainSupplyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalSupply);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalSupplyExists(animalSupply.GrainSupplyId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", animalSupply.WoredaId);
            return View(animalSupply);
        }

        // GET: AnimalSupplies/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalSupply = await _context.AnimalSupplys
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.GrainSupplyId == id);
            if (animalSupply == null)
            {
                return NotFound();
            }

            return View(animalSupply);
        }

        // POST: AnimalSupplies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalSupply = await _context.AnimalSupplys.FindAsync(id);
            if (animalSupply != null)
            {
                _context.AnimalSupplys.Remove(animalSupply);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalSupplyExists(Guid id)
        {
            return _context.AnimalSupplys.Any(e => e.GrainSupplyId == id);
        }
    }
}
