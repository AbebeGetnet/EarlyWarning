using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.Aid;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using EarlyWarning.Models;
using EarlyWarning.Enums;

namespace EarlyWarning.Controllers.AidController
{
    public class AssistanceRecipientsController : Controller
    {
        private readonly EarlyWarningDbContext _context; 
        private readonly UserManager<ApplicationUser> _userManager;

        public AssistanceRecipientsController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AssistanceRecipients
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.AssistanceRecipients.Include(a => a.Woreda);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: AssistanceRecipients/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assistanceRecipient = await _context.AssistanceRecipients
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.AssistanceRecipientId == id);
            if (assistanceRecipient == null)
            {
                return NotFound();
            }

            return View(assistanceRecipient);
        }
        // Helper method to get Monday of current week
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: AssistanceRecipients/Create
        public async Task<IActionResult> Create(DateTime? startDate, DateTime? endDate, Guid? woredaId, string? reportedBy)
        {
            var assistanceRecipient = new AssistanceRecipient();

            // Pre-populate from query string parameters (coming from WeeklyProvideds redirect)
            if (startDate.HasValue)
                assistanceRecipient.StartDate = startDate.Value;
            else
                assistanceRecipient.StartDate = GetStartOfWeek(DateTime.Now, DayOfWeek.Monday);

            if (endDate.HasValue)
                assistanceRecipient.EndDate = endDate.Value;
            else
                assistanceRecipient.EndDate = DateTime.Now;

            if (woredaId.HasValue)
                assistanceRecipient.WoredaId = woredaId.Value;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                assistanceRecipient.WoredaId = currentUser?.LocationId ?? Guid.Empty;
            }

            if (!string.IsNullOrEmpty(reportedBy))
                assistanceRecipient.ReportedBy = reportedBy;
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                assistanceRecipient.ReportedBy = currentUser?.Id;
            }

            assistanceRecipient.ReportDate = DateTime.Now;
            assistanceRecipient.ApprovalStatus = ApprovalStatus.በሂደት_ላይ;

            // Woreda dropdown
            ViewData["WoredaId"] = new SelectList(
                await _context.Locations
                    .Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive)
                    .ToListAsync(),
                "Id",
                "LocationAmharicName",
                assistanceRecipient.WoredaId
            );

            // Get all kebeles for assistance details
            ViewBag.Kebeles = await _context.Locations
                .Where(x => x.Level == LocationLevel.ቀበሌ && x.IsActive)
                .OrderBy(x => x.LocationAmharicName)
                .ToListAsync();

            // Show success message if coming from previous report
            if (TempData["Success"] != null)
                ViewBag.PreviousSuccess = TempData["Success"];

            // Get woreda name for display
            var woreda = await _context.Locations.FindAsync(assistanceRecipient.WoredaId);
            ViewBag.WoredaName = woreda?.LocationAmharicName ?? "ያልተመረጠ";

            return View(assistanceRecipient);
        }

        // POST: AssistanceRecipients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( AssistanceRecipient assistanceRecipient)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                assistanceRecipient.ReportedBy = userId;
                assistanceRecipient.WoredaId = currentUser.LocationId;
                assistanceRecipient.AssistanceRecipientId = Guid.NewGuid();
                _context.Add(assistanceRecipient);
                await _context.SaveChangesAsync();
                // Store data in TempData to pass to next action
                TempData["PreviousStartDate"] = assistanceRecipient.StartDate.ToString("yyyy-MM-dd");
                TempData["PreviousEndDate"] = assistanceRecipient.EndDate.ToString("yyyy-MM-dd");
                TempData["PreviousWoredaId"] = assistanceRecipient.WoredaId.ToString();
                TempData["PreviousReportedBy"] = userId;
                TempData["IsRedirectedFromWaterIssue"] = true;

                // Redirect to EpidemicDisease Create action
                return RedirectToAction("Create", "WeeklyProvideds", new
                {
                    startDate = assistanceRecipient.StartDate.ToString("yyyy-MM-dd"),
                    endDate = assistanceRecipient.EndDate.ToString("yyyy-MM-dd"),
                    woredaId = assistanceRecipient.WoredaId,
                    reportedBy = userId
                });
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", assistanceRecipient.WoredaId);
            return View(assistanceRecipient);
        }

        // GET: AssistanceRecipients/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assistanceRecipient = await _context.AssistanceRecipients.FindAsync(id);
            if (assistanceRecipient == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", assistanceRecipient.WoredaId);
            return View(assistanceRecipient);
        }

        // POST: AssistanceRecipients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AssistanceRecipientId,StartDate,EndDate,WoredaId,MaleHouseholdHeads,FemaleHouseholdHeads,MaleFamilyMembers,FemaleFamilyMembers,MaleChildren,FemaleChildren,MaleYouth,FemaleYouth,MaleElderly,FemaleElderly,MaleDisabled,FemaleDisabled,Notes,ReportDate,ReportedBy,ApprovalStatus")] AssistanceRecipient assistanceRecipient)
        {
            if (id != assistanceRecipient.AssistanceRecipientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assistanceRecipient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssistanceRecipientExists(assistanceRecipient.AssistanceRecipientId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", assistanceRecipient.WoredaId);
            return View(assistanceRecipient);
        }

        // GET: AssistanceRecipients/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assistanceRecipient = await _context.AssistanceRecipients
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.AssistanceRecipientId == id);
            if (assistanceRecipient == null)
            {
                return NotFound();
            }

            return View(assistanceRecipient);
        }

        // POST: AssistanceRecipients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var assistanceRecipient = await _context.AssistanceRecipients.FindAsync(id);
            if (assistanceRecipient != null)
            {
                _context.AssistanceRecipients.Remove(assistanceRecipient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssistanceRecipientExists(Guid id)
        {
            return _context.AssistanceRecipients.Any(e => e.AssistanceRecipientId == id);
        }
    }
}
