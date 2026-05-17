using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Models.Wizard;
using EarlyWarning.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EarlyWarning.Controllers
{
    public class WizardController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly IReportSaveService _saveService;

        public WizardController(EarlyWarningDbContext context, IReportSaveService saveService)
        {
            _context = context;
            _saveService = saveService;
        }

        private const string WizardDataKey = "WizardData";

        private CombinedReportWizardModel GetWizardFromTempData()
        {
            var json = TempData[WizardDataKey] as string;
            if (string.IsNullOrEmpty(json))
                return new CombinedReportWizardModel();
            return JsonSerializer.Deserialize<CombinedReportWizardModel>(json);
        }

        private void SaveWizardToTempData(CombinedReportWizardModel model)
        {
            TempData[WizardDataKey] = JsonSerializer.Serialize(model);
            TempData.Keep(WizardDataKey);
        }

        [HttpGet("wizard")]
        public async Task<IActionResult> Index(int step = 1)
        {
            var model = GetWizardFromTempData();
            model.CurrentStep = step;
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost("wizard/step")]
        public async Task<IActionResult> NextStep(CombinedReportWizardModel incoming, string action)
        {
            // Load the existing wizard state
            var existing = GetWizardFromTempData();

            // Update only the current step's data from incoming
            switch (existing.CurrentStep)
            {
                case 1:
                    existing.Rainfall = incoming.Rainfall;
                    break;
                case 2:
                    existing.Farming = incoming.Farming;
                    break;
                case 3:
                    existing.Growth = incoming.Growth;
                    break;
                case 4:
                    existing.PestDisease = incoming.PestDisease;
                    break;
                case 5:
                    existing.Pasture = incoming.Pasture;
                    break;
                case 6:
                    existing.WaterSupply = incoming.WaterSupply;
                    break;
                case 7:
                    existing.AnimalHealth = incoming.AnimalHealth;
                    break;
            }

            // Update common fields
            existing.WoredaId = incoming.WoredaId;
            existing.ReportingWeekStart = incoming.ReportingWeekStart;

            // Validate only the current step
            bool isValid = ValidateStep(existing, existing.CurrentStep);

            if (action == "next" && !isValid)
            {
                // Stay on same step, show validation errors
                SaveWizardToTempData(existing);
                await PopulateDropdowns(existing);
                return View("Index", existing);
            }

            // Navigation: update step index
            if (action == "next")
                existing.CurrentStep++;
            else if (action == "prev")
                existing.CurrentStep--;

            // If this is final submit
            if (action == "submit")
            {
                // Ensure all steps are valid before saving (optional)
                if (!IsModelCompletelyValid(existing))
                {
                    TempData["Error"] = "እባክዎ ሁሉንም ደረጃዎች በሚገባ ይሙሉ።";
                    existing.CurrentStep = 1; // go back to first step
                    SaveWizardToTempData(existing);
                    await PopulateDropdowns(existing);
                    return View("Index", existing);
                }

                try
                {
                    await _saveService.SaveAllReportsAsync(existing, User.Identity.Name);
                    TempData["Success"] = "ሪፖርት በሚገባ ተመዝግቧል!";
                    // Clear wizard data after successful save
                    TempData.Remove(WizardDataKey);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"ስህተት: {ex.Message}";
                    SaveWizardToTempData(existing);
                    await PopulateDropdowns(existing);
                    return View("Index", existing);
                }
            }

            // Save updated wizard state and go to next/previous step
            SaveWizardToTempData(existing);
            await PopulateDropdowns(existing);
            return RedirectToAction("Index", new { step = existing.CurrentStep });
        }

        private bool ValidateStep(CombinedReportWizardModel model, int step)
        {
            object stepModel = step switch
            {
                1 => model.Rainfall,
                2 => model.Farming,
                3 => model.Growth,
                4 => model.PestDisease,
                5 => model.Pasture,
                6 => model.WaterSupply,
                7 => model.AnimalHealth,
                _ => null
            };

            if (stepModel == null) return true;

            var validationContext = new ValidationContext(stepModel);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(stepModel, validationContext, results, true);
        }

        private bool IsModelCompletelyValid(CombinedReportWizardModel model)
        {
            // Basic check – you can expand to check all steps if needed
            return model.WoredaId != Guid.Empty && model.ReportingWeekStart != default;
        }

        private async Task PopulateDropdowns(CombinedReportWizardModel model)
        {
            // 1. Woredas – safe list
            var woredas = await _context.Locations
                .Where(l => l.Level == LocationLevel.ወረዳ)
                .Select(l => new { l.Id, l.LocationName })
                .ToListAsync();

            var safeWoredaItems = woredas
                .Where(w => w != null && w.Id != Guid.Empty && !string.IsNullOrEmpty(w.LocationName))
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.LocationName
                })
                .ToList();

            ViewBag.Woredas = safeWoredaItems.Any() ? safeWoredaItems : new List<SelectListItem>();

            // 2. Crop Pest / Diseases – safe list
            var cropPests = await _context.CropPestAndDesease.ToListAsync();
            var safeCropPestItems = cropPests
                .Where(c => c != null && c.Id != Guid.Empty && !string.IsNullOrEmpty(c.Name))
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            ViewBag.CropPestDiseases = safeCropPestItems.Any() ? safeCropPestItems : new List<SelectListItem>();

            // 3. Animal Diseases – safe list
            var animalDiseases = await _context.AnimalDisease.ToListAsync();
            var safeAnimalDiseaseItems = animalDiseases
                .Where(a => a != null && a.Id != Guid.Empty && !string.IsNullOrEmpty(a.Name))
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();

            ViewBag.AnimalDiseases = safeAnimalDiseaseItems.Any() ? safeAnimalDiseaseItems : new List<SelectListItem>();
        }
    }
}