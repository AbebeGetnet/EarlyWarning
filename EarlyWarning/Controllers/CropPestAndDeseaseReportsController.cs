using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class CropPestAndDeseasReportsController : Controller
{
    private readonly ICropPestAndDeseasReportRepository _repository;
    private readonly EarlyWarningDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CropPestAndDeseasReportsController(ICropPestAndDeseasReportRepository repository, EarlyWarningDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _dbContext = dbContext;
        _userManager = userManager;
    }
    public async Task<IActionResult> Index()
    {
        var reports = await _repository.GetAllAsync(r => r.Woreda);
        return View(reports);
    }
    [Authorize(Roles = "Data Encoder")]
    public async Task<IActionResult> Create(RegistrationWithardViewModel model, DateTime StartDate, DateTime EndDate)
    {
        var woreda = await GetCurrentUserWoredaAsync();
        if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
        {
            TempData["Error"] = "Your account is not linked to a valid woreda.";
            return RedirectToAction(nameof(Index));
        }
        var cropPestAndDeseases = await _dbContext.CropPestAndDesease.ToListAsync();

        var cropModel = new RegistrationWithardViewModel
        {
            CropPestAndDeseaseReport = model.CropPestAndDeseaseReport,
        };
        return View(cropModel);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegistrationWithardViewModel model, List<Guid> SelectedDiseaseIds, DateTime StartDate, DateTime EndDate)
    {
        var woreda = await GetCurrentUserWoredaAsync();
        var currentUser = await _userManager.GetUserAsync(User);

        if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
        {
            TempData["Error"] = "Invalid woreda.";
            return RedirectToAction(nameof(Index));
        }

        if (model.WoredaId != woreda.Id)
            ModelState.AddModelError("", "Woreda mismatch.");

        if (!ModelState.IsValid)
        {
            model.CropPestAndDeseaseReport.WoredaId = woreda.Id;
            model.CropPestAndDeseaseReport.UserId = currentUser.Id;
            model.CropPestAndDeseaseReport.StartDate = StartDate;
            model.CropPestAndDeseaseReport.EndDate = EndDate;
            model.CropPestAndDeseaseReport.Status = ReportStatus.Draft;
            model.CropPestAndDeseaseReport.SelectedDiseaseIds = SelectedDiseaseIds ?? new List<Guid>();
            model.CropPestAndDeseaseReport.SerializeCropDiseases();
            await _repository.AddAsync(model.CropPestAndDeseaseReport);
            await _repository.SaveChangesAsync();
            //TempData["Success"] = "Crop pest/disease report created.";
            return RedirectToAction("Create", "PastureStatus", new {model, model.StartDate, model.EndDate});
        }

        ViewBag.WoredaName = woreda.LocationName;
        return View(model);
    }

    // GET: CropPestAndDeseaseReport/Details/{id}
    public async Task<IActionResult> Details(Guid id)
    {
        var report = await _repository.GetByIdAsync(id, r => r.Woreda);
        if (report == null)
            return NotFound();

        // Deserialize selected disease IDs from JSON
        report.DeserializeCropDiseases();

        // Load all diseases to resolve names for display
        var allDiseases = await _dbContext.CropPestAndDesease.ToListAsync();
        report.DiseaseNames = allDiseases
            .Where(d => report.SelectedDiseaseIds.Contains(d.Id))
            .Select(d => d.Name)
            .ToList();

        return View(report);
    }
    // GET: Edit
    public async Task<IActionResult> Edit(Guid id)
    {
        var report = await _repository.GetByIdAsync(id, r => r.Woreda);
        if (report == null)
            return NotFound();

        // Get all diseases for the checkboxes
        var allDiseases = await _dbContext.CropPestAndDesease.ToListAsync(); // or via another repo
        ViewBag.CropPestAndDeseases = allDiseases;
        ViewBag.WoredaName = report.Woreda?.LocationName;

        return View(report);
    }

    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CropPestAndDeseaseReport model)
    {
        if (id != model.Id)
            return BadRequest();

        // Remove navigation properties from ModelState validation
        ModelState.Remove("Woreda");
        ModelState.Remove("DiseaseNames"); // if present

        if (ModelState.IsValid)
        {
            try
            {
                // Retrieve the existing report from the database (tracked)
                var existingReport = await _repository.GetByIdAsync(id);
                if (existingReport == null)
                    return NotFound();

                existingReport.HasPestAndDeseasOccured = model.HasPestAndDeseasOccured;
                existingReport.AffectedLandInHectar = model.AffectedLandInHectar;
                existingReport.TypeOfCropAffected = model.TypeOfCropAffected;
                existingReport.MaleHouseHold = model.MaleHouseHold;
                existingReport.FemaleHouseHold = model.FemaleHouseHold;
                existingReport.ChildhMale = model.ChildhMale;
                existingReport.ChildFemale = model.ChildFemale;
                existingReport.YouthMale = model.YouthMale;
                existingReport.YouthFemale = model.YouthFemale;
                existingReport.ElderlyMale = model.ElderlyMale;
                existingReport.ElderlyFemale = model.ElderlyFemale;
                existingReport.DisabledMale = model.DisabledMale;
                existingReport.DisabledFemale = model.DisabledFemale;
                existingReport.Remarks = model.Remarks;
                existingReport.WoredaId = model.WoredaId;

                // Update the selected disease IDs
                existingReport.SelectedDiseaseIds = model.SelectedDiseaseIds ?? new List<Guid>();
                existingReport.SerializeCropDiseases();

                // The repository's Update method will serialize JSON automatically
                _repository.Update(existingReport);
                await _repository.SaveChangesAsync();

                TempData["Success"] = "ሪፖርቱ በሚገባ ተስተካክሏል።";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ReportExists(id))
                    return NotFound();
                throw;
            }
        }

        // If we got this far, something failed – redisplay form
        var allDiseases = await _dbContext.CropPestAndDesease.ToListAsync();
        ViewBag.CropPestAndDeseases = allDiseases;
        ViewBag.WoredaName = (await _dbContext.Locations.FindAsync(model.WoredaId))?.LocationName;
        return View(model);
    }

    // GET: Delete confirmation page (optional but recommended for better UX)
    public async Task<IActionResult> Delete(Guid id)
    {
        var report = await _repository.GetByIdAsync(id, r => r.Woreda);
        if (report == null)
            return NotFound();

        // Deserialize diseases to show which ones were selected (optional)
        report.DeserializeCropDiseases();
        var allDiseases = await _dbContext.CropPestAndDesease.ToListAsync();
        report.DiseaseNames = allDiseases
            .Where(d => report.SelectedDiseaseIds.Contains(d.Id))
            .Select(d => d.Name)
            .ToList();

        return View(report);
    }

    // POST: Delete the report
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        // Optional: Prevent deletion of already submitted/approved reports
        if (report.Status != ReportStatus.Draft && report.Status != ReportStatus.Rejected)
        {
            TempData["Error"] = "ሪፖርቱ መሰረዝ አይቻልም። በረቂቅ ወይም ውድቅ ሁኔታ ላይ ያሉ ሪፖርቶችን ብቻ መሰረዝ ይቻላል።";
            return RedirectToAction(nameof(Index));
        }

        _repository.Delete(report);
        await _repository.SaveChangesAsync();

        TempData["Success"] = "ሪፖርቱ በሚገባ ተሰርዟል።";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Data Encoder")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        var woreda = await GetCurrentUserWoredaAsync();
        if (woreda == null || report.WoredaId != woreda.Id)
        {
            TempData["Error"] = "You are not authorized to submit this report.";
            return RedirectToAction(nameof(Index));
        }

        if (report.Status != ReportStatus.Draft && report.Status != ReportStatus.Rejected)
        {
            TempData["Error"] = "Only draft or rejected reports can be submitted.";
            return RedirectToAction(nameof(Index));
        }

        report.Status = ReportStatus.Submitted;
        report.SubmittedAt = DateTime.UtcNow;
        report.SubmittedById = (await _userManager.GetUserAsync(User))?.Id;

        await _repository.SaveChangesAsync();
        TempData["Success"] = "Report submitted for zone approval.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ZoneAdmin")]
    public async Task<IActionResult> ApproveByZone(Guid id)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        if (report.Status != ReportStatus.Submitted)
        {
            TempData["Error"] = "Only submitted reports can be zone‑approved.";
            return RedirectToAction(nameof(Index));
        }

        report.Status = ReportStatus.ZoneApproved;
        report.ZoneApprovedAt = DateTime.UtcNow;
        report.ZoneApprovedById = (await _userManager.GetUserAsync(User))?.Id;

        await _repository.SaveChangesAsync();
        TempData["Success"] = "Report approved at zone level.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ZoneAdmin")]
    public async Task<IActionResult> RejectByZone(Guid id, string rejectionRemark)
    {
        if (string.IsNullOrWhiteSpace(rejectionRemark))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction(nameof(Index));
        }

        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        if (report.Status != ReportStatus.Submitted)
        {
            TempData["Error"] = "Only submitted reports can be rejected by zone.";
            return RedirectToAction(nameof(Index));
        }

        report.Status = ReportStatus.Rejected;
        report.ZoneRejectionRemark = rejectionRemark;
        report.ZoneRejectedAt = DateTime.UtcNow;
        report.ZoneRejectedById = (await _userManager.GetUserAsync(User))?.Id;

        await _repository.SaveChangesAsync();
        TempData["Success"] = "Report rejected at zone level.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "RegionAdmin")]
    public async Task<IActionResult> ApproveByRegion(Guid id)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        if (report.Status != ReportStatus.ZoneApproved)
        {
            TempData["Error"] = "Only zone‑approved reports can be region‑approved.";
            return RedirectToAction(nameof(Index));
        }

        report.Status = ReportStatus.RegionApproved;
        report.RegionApprovedAt = DateTime.UtcNow;
        report.RegionApprovedById = (await _userManager.GetUserAsync(User))?.Id;

        await _repository.SaveChangesAsync();
        TempData["Success"] = "Report approved at region level.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "RegionAdmin")]
    public async Task<IActionResult> RejectByRegion(Guid id, string rejectionRemark)
    {
        if (string.IsNullOrWhiteSpace(rejectionRemark))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction(nameof(Index));
        }

        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        if (report.Status != ReportStatus.ZoneApproved)
        {
            TempData["Error"] = "Only zone‑approved reports can be rejected by region.";
            return RedirectToAction(nameof(Index));
        }

        report.Status = ReportStatus.Rejected;
        report.RegionRejectionRemark = rejectionRemark;
        report.RegionRejectedAt = DateTime.UtcNow;
        report.RegionRejectedById = (await _userManager.GetUserAsync(User))?.Id;

        await _repository.SaveChangesAsync();
        TempData["Success"] = "Report rejected at region level.";
        return RedirectToAction(nameof(Index));
    }
    private async Task<Locations?> GetCurrentUserWoredaAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user == null ? null : await _dbContext.Locations.FindAsync(user.LocationId);
    }
    private async Task<bool> ReportExists(Guid id)
    {
        return await _repository.GetByIdAsync(id) != null;
    }
}