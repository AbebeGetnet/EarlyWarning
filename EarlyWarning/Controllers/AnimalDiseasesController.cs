using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models;

namespace EarlyWarning.Controllers
{
    public class AnimalDiseasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public AnimalDiseasesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: AnimalDiseases
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnimalDisease.ToListAsync());
        }

        // GET: AnimalDiseases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalDisease = await _context.AnimalDisease
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalDisease == null)
            {
                return NotFound();
            }

            return View(animalDisease);
        }

        // GET: AnimalDiseases/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnimalDiseases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] AnimalDisease animalDisease)
        {
            if (ModelState.IsValid)
            {
                animalDisease.Id = Guid.NewGuid();
                _context.Add(animalDisease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animalDisease);
        }

        // GET: AnimalDiseases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalDisease = await _context.AnimalDisease.FindAsync(id);
            if (animalDisease == null)
            {
                return NotFound();
            }
            return View(animalDisease);
        }

        // POST: AnimalDiseases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] AnimalDisease animalDisease)
        {
            if (id != animalDisease.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalDisease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalDiseaseExists(animalDisease.Id))
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
            return View(animalDisease);
        }

        // GET: AnimalDiseases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalDisease = await _context.AnimalDisease
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalDisease == null)
            {
                return NotFound();
            }

            return View(animalDisease);
        }

        // POST: AnimalDiseases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var animalDisease = await _context.AnimalDisease.FindAsync(id);
            if (animalDisease != null)
            {
                _context.AnimalDisease.Remove(animalDisease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalDiseaseExists(Guid id)
        {
            return _context.AnimalDisease.Any(e => e.Id == id);
        }
    }
}
