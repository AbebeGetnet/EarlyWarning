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
    public class CropPestAndDeseasesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public CropPestAndDeseasesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: CropPestAndDeseases
        public async Task<IActionResult> Index()
        {
            return View(await _context.CropPestAndDesease.ToListAsync());
        }
               

        // GET: CropPestAndDeseases/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CropPestAndDeseases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CropPestAndDesease cropPestAndDesease)
        {
            if (ModelState.IsValid)
            {
                cropPestAndDesease.Id = Guid.NewGuid();
                _context.Add(cropPestAndDesease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cropPestAndDesease);
        }

        // GET: CropPestAndDeseases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cropPestAndDesease = await _context.CropPestAndDesease.FindAsync(id);
            if (cropPestAndDesease == null)
            {
                return NotFound();
            }
            return View(cropPestAndDesease);
        }

        // POST: CropPestAndDeseases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,CPDType")] CropPestAndDesease cropPestAndDesease)
        {
            if (id != cropPestAndDesease.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cropPestAndDesease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CropPestAndDeseaseExists(cropPestAndDesease.Id))
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
            return View(cropPestAndDesease);
        }

        // GET: CropPestAndDeseases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cropPestAndDesease = await _context.CropPestAndDesease
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cropPestAndDesease == null)
            {
                return NotFound();
            }

            return View(cropPestAndDesease);
        }

        // POST: CropPestAndDeseases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cropPestAndDesease = await _context.CropPestAndDesease.FindAsync(id);
            if (cropPestAndDesease != null)
            {
                _context.CropPestAndDesease.Remove(cropPestAndDesease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CropPestAndDeseaseExists(Guid id)
        {
            return _context.CropPestAndDesease.Any(e => e.Id == id);
        }
    }
}
