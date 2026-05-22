using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.PriceofGrain;

namespace EarlyWarning.Controllers.GrainPriceController
{
    public class GrainPriceIncreaseDecreasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public GrainPriceIncreaseDecreasesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: GrainPriceIncreaseDecreases
        public async Task<IActionResult> Index()
        {
            return View(await _context.GrainPriceIncreaseDecreases.ToListAsync());
        }

        // GET: GrainPriceIncreaseDecreases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPriceIncreaseDecrease = await _context.GrainPriceIncreaseDecreases
                .FirstOrDefaultAsync(m => m.GrainIncreaseDecreaseId == id);
            if (grainPriceIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(grainPriceIncreaseDecrease);
        }

        // GET: GrainPriceIncreaseDecreases/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GrainPriceIncreaseDecreases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GrainIncreaseDecreaseId,ReasonName,ReasonType")] GrainPriceIncreaseDecrease grainPriceIncreaseDecrease)
        {
            if (ModelState.IsValid)
            {
                grainPriceIncreaseDecrease.GrainIncreaseDecreaseId = Guid.NewGuid();
                _context.Add(grainPriceIncreaseDecrease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grainPriceIncreaseDecrease);
        }

        // GET: GrainPriceIncreaseDecreases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPriceIncreaseDecrease = await _context.GrainPriceIncreaseDecreases.FindAsync(id);
            if (grainPriceIncreaseDecrease == null)
            {
                return NotFound();
            }
            return View(grainPriceIncreaseDecrease);
        }

        // POST: GrainPriceIncreaseDecreases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainIncreaseDecreaseId,ReasonName,ReasonType")] GrainPriceIncreaseDecrease grainPriceIncreaseDecrease)
        {
            if (id != grainPriceIncreaseDecrease.GrainIncreaseDecreaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grainPriceIncreaseDecrease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrainPriceIncreaseDecreaseExists(grainPriceIncreaseDecrease.GrainIncreaseDecreaseId))
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
            return View(grainPriceIncreaseDecrease);
        }

        // GET: GrainPriceIncreaseDecreases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainPriceIncreaseDecrease = await _context.GrainPriceIncreaseDecreases
                .FirstOrDefaultAsync(m => m.GrainIncreaseDecreaseId == id);
            if (grainPriceIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(grainPriceIncreaseDecrease);
        }

        // POST: GrainPriceIncreaseDecreases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var grainPriceIncreaseDecrease = await _context.GrainPriceIncreaseDecreases.FindAsync(id);
            if (grainPriceIncreaseDecrease != null)
            {
                _context.GrainPriceIncreaseDecreases.Remove(grainPriceIncreaseDecrease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrainPriceIncreaseDecreaseExists(Guid id)
        {
            return _context.GrainPriceIncreaseDecreases.Any(e => e.GrainIncreaseDecreaseId == id);
        }
    }
}
