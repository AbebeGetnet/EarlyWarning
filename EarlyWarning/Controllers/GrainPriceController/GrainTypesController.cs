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
    public class GrainTypesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public GrainTypesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: GrainTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.GrainTypes.ToListAsync());
        }

        // GET: GrainTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainType = await _context.GrainTypes
                .FirstOrDefaultAsync(m => m.GrainTypeId == id);
            if (grainType == null)
            {
                return NotFound();
            }

            return View(grainType);
        }

        // GET: GrainTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GrainTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GrainTypeId,GrainName,IsActive")] GrainType grainType)
        {
            if (ModelState.IsValid)
            {
                grainType.GrainTypeId = Guid.NewGuid();
                _context.Add(grainType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grainType);
        }

        // GET: GrainTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainType = await _context.GrainTypes.FindAsync(id);
            if (grainType == null)
            {
                return NotFound();
            }
            return View(grainType);
        }

        // POST: GrainTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("GrainTypeId,GrainName,IsActive")] GrainType grainType)
        {
            if (id != grainType.GrainTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grainType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrainTypeExists(grainType.GrainTypeId))
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
            return View(grainType);
        }

        // GET: GrainTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grainType = await _context.GrainTypes
                .FirstOrDefaultAsync(m => m.GrainTypeId == id);
            if (grainType == null)
            {
                return NotFound();
            }

            return View(grainType);
        }

        // POST: GrainTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var grainType = await _context.GrainTypes.FindAsync(id);
            if (grainType != null)
            {
                _context.GrainTypes.Remove(grainType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrainTypeExists(Guid id)
        {
            return _context.GrainTypes.Any(e => e.GrainTypeId == id);
        }
    }
}
