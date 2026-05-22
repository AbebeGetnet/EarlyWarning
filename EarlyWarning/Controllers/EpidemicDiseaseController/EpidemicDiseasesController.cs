using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.EpidemicDisease;
using EarlyWarning.Enums;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using EarlyWarning.Service;
using System.Security.Claims;
using EarlyWarning.Models.HumanDrinkWaterIssue;

namespace EarlyWarning.Controllers.EpidemicDiseaseController
{
    public class EpidemicDiseasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportFlowService _reportFlowService;
        public EpidemicDiseasesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager,
        IReportFlowService reportFlowService)
        {
            _context = context;
            _userManager = userManager;
            _reportFlowService = reportFlowService;
        }

        // GET: EpidemicDiseases
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.EpidemicDiseases.Include(e => e.Woreda);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: EpidemicDiseases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var epidemic = await _context.EpidemicDiseases
                .Include(e => e.Woreda)
                .Include(e => e.EpidemicDiseaseSelections)
                    .ThenInclude(s => s.EpidemicDiseaseType)
                .FirstOrDefaultAsync(m => m.EpidemicDiseaseId == id);

            if (epidemic == null) return NotFound();

            return View(epidemic);
        }
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: EpidemicDiseases/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var epidemicDisease = new EpidemicDisease();

            // Pre-populate from query string parameters (coming from redirect)
            if (startDate.HasValue)
                epidemicDisease.StartDate = startDate.Value;
            else
                epidemicDisease.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday); // Optional: default to start of week

            if (endDate.HasValue)
                epidemicDisease.EndDate = endDate.Value;
            else
                epidemicDisease.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                epidemicDisease.WoredaId = woredaId.Value;

            if (!string.IsNullOrEmpty(reportedBy))
                epidemicDisease.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                epidemicDisease.ReportedBy = currentUser?.Id;
            }

            epidemicDisease.ReportDate = DateTime.Now;

            ViewData["WoredaId"] = new SelectList(
                _context.Locations.Where(x => x.Level == Enums.LocationLevel.ወረዳ),
                "Id",
                "LocationAmharicName",
                epidemicDisease.WoredaId
            );

            // Get Epidemic Disease Types for multiple selection
            var diseaseTypes = await _context.EpidemicDiseaseTypes
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.EpidemicDiseaseTypes = diseaseTypes;

            // Store flag in ViewBag to show notification
            if (TempData["IsRedirectedFromWaterIssue"] != null)
                ViewBag.ShowContinuationMessage = true;

            return View(epidemicDisease);
        }

        // POST: EpidemicDiseases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: EpidemicDiseases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EpidemicDisease epidemicDisease,
            List<Guid>? selectedDiseaseTypeIds)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Reload dropdowns when returning view
            ViewData["WoredaId"] = new SelectList(
                _context.Locations
                .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive),
                "Id", "LocationAmharicName", epidemicDisease.WoredaId);

            ViewBag.EpidemicDiseaseTypes = await _context.EpidemicDiseaseTypes
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(epidemicDisease);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // set defaults
                epidemicDisease.EpidemicDiseaseId = Guid.NewGuid();
                epidemicDisease.ReportDate = DateTime.Now;
                epidemicDisease.ReportedBy = userId;
                epidemicDisease.WoredaId = currentUser.LocationId;


                // ⭐ CREATE CHILD LIST BEFORE SAVE
                epidemicDisease.EpidemicDiseaseSelections = new List<EpidemicDiseaseSelection>();

                if (selectedDiseaseTypeIds != null && selectedDiseaseTypeIds.Any())
                {
                    foreach (var diseaseTypeId in selectedDiseaseTypeIds)
                    {
                        epidemicDisease.EpidemicDiseaseSelections.Add(
                            new EpidemicDiseaseSelection
                            {
                                EpidemicDiseaseSelectionId = Guid.NewGuid(),
                                EpidemicDiseaseTypeId = diseaseTypeId
                            });
                    }
                }

                // ⭐ SAVE EVERYTHING IN ONE SAVECHANGES
                _context.EpidemicDiseases.Add(epidemicDisease);
                await _context.SaveChangesAsync();


                // ✅ SAVE PENDING DATA FOR NEXT REPORT (GrainSupply)
                //_reportFlowService.SavePendingReport(
                //    "GrainSupply",
                //    epidemicDisease.StartDate,
                //    epidemicDisease.EndDate,
                //    epidemicDisease.WoredaId,
                //    epidemicDisease.ReportedBy
                //);

                // Store data in TempData to pass to next action
                TempData["PreviousStartDate"] = epidemicDisease.StartDate.ToString("yyyy-MM-dd");
                TempData["PreviousEndDate"] = epidemicDisease.EndDate.ToString("yyyy-MM-dd");
                TempData["PreviousWoredaId"] = epidemicDisease.WoredaId.ToString();
                TempData["PreviousReportedBy"] = userId;
                TempData["IsRedirectedFromWaterIssue"] = true;

                // Redirect to EpidemicDisease Create action
                return RedirectToAction("Create", "GrainSupplies", new
                {
                    startDate = epidemicDisease.StartDate.ToString("yyyy-MM-dd"),
                    endDate = epidemicDisease.EndDate.ToString("yyyy-MM-dd"),
                    woredaId = epidemicDisease.WoredaId,
                    reportedBy = userId
                });
                //TempData["Success"] = "የወረርሽኝ ሪፖርት በሚገባ ተመዝግቧል";

                //// Show continue prompt
                //TempData["ShowContinuePrompt"] = true;
                //TempData["NextReportType"] = "GrainSupply";
                //TempData["NextReportName"] = "የእህል አቅርቦት";

                //return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Save failed: " + ex.InnerException?.Message ?? ex.Message);
                return View(epidemicDisease);
            }
        }

        // GET: EpidemicDiseases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var epidemicDisease = await _context.EpidemicDiseases.FindAsync(id);
            if (epidemicDisease == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", epidemicDisease.WoredaId);
            return View(epidemicDisease);
        }

        // POST: EpidemicDiseases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("EpidemicDiseaseId,StartDate,EndDate,HasEpidemicDisease,NumberOfAffectedKebeles,MaleHouseholdHeads,FemaleHouseholdHeads,MaleFamilyMembers,FemaleFamilyMembers,MaleChildren,FemaleChildren,MaleYouth,FemaleYouth,MaleElderly,FemaleElderly,MaleDisabled,FemaleDisabled,LactatingMothers,PregnantWomen,WoredaId,ReportDate,ReportedBy,Notes")] EpidemicDisease epidemicDisease)
        {
            if (id != epidemicDisease.EpidemicDiseaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(epidemicDisease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EpidemicDiseaseExists(epidemicDisease.EpidemicDiseaseId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", epidemicDisease.WoredaId);
            return View(epidemicDisease);
        }

        // GET: EpidemicDiseases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var epidemicDisease = await _context.EpidemicDiseases
                .Include(e => e.Woreda)
                .FirstOrDefaultAsync(m => m.EpidemicDiseaseId == id);
            if (epidemicDisease == null)
            {
                return NotFound();
            }

            return View(epidemicDisease);
        }

        // POST: EpidemicDiseases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var epidemicDisease = await _context.EpidemicDiseases.FindAsync(id);
            if (epidemicDisease != null)
            {
                _context.EpidemicDiseases.Remove(epidemicDisease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EpidemicDiseaseExists(Guid id)
        {
            return _context.EpidemicDiseases.Any(e => e.EpidemicDiseaseId == id);
        }
    }
}
