using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.WeeklyAccidents;

namespace EarlyWarning.Controllers.WeeklyAccidentsController
{
    public class AccidentTypesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public AccidentTypesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: AccidentTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.AccidentTypes.ToListAsync());
        }

        // GET: AccidentTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accidentType = await _context.AccidentTypes
                .FirstOrDefaultAsync(m => m.AccidentTypeId == id);
            if (accidentType == null)
            {
                return NotFound();
            }

            return View(accidentType);
        }

        // GET: AccidentTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AccidentTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccidentTypeId,AccidentName,IsActive")] AccidentType accidentType)
        {
            if (ModelState.IsValid)
            {
                accidentType.AccidentTypeId = Guid.NewGuid();
                _context.Add(accidentType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accidentType);
        }

        // GET: AccidentTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accidentType = await _context.AccidentTypes.FindAsync(id);
            if (accidentType == null)
            {
                return NotFound();
            }
            return View(accidentType);
        }

        // POST: AccidentTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AccidentTypeId,AccidentName,IsActive")] AccidentType accidentType)
        {
            if (id != accidentType.AccidentTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accidentType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccidentTypeExists(accidentType.AccidentTypeId))
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
            return View(accidentType);
        }

        // GET: AccidentTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accidentType = await _context.AccidentTypes
                .FirstOrDefaultAsync(m => m.AccidentTypeId == id);
            if (accidentType == null)
            {
                return NotFound();
            }

            return View(accidentType);
        }

        // POST: AccidentTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var accidentType = await _context.AccidentTypes.FindAsync(id);
            if (accidentType != null)
            {
                _context.AccidentTypes.Remove(accidentType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccidentTypeExists(Guid id)
        {
            return _context.AccidentTypes.Any(e => e.AccidentTypeId == id);
        }
    }
}
