using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EarlyWarning.Data;
using EarlyWarning.Models.EpidemicDisease;

namespace EarlyWarning.Controllers.EpidemicDiseaseController
{
    public class EpidemicDiseaseTypesController : Controller
    {
        private readonly EarlyWarningDbContext _context;

        public EpidemicDiseaseTypesController(EarlyWarningDbContext context)
        {
            _context = context;
        }

        // GET: EpidemicDiseaseTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.EpidemicDiseaseTypes.ToListAsync());
        }

        // GET: EpidemicDiseaseTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var epidemicDiseaseType = await _context.EpidemicDiseaseTypes
                .FirstOrDefaultAsync(m => m.EpidemicDiseaseTypeId == id);
            if (epidemicDiseaseType == null)
            {
                return NotFound();
            }

            return View(epidemicDiseaseType);
        }

        // GET: EpidemicDiseaseTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EpidemicDiseaseTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( EpidemicDiseaseType epidemicDiseaseType)
        {
            try
            {
                // Check if disease with same name already exists
                // Option 3: Case-insensitive (works for both English and Amharic)
                // Option 2: Trim whitespace and compare
                

                if (ModelState.IsValid)
                {
                    epidemicDiseaseType.EpidemicDiseaseTypeId = Guid.NewGuid();
                    _context.EpidemicDiseaseTypes.Add(epidemicDiseaseType);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "በሽታው በሚገባ ተመዝግቧል" });
                    }

                    TempData["Success"] = "በሽታው በሚገባ ተመዝግቧል";
                    return RedirectToAction(nameof(Index));
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return View(epidemicDiseaseType);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "ውሂቡን ለማስቀመጥ አልተቻለም: " + ex.Message });
                }

                ModelState.AddModelError("", "ውሂቡን ለማስቀመጥ አልተቻለም");
                return View(epidemicDiseaseType);
            }
        }

        // GET: EpidemicDiseaseTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var epidemicDiseaseType = await _context.EpidemicDiseaseTypes.FindAsync(id);
            if (epidemicDiseaseType == null)
            {
                return NotFound();
            }
            return View(epidemicDiseaseType);
        }

        // POST: EpidemicDiseaseTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: EpidemicDisease/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("EpidemicDiseaseId,Name,Description,IsActive")] EpidemicDiseaseType epidemicDiseaseType)
        {
            try
            {
                // Check if another disease with same name exists (excluding current)
                var existingDisease = await _context.EpidemicDiseaseTypes
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == epidemicDiseaseType.Name.ToLower()
                        && d.EpidemicDiseaseTypeId != epidemicDiseaseType.EpidemicDiseaseTypeId);

                if (existingDisease != null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "ይህ የበሽታ ስም ቀድሞ ተመዝግቧል" });
                    }
                    ModelState.AddModelError("Name", "ይህ የበሽታ ስም ቀድሞ ተመዝግቧል");
                    return View(epidemicDiseaseType);
                }

                if (ModelState.IsValid)
                {
                    var existing = await _context.EpidemicDiseaseTypes.FindAsync(epidemicDiseaseType.EpidemicDiseaseTypeId);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "በሽታው አልተገኘም" });
                    }

                    existing.Name = epidemicDiseaseType.Name;
                    existing.Description = epidemicDiseaseType.Description;
                    existing.IsActive = epidemicDiseaseType.IsActive;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "በሽታው በሚገባ ተሻሽሏል" });
                    }

                    TempData["Success"] = "በሽታው በሚገባ ተሻሽሏል";
                    return RedirectToAction(nameof(Index));
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return View(epidemicDiseaseType);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "ውሂቡን ለማስተካከል አልተቻለም: " + ex.Message });
                }

                ModelState.AddModelError("", "ውሂቡን ለማስተካከል አልተቻለም");
                return View(epidemicDiseaseType);
            }
        }

        // GET: EpidemicDiseaseTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var epidemicDiseaseType = await _context.EpidemicDiseaseTypes
                .FirstOrDefaultAsync(m => m.EpidemicDiseaseTypeId == id);
            if (epidemicDiseaseType == null)
            {
                return NotFound();
            }

            return View(epidemicDiseaseType);
        }

        // POST: EpidemicDiseaseTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var epidemicDiseaseType = await _context.EpidemicDiseaseTypes.FindAsync(id);
            if (epidemicDiseaseType != null)
            {
                _context.EpidemicDiseaseTypes.Remove(epidemicDiseaseType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EpidemicDiseaseTypeExists(Guid id)
        {
            return _context.EpidemicDiseaseTypes.Any(e => e.EpidemicDiseaseTypeId == id);
        }
    }
}
