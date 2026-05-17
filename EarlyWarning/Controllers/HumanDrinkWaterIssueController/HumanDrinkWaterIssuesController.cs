using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using Microsoft.AspNetCore.Identity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Claims;

namespace EarlyWarning.Controllers.HumanDrinkWaterIssueController
{
    public class HumanDrinkWaterIssuesController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HumanDrinkWaterIssuesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: HumanDrinkWaterIssues
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Super Administrator"))
            {
                var earlyWarningDbContext = _context.HumanDrinkWaterIssues.Include(h => h.Woreda).Where(x=>x.Woreda.Parent.ParentId== currentUser.LocationId);
                return View(await earlyWarningDbContext.ToListAsync());
            }
            else if (User.IsInRole("Super Administrator"))
            {
                var earlyWarningDbContext = _context.HumanDrinkWaterIssues.Include(h => h.Woreda).Where(x => x.Woreda.ParentId == currentUser.LocationId);
                return View(await earlyWarningDbContext.ToListAsync());
            }
            else
            {
                // 🆕 Get user names for each report

                var query = _context.HumanDrinkWaterIssues
                .Include(h => h.Woreda)
                .AsQueryable();
                var reports = await query.OrderByDescending(h => h.ReportDate).ToListAsync();
                var userIds = reports.Where(r => !string.IsNullOrEmpty(r.ReportedBy)).Select(r => r.ReportedBy).Distinct().ToList();
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FirstName +" "+ u.LastName);

                ViewBag.UserNames = users;
                var earlyWarningDbContext = _context.HumanDrinkWaterIssues.Include(h => h.Woreda).Where(x => x.WoredaId == currentUser.LocationId);
                return View(await earlyWarningDbContext.ToListAsync());
            }


        }

        // GET: HumanDrinkWaterIssues/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var humanDrinkWaterIssue = await _context.HumanDrinkWaterIssues
                .Include(h => h.Woreda)
                .Include(x => x.HumanDrinkWaterKebeles)
                        .ThenInclude(x => x.Kebele)
                .FirstOrDefaultAsync(m => m.HumanDrinkWaterIssueId == id);
            if (humanDrinkWaterIssue == null)
            {
                return NotFound();
            }

            return View(humanDrinkWaterIssue);
        }
        // Helper method for getting start of week
        private DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        // GET: HumanDrinkWaterIssues/Create
        public IActionResult Create()
        {
            var today = DateTime.Today;
            var currentMonday = GetStartOfWeek(today, DayOfWeek.Monday);
            var currentSunday = currentMonday.AddDays(6);

            var model = new HumanDrinkWaterIssue
            {
                StartDate = currentMonday,
                EndDate = currentSunday,
                ReportDate = DateTime.Now
            };
            ViewBag.WoredaId = new SelectList(_context.Locations
                    .Where(l => l.Level == LocationLevel.ወረዳ ), "Id", "LocationName");
            ViewBag.Kebeles = new SelectList(_context.Locations
                                .Where(l => l.Level == LocationLevel.ቀበሌ),
                                "Id", "LocationName"); 
            return View();
        }

        // POST: HumanDrinkWaterIssues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HumanDrinkWaterIssue humanDrinkWaterIssue)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                // ✔ 2. Generate PK
                humanDrinkWaterIssue.HumanDrinkWaterIssueId = Guid.NewGuid();
                // 🆕 Get current user ID and name
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                humanDrinkWaterIssue.ReportedBy = userId;
                humanDrinkWaterIssue.WoredaId = currentUser.LocationId;
                humanDrinkWaterIssue.ReportDate = DateTime.Now;
                // ✔ 3. Auto-calculate number of affected kebeles
                humanDrinkWaterIssue.NumberOfAffectedKebeles = humanDrinkWaterIssue.AffectedKebeleIds?.Count ?? 0;

                // ✔ 4. Save main table first
                _context.HumanDrinkWaterIssues.Add(humanDrinkWaterIssue);
                await _context.SaveChangesAsync();


                // ✔ 5. Insert into junction table (MOST IMPORTANT PART)
                if (humanDrinkWaterIssue.AffectedKebeleIds != null && humanDrinkWaterIssue.AffectedKebeleIds.Any())
                {
                    var kebeleRelations = humanDrinkWaterIssue.AffectedKebeleIds.Select(kebeleId =>
                        new HumanDrinkWaterKebele
                        {
                            HumanDrinkWaterKebeleId = Guid.NewGuid(),
                            HumanDrinkWaterIssueId = humanDrinkWaterIssue.HumanDrinkWaterIssueId,
                            KebeleId = kebeleId
                        }).ToList();

                    _context.HumanDrinkWaterKebeles.AddRange(kebeleRelations);
                    await _context.SaveChangesAsync();
                }
                // 🆕 REDIRECT TO EPIDEMIC DISEASE CREATE WITH DATA
                // Store data in TempData to pass to next action
                TempData["PreviousStartDate"] = humanDrinkWaterIssue.StartDate.ToString("yyyy-MM-dd");
                TempData["PreviousEndDate"] = humanDrinkWaterIssue.EndDate.ToString("yyyy-MM-dd");
                TempData["PreviousWoredaId"] = humanDrinkWaterIssue.WoredaId.ToString();
                TempData["PreviousReportedBy"] = userId;
                TempData["IsRedirectedFromWaterIssue"] = true;

                // Redirect to EpidemicDisease Create action
                return RedirectToAction("Create", "EpidemicDiseases", new
                {
                    startDate = humanDrinkWaterIssue.StartDate.ToString("yyyy-MM-dd"),
                    endDate = humanDrinkWaterIssue.EndDate.ToString("yyyy-MM-dd"),
                    woredaId = humanDrinkWaterIssue.WoredaId,
                    reportedBy = userId
                });
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", humanDrinkWaterIssue.WoredaId);
            return View(humanDrinkWaterIssue);
        }

        // GET: HumanDrinkWaterIssues/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var humanDrinkWaterIssue = await _context.HumanDrinkWaterIssues.FindAsync(id);
            if (humanDrinkWaterIssue == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", humanDrinkWaterIssue.WoredaId);
            return View(humanDrinkWaterIssue);
        }

        // POST: HumanDrinkWaterIssues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("HumanDrinkWaterIssueId,StartDate,EndDate,HasWaterProblem,NumberOfAffectedKebeles,MaleHouseholdHeads,FemaleHouseholdHeads,MaleFamilyMembers,FemaleFamilyMembers,MaleChildren,FemaleChildren,MaleYouth,FemaleYouth,MaleElderly,FemaleElderly,MaleDisabled,FemaleDisabled,ReportDate,ReportedBy,WoredaId")] HumanDrinkWaterIssue humanDrinkWaterIssue)
        {
            if (id != humanDrinkWaterIssue.HumanDrinkWaterIssueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(humanDrinkWaterIssue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HumanDrinkWaterIssueExists(humanDrinkWaterIssue.HumanDrinkWaterIssueId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", humanDrinkWaterIssue.WoredaId);
            return View(humanDrinkWaterIssue);
        }

        // GET: HumanDrinkWaterIssues/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var humanDrinkWaterIssue = await _context.HumanDrinkWaterIssues
                .Include(h => h.Woreda)
                .FirstOrDefaultAsync(m => m.HumanDrinkWaterIssueId == id);
            if (humanDrinkWaterIssue == null)
            {
                return NotFound();
            }

            return View(humanDrinkWaterIssue);
        }

        // POST: HumanDrinkWaterIssues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var humanDrinkWaterIssue = await _context.HumanDrinkWaterIssues.FindAsync(id);
            if (humanDrinkWaterIssue != null)
            {
                _context.HumanDrinkWaterIssues.Remove(humanDrinkWaterIssue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HumanDrinkWaterIssueExists(Guid id)
        {
            return _context.HumanDrinkWaterIssues.Any(e => e.HumanDrinkWaterIssueId == id);
        }
    }
}
