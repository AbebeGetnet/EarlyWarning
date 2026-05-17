using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.SupplyofGrain;

namespace EarlyWarning.Controllers.GrainSupplyController
{
    public class GrainIncreaseDecreasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public GrainIncreaseDecreasesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: GrainIncreaseDecreases
        public async Task<IActionResult> Index()
        {
            return View(await _context.GrainIncreaseDecreases.ToListAsync());
        }

        // GET: GrainIncreaseDecreases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainIncreaseDecrease = await _context.GrainIncreaseDecreases
                .FirstOrDefaultAsync(m => m.GrainIncreaseDecreaseId == id);
            if (grainIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(grainIncreaseDecrease);
        }

        // GET: GrainIncreaseDecreases/Create
        public IActionResult Create()
        {
            return View(new GrainIncreaseDecrease());
        }

        // POST: GrainIncreaseDecreases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GrainIncreaseDecreaseId,ReasonName,ReasonType")] GrainIncreaseDecrease grainIncreaseDecrease)
        {
            if (ModelState.IsValid)
            {
                grainIncreaseDecrease.GrainIncreaseDecreaseId = Guid.NewGuid();
                _context.Add(grainIncreaseDecrease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grainIncreaseDecrease);
        }

        // GET: GrainIncreaseDecreases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainIncreaseDecrease = await _context.GrainIncreaseDecreases.FindAsync(id);
            if (grainIncreaseDecrease == null)
            {
                return NotFound();
            }
            return View(grainIncreaseDecrease);
        }

        // POST: GrainIncreaseDecreases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainIncreaseDecreaseId,ReasonName,ReasonType")] GrainIncreaseDecrease grainIncreaseDecrease)
        {
            if (id != grainIncreaseDecrease.GrainIncreaseDecreaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grainIncreaseDecrease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrainIncreaseDecreaseExists(grainIncreaseDecrease.GrainIncreaseDecreaseId))
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
            return View(grainIncreaseDecrease);
        }

        // GET: GrainIncreaseDecreases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainIncreaseDecrease = await _context.GrainIncreaseDecreases
                .FirstOrDefaultAsync(m => m.GrainIncreaseDecreaseId == id);
            if (grainIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(grainIncreaseDecrease);
        }

        // POST: GrainIncreaseDecreases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var grainIncreaseDecrease = await _context.GrainIncreaseDecreases.FindAsync(id);
            if (grainIncreaseDecrease != null)
            {
                _context.GrainIncreaseDecreases.Remove(grainIncreaseDecrease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrainIncreaseDecreaseExists(Guid id)
        {
            return _context.GrainIncreaseDecreases.Any(e => e.GrainIncreaseDecreaseId == id);
        }
    }
}
