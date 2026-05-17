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
    public class SupplyTypesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public SupplyTypesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: SupplyTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.SupplyTypes.ToListAsync());
        }

        // GET: SupplyTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplyType = await _context.SupplyTypes
                .FirstOrDefaultAsync(m => m.SupplyTypeId == id);
            if (supplyType == null)
            {
                return NotFound();
            }

            return View(supplyType);
        }

        // GET: SupplyTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SupplyTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupplyTypeId,SupplyTypeName,IsActive")] SupplyType supplyType)
        {
            if (ModelState.IsValid)
            {
                supplyType.SupplyTypeId = Guid.NewGuid();
                _context.Add(supplyType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplyType);
        }

        // GET: SupplyTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplyType = await _context.SupplyTypes.FindAsync(id);
            if (supplyType == null)
            {
                return NotFound();
            }
            return View(supplyType);
        }

        // POST: SupplyTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("SupplyTypeId,SupplyTypeName,IsActive")] SupplyType supplyType)
        {
            if (id != supplyType.SupplyTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplyType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplyTypeExists(supplyType.SupplyTypeId))
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
            return View(supplyType);
        }

        // GET: SupplyTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplyType = await _context.SupplyTypes
                .FirstOrDefaultAsync(m => m.SupplyTypeId == id);
            if (supplyType == null)
            {
                return NotFound();
            }

            return View(supplyType);
        }

        // POST: SupplyTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var supplyType = await _context.SupplyTypes.FindAsync(id);
            if (supplyType != null)
            {
                _context.SupplyTypes.Remove(supplyType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupplyTypeExists(Guid id)
        {
            return _context.SupplyTypes.Any(e => e.SupplyTypeId == id);
        }
    }
}
