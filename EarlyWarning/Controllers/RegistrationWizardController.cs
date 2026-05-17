using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class RegistrationWizardController : Controller
{
    private readonly EarlyWarningDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegistrationWizardController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    // GET: RegistrationWizard/Index
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var userWoreda = await _context.Locations.FindAsync(user.LocationId);

        // Get all distinct (WoredaId, StartDate, EndDate) groups
        var groups = await _context.RainfallReports
            .Where(r => userWoreda == null || r.WoredaId == userWoreda.Id) // filter if normal user
            .Select(r => new { r.WoredaId, r.StartDate, r.EndDate })
            .Distinct()
            .OrderByDescending(g => g.StartDate)
            .ToListAsync();

        var result = new List<RegistrationWizardIndexViewModel>();

        foreach (var group in groups)
        {
            var woreda = await _context.Locations.FindAsync(group.WoredaId);
            var rainfall = await _context.RainfallReports
                .FirstOrDefaultAsync(r => r.WoredaId == group.WoredaId && r.StartDate == group.StartDate && r.EndDate == group.EndDate);
            var farming = await _context.FarmingActivities
                .FirstOrDefaultAsync(f => f.WoredaId == group.WoredaId && f.StartDate == group.StartDate && f.EndDate == group.EndDate);
            var cropReport = await _context.CropPestAndDeseaseReports
                .FirstOrDefaultAsync(c => c.WoredaId == group.WoredaId && c.StartDate == group.StartDate && c.EndDate == group.EndDate);
            var pasture = await _context.PastureStatuses
                .FirstOrDefaultAsync(p => p.WoredaId == group.WoredaId && p.StartDate == group.StartDate && p.EndDate == group.EndDate);
            var water = await _context.AnimalWaterSupplyStatuses
                .FirstOrDefaultAsync(w => w.WoredaId == group.WoredaId && w.StartDate == group.StartDate && w.EndDate == group.EndDate);
            var health = await _context.AnimalHealthStatuses
                .FirstOrDefaultAsync(h => h.WoredaId == group.WoredaId && h.StartDate == group.StartDate && h.EndDate == group.EndDate);

            // Deserialize disease names & kebele names
            List<string> cropDiseaseNames = new();
            if (cropReport != null && cropReport.HasPestAndDeseasOccured)
            {
                cropReport.DeserializeCropDiseases();
                var allCropDiseases = await _context.CropPestAndDesease.ToListAsync();
                cropDiseaseNames = allCropDiseases
                    .Where(d => cropReport.SelectedDiseaseIds.Contains(d.Id))
                    .Select(d => d.Name)
                    .ToList();
            }

            List<string> animalDiseaseNames = new();
            if (health != null && health.Enough) // "አለ"
            {
                health.DeserializeCropDiseases();
                var allAnimalDiseases = await _context.AnimalDisease.ToListAsync();
                animalDiseaseNames = allAnimalDiseases
                    .Where(d => health.SelectedDiseaseIds.Contains(d.Id))
                    .Select(d => d.Name)
                    .ToList();
            }

            List<string> kebeleNames = new();
            if (rainfall != null)
            {
                rainfall.DeserializeKebeles();
                var kebeles = await _context.Locations
                    .Where(k => rainfall.SelectedKebeleIds.Contains(k.Id))
                    .Select(k => k.LocationName)
                    .ToListAsync();
                kebeleNames = kebeles;
            }

            result.Add(new RegistrationWizardIndexViewModel
            {
                WoredaId = group.WoredaId,
                WoredaName = woreda?.LocationName ?? "",
                StartDate = group.StartDate,
                EndDate = group.EndDate,
                RainfallReport = rainfall,
                FarmingActivity = farming,
                CropPestAndDeseaseReport = cropReport,
                PastureStatus = pasture,
                AnimalWaterSupplyStatus = water,
                AnimalHealthStatus = health,
                CropDiseaseNames = cropDiseaseNames,
                AnimalDiseaseNames = animalDiseaseNames,
                KebeleNames = kebeleNames
            });
        }

        return View(result);
    }

    //[Authorize(Roles = "Data Encoder")]
    // GET: Show the wizard
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var woreda = await _context.Locations.FindAsync(user.LocationId);
        //if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
        //{
        //    TempData["Error"] = "Your account is not linked to a valid woreda.";
        //    return RedirectToAction("Index", "Home");
        //}

        // Load kebeles under this woreda
        var kebeles = await _context.Locations
            .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
            .OrderBy(l => l.LocationName).ToListAsync();

        var model = new RegistrationWithardViewModel
        {
            WoredaId = woreda.Id,
            StartDate = DateTime.Now.Date,
            EndDate = DateTime.Now.Date.AddDays(7),
            Kebelies = kebeles,
            AnimalDiseases = await _context.AnimalDisease.ToListAsync(),
            CropPestAndDeseases = await _context.CropPestAndDesease.ToListAsync(),
            // Initialize each report
            RainfallReport = new RainfallReport { WoredaId = woreda.Id },
            FarmingActivity = new FarmingActivity { WoredaId = woreda.Id },
            CropPestAndDeseaseReport = new CropPestAndDeseaseReport { WoredaId = woreda.Id },
            PastureStatus = new PastureStatus { WoredaId = woreda.Id },
            AnimalWaterSupplyStatus = new AnimalWaterSupplyStatus { WoredaId = woreda.Id },
            AnimalHealthStatus = new AnimalHealthStatus { WoredaId = woreda.Id }
        };

        ViewBag.WoredaName = woreda.LocationName;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegistrationWithardViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var woreda = await _context.Locations.FindAsync(user.LocationId);
        //if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
        //{
        //    TempData["Error"] = "Invalid woreda.";
        //    return RedirectToAction("Index", "Home");
        //}

        // Ensure all child reports have the correct WoredaId
        model.RainfallReport.WoredaId = woreda.Id;
        model.FarmingActivity.WoredaId = woreda.Id;
        model.CropPestAndDeseaseReport.WoredaId = woreda.Id;
        model.PastureStatus.WoredaId = woreda.Id;
        model.AnimalWaterSupplyStatus.WoredaId = woreda.Id;
        model.AnimalHealthStatus.WoredaId = woreda.Id;

        // Set shared dates and status
        foreach (var report in new dynamic[] {
            model.RainfallReport,
            model.FarmingActivity,
            model.CropPestAndDeseaseReport,
            model.PastureStatus,
            model.AnimalWaterSupplyStatus,
            model.AnimalHealthStatus })
        {
            report.StartDate = model.StartDate;
            report.EndDate = model.EndDate;
            report.Status = ReportStatus.Draft;
        }

        // Handle kebele selection for RainfallReport
        model.RainfallReport.SelectedKebeleIds = model.SelectedKebeliesIds ?? new List<Guid>();
        model.RainfallReport.SelectedDroughtKebeleIds = model.SelectedKebeliesIds ?? new List<Guid>();
        model.RainfallReport.SerializeKebeles();          // for flood‑affected kebeles
        model.RainfallReport.SerializeDroughtKebeles();   // for drought‑affected kebeles

        // Handle crop disease selection
        model.CropPestAndDeseaseReport.SelectedDiseaseIds = model.SelectedCropDiseaseIds ?? new List<Guid>();
        model.CropPestAndDeseaseReport.SerializeCropDiseases();

        // Handle animal disease selection
        model.AnimalHealthStatus.SelectedDiseaseIds = model.SelectedAnimalDiseaseIds ?? new List<Guid>();
        model.AnimalHealthStatus.SerializeCropDiseases(); // reuses same method

        // Validate each report (you can add more validation as needed)
        if (!ModelState.IsValid)
        {
            // Reload lists for redisplay
            model.Kebelies = await _context.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();
            model.AnimalDiseases = await _context.AnimalDisease.ToListAsync();
            model.CropPestAndDeseases = await _context.CropPestAndDesease.ToListAsync();
            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }

        // Save all reports in a transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.RainfallReports.AddAsync(model.RainfallReport);
            await _context.FarmingActivities.AddAsync(model.FarmingActivity);
            await _context.CropPestAndDeseaseReports.AddAsync(model.CropPestAndDeseaseReport);
            await _context.PastureStatuses.AddAsync(model.PastureStatus);
            await _context.AnimalWaterSupplyStatuses.AddAsync(model.AnimalWaterSupplyStatus);
            await _context.AnimalHealthStatuses.AddAsync(model.AnimalHealthStatus);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = "All reports have been saved successfully.";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            // Reload lists again
            model.Kebelies = await _context.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();
            model.AnimalDiseases = await _context.AnimalDisease.ToListAsync();
            model.CropPestAndDeseases = await _context.CropPestAndDesease.ToListAsync();
            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }
    }
}