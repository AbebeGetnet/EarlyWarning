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
    public class AnimalTypesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalTypesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: AnimalTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnimalTypes.ToListAsync());
        }

        // GET: AnimalTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalType = await _context.AnimalTypes
                .FirstOrDefaultAsync(m => m.AnimalTypeId == id);
            if (animalType == null)
            {
                return NotFound();
            }

            return View(animalType);
        }

        // GET: AnimalTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnimalTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalTypeId,AnimalTypeName,IsActive")] AnimalType animalType)
        {
            if (ModelState.IsValid)
            {
                animalType.AnimalTypeId = Guid.NewGuid();
                _context.Add(animalType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animalType);
        }

        // GET: AnimalTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalType = await _context.AnimalTypes.FindAsync(id);
            if (animalType == null)
            {
                return NotFound();
            }
            return View(animalType);
        }

        // POST: AnimalTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AnimalTypeId,AnimalTypeName,IsActive")] AnimalType animalType)
        {
            if (id != animalType.AnimalTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalTypeExists(animalType.AnimalTypeId))
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
            return View(animalType);
        }

        // GET: AnimalTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalType = await _context.AnimalTypes
                .FirstOrDefaultAsync(m => m.AnimalTypeId == id);
            if (animalType == null)
            {
                return NotFound();
            }

            return View(animalType);
        }

        // POST: AnimalTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalType = await _context.AnimalTypes.FindAsync(id);
            if (animalType != null)
            {
                _context.AnimalTypes.Remove(animalType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalTypeExists(Guid id)
        {
            return _context.AnimalTypes.Any(e => e.AnimalTypeId == id);
        }
    }
}
