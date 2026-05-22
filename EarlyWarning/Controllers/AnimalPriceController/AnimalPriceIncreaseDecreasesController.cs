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
    public class AnimalPriceIncreaseDecreasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalPriceIncreaseDecreasesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: AnimalPriceIncreaseDecreases
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnimalPriceIncreaseDecreases.ToListAsync());
        }

        // GET: AnimalPriceIncreaseDecreases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPriceIncreaseDecrease = await _context.AnimalPriceIncreaseDecreases
                .FirstOrDefaultAsync(m => m.AnimalPriceIncreaseDecreaseId == id);
            if (animalPriceIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(animalPriceIncreaseDecrease);
        }

        // GET: AnimalPriceIncreaseDecreases/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnimalPriceIncreaseDecreases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalPriceIncreaseDecreaseId,ReasonName,ReasonType")] AnimalPriceIncreaseDecrease animalPriceIncreaseDecrease)
        {
            if (ModelState.IsValid)
            {
                animalPriceIncreaseDecrease.AnimalPriceIncreaseDecreaseId = Guid.NewGuid();
                _context.Add(animalPriceIncreaseDecrease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animalPriceIncreaseDecrease);
        }

        // GET: AnimalPriceIncreaseDecreases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPriceIncreaseDecrease = await _context.AnimalPriceIncreaseDecreases.FindAsync(id);
            if (animalPriceIncreaseDecrease == null)
            {
                return NotFound();
            }
            return View(animalPriceIncreaseDecrease);
        }

        // POST: AnimalPriceIncreaseDecreases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AnimalPriceIncreaseDecreaseId,ReasonName,ReasonType")] AnimalPriceIncreaseDecrease animalPriceIncreaseDecrease)
        {
            if (id != animalPriceIncreaseDecrease.AnimalPriceIncreaseDecreaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalPriceIncreaseDecrease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalPriceIncreaseDecreaseExists(animalPriceIncreaseDecrease.AnimalPriceIncreaseDecreaseId))
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
            return View(animalPriceIncreaseDecrease);
        }

        // GET: AnimalPriceIncreaseDecreases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalPriceIncreaseDecrease = await _context.AnimalPriceIncreaseDecreases
                .FirstOrDefaultAsync(m => m.AnimalPriceIncreaseDecreaseId == id);
            if (animalPriceIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(animalPriceIncreaseDecrease);
        }

        // POST: AnimalPriceIncreaseDecreases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalPriceIncreaseDecrease = await _context.AnimalPriceIncreaseDecreases.FindAsync(id);
            if (animalPriceIncreaseDecrease != null)
            {
                _context.AnimalPriceIncreaseDecreases.Remove(animalPriceIncreaseDecrease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalPriceIncreaseDecreaseExists(Guid id)
        {
            return _context.AnimalPriceIncreaseDecreases.Any(e => e.AnimalPriceIncreaseDecreaseId == id);
        }
    }
}
