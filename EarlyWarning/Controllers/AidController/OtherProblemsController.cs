using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.Aid;

namespace EarlyWarning.Controllers.AidController
{
    public class OtherProblemsController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public OtherProblemsController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: OtherProblems
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.OtherProblems.Include(o => o.Woreda);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: OtherProblems/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var otherProblem = await _context.OtherProblems
                .Include(o => o.Woreda)
                .FirstOrDefaultAsync(m => m.OtherProblemId == id);
            if (otherProblem == null)
            {
                return NotFound();
            }

            return View(otherProblem);
        }

        // GET: OtherProblems/Create
        public IActionResult Create()
        {
            ViewData["WoredaId"] = new SelectList(_context.Locations
                .Where(x=>x.Level== Enums.LocationLevel.ወረዳ), "Id", "LocationAmharicName");
            return View();
        }

        // POST: OtherProblems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OtherProblemId,StartDate,EndDate,HasOtherProblem,ProblemName,WoredaId,MaleHouseholdHeads,FemaleHouseholdHeads,MaleFamilyMembers,FemaleFamilyMembers,MaleChildren,FemaleChildren,MaleYouth,FemaleYouth,MaleElderly,FemaleElderly,MaleDisabled,FemaleDisabled,GeneralNotes,ReportDate,ReportedBy,ApprovalStatus")] OtherProblem otherProblem)
        {
            if (ModelState.IsValid)
            {
                otherProblem.OtherProblemId = Guid.NewGuid();
                _context.Add(otherProblem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", otherProblem.WoredaId);
            return View(otherProblem);
        }

        // GET: OtherProblems/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var otherProblem = await _context.OtherProblems.FindAsync(id);
            if (otherProblem == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", otherProblem.WoredaId);
            return View(otherProblem);
        }

        // POST: OtherProblems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("OtherProblemId,StartDate,EndDate,HasOtherProblem,ProblemName,WoredaId,MaleHouseholdHeads,FemaleHouseholdHeads,MaleFamilyMembers,FemaleFamilyMembers,MaleChildren,FemaleChildren,MaleYouth,FemaleYouth,MaleElderly,FemaleElderly,MaleDisabled,FemaleDisabled,GeneralNotes,ReportDate,ReportedBy,ApprovalStatus")] OtherProblem otherProblem)
        {
            if (id != otherProblem.OtherProblemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(otherProblem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OtherProblemExists(otherProblem.OtherProblemId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", otherProblem.WoredaId);
            return View(otherProblem);
        }

        // GET: OtherProblems/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var otherProblem = await _context.OtherProblems
                .Include(o => o.Woreda)
                .FirstOrDefaultAsync(m => m.OtherProblemId == id);
            if (otherProblem == null)
            {
                return NotFound();
            }

            return View(otherProblem);
        }

        // POST: OtherProblems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var otherProblem = await _context.OtherProblems.FindAsync(id);
            if (otherProblem != null)
            {
                _context.OtherProblems.Remove(otherProblem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OtherProblemExists(Guid id)
        {
            return _context.OtherProblems.Any(e => e.OtherProblemId == id);
        }
    }
}
