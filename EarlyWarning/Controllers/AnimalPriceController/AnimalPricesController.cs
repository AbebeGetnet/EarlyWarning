using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.AnimalPrice;

namespace EarlyWarning.Controllers.AnimalPriceController
{
    public class AnimalPricesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalPricesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: AnimalPrices
        public async Task<IActionResult> Index()
        {
            var earlyWarningDbContext = _context.AnimalPrice.Include(a => a.Woreda);
            return View(await earlyWarningDbContext.ToListAsync());
        }

        // GET: AnimalPrices/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPrice = await _context.AnimalPrice
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.AnimalPriceId == id);
            if (animalPrice == null)
            {
                return NotFound();
            }

            return View(animalPrice);
        }

        // GET: AnimalPrices/Create
        public IActionResult Create()
        {
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName");
            return View();
        }

        // POST: AnimalPrices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalPriceId,StartDate,EndDate,GrainSupplyStatus,WoredaId,AdditionalNotes,ReportDate,ReportedBy,ApprovalStatus")] AnimalPrice animalPrice)
        {
            if (ModelState.IsValid)
            {
                animalPrice.AnimalPriceId = Guid.NewGuid();
                _context.Add(animalPrice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", animalPrice.WoredaId);
            return View(animalPrice);
        }

        // GET: AnimalPrices/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPrice = await _context.AnimalPrice.FindAsync(id);
            if (animalPrice == null)
            {
                return NotFound();
            }
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", animalPrice.WoredaId);
            return View(animalPrice);
        }

        // POST: AnimalPrices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AnimalPriceId,StartDate,EndDate,GrainSupplyStatus,WoredaId,AdditionalNotes,ReportDate,ReportedBy,ApprovalStatus")] AnimalPrice animalPrice)
        {
            if (id != animalPrice.AnimalPriceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalPrice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalPriceExists(animalPrice.AnimalPriceId))
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
            ViewData["WoredaId"] = new SelectList(_context.Locations, "Id", "LocationAmharicName", animalPrice.WoredaId);
            return View(animalPrice);
        }

        // GET: AnimalPrices/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPrice = await _context.AnimalPrice
                .Include(a => a.Woreda)
                .FirstOrDefaultAsync(m => m.AnimalPriceId == id);
            if (animalPrice == null)
            {
                return NotFound();
            }

            return View(animalPrice);
        }

        // POST: AnimalPrices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalPrice = await _context.AnimalPrice.FindAsync(id);
            if (animalPrice != null)
            {
                _context.AnimalPrice.Remove(animalPrice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalPriceExists(Guid id)
        {
            return _context.AnimalPrice.Any(e => e.AnimalPriceId == id);
        }
    }
}
