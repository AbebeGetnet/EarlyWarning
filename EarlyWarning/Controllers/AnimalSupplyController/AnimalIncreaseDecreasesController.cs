using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.AnimalSupply;

namespace EarlyWarning.Controllers.AnimalSupplyController
{
    public class AnimalIncreaseDecreasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalIncreaseDecreasesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: AnimalIncreaseDecreases
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnimalIncreaseDecreases.ToListAsync());
        }

        // GET: AnimalIncreaseDecreases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalIncreaseDecrease = await _context.AnimalIncreaseDecreases
                .FirstOrDefaultAsync(m => m.AnimalIncreaseDecreaseId == id);
            if (animalIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(animalIncreaseDecrease);
        }

        // GET: AnimalIncreaseDecreases/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnimalIncreaseDecreases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalIncreaseDecreaseId,ReasonName,ReasonType")] AnimalIncreaseDecrease animalIncreaseDecrease)
        {
            if (ModelState.IsValid)
            {
                animalIncreaseDecrease.AnimalIncreaseDecreaseId = Guid.NewGuid();
                _context.Add(animalIncreaseDecrease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animalIncreaseDecrease);
        }

        // GET: AnimalIncreaseDecreases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalIncreaseDecrease = await _context.AnimalIncreaseDecreases.FindAsync(id);
            if (animalIncreaseDecrease == null)
            {
                return NotFound();
            }
            return View(animalIncreaseDecrease);
        }

        // POST: AnimalIncreaseDecreases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AnimalIncreaseDecreaseId,ReasonName,ReasonType")] AnimalIncreaseDecrease animalIncreaseDecrease)
        {
            if (id != animalIncreaseDecrease.AnimalIncreaseDecreaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalIncreaseDecrease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalIncreaseDecreaseExists(animalIncreaseDecrease.AnimalIncreaseDecreaseId))
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
            return View(animalIncreaseDecrease);
        }

        // GET: AnimalIncreaseDecreases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalIncreaseDecrease = await _context.AnimalIncreaseDecreases
                .FirstOrDefaultAsync(m => m.AnimalIncreaseDecreaseId == id);
            if (animalIncreaseDecrease == null)
            {
                return NotFound();
            }

            return View(animalIncreaseDecrease);
        }

        // POST: AnimalIncreaseDecreases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalIncreaseDecrease = await _context.AnimalIncreaseDecreases.FindAsync(id);
            if (animalIncreaseDecrease != null)
            {
                _context.AnimalIncreaseDecreases.Remove(animalIncreaseDecrease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalIncreaseDecreaseExists(Guid id)
        {
            return _context.AnimalIncreaseDecreases.Any(e => e.AnimalIncreaseDecreaseId == id);
        }
    }
}
