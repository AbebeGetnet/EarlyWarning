using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Controllers
{
    [Authorize]
    public class AnimalHealthStatusController : Controller
    {
        private readonly AnimalHealthStatusService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EarlyWarningDbContext _dbContext;

        public AnimalHealthStatusController(AnimalHealthStatusService service, UserManager<ApplicationUser> userManager, EarlyWarningDbContext dbContext)
        {
            _service = service;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: List
        public async Task<IActionResult> Index()
        {
            var reports = await _service.GetAllReportsAsync();
            // Load disease names for each report
            var allDiseases = await _dbContext.AnimalDisease.ToListAsync();
            foreach (var report in reports)
            {
                report.DeserializeCropDiseases();
                report.DiseaseNames = allDiseases
                    .Where(d => report.SelectedDiseaseIds.Contains(d.Id))
                    .Select(d => d.Name)
                    .ToList();
            }
            return View(reports);
        }

        // GET: Create
        public async Task<IActionResult> Create(RegistrationWithardViewModel model, DateTime StartDate, DateTime EndDate)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }
            var animalDeseases = await _dbContext.AnimalDisease.ToListAsync();


            ViewBag.AnimalDiseases = animalDeseases;
            ViewBag.WoredaName = woreda.LocationName;
            //await LoadDiseasesViewBag();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnimalHealthStatus model, List<Guid> SelectedDiseaseIds)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            if (model.WoredaId != woreda.Id)
                ModelState.AddModelError("", "Invalid woreda selection.");

            if (!ModelState.IsValid)
            {
                model.SelectedDiseaseIds = SelectedDiseaseIds ?? new List<Guid>();
                model.SerializeCropDiseases();
                model.Status = ReportStatus.Draft;
                await _service.CreateReportAsync(model);
                //TempData["Success"] = "Animal health status report created successfully.";
                return RedirectToAction("Create","HumanDrinkWater",new {model,model.StartDate, model.EndDate});
            }

            ViewBag.WoredaName = woreda.LocationName;
            await LoadDiseasesViewBag();
            return View(model);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(Guid id)
        {
            var report = await _service.GetReportByIdAsync(id);
            if (report == null) return NotFound();

            if (report.Status != ReportStatus.Draft)
            {
                TempData["Error"] = "Only draft reports can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var woreda = await GetCurrentUserWoredaAsync();
            if (report.WoredaId != woreda?.Id)
            {
                TempData["Error"] = "You are not authorized to edit this report.";
                return RedirectToAction(nameof(Index));
            }

            report.DeserializeCropDiseases();
            ViewBag.WoredaName = woreda?.LocationName;
            await LoadDiseasesViewBag();
            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AnimalHealthStatus model, List<Guid> SelectedDiseaseIds)
        {
            if (id != model.Id) return BadRequest();

            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            if (model.WoredaId != woreda.Id)
                ModelState.AddModelError("", "Invalid woreda selection.");

            if (!ModelState.IsValid)
            {
                var existingReport = await _service.GetReportByIdAsync(id);
                if (existingReport == null) return NotFound();

                if (existingReport.Status != ReportStatus.Draft)
                {
                    TempData["Error"] = "Only draft reports can be edited.";
                    return RedirectToAction(nameof(Index));
                }

                // Update scalar properties
                existingReport.Enough = model.Enough;
                existingReport.MaleHouseHold = model.MaleHouseHold;
                existingReport.FemaleHouseHold = model.FemaleHouseHold;
                existingReport.MaleFamily = model.MaleFamily;
                existingReport.FemaleFamily = model.FemaleFamily;
                existingReport.ChildhMale = model.ChildhMale;
                existingReport.ChildFemale = model.ChildFemale;
                existingReport.YouthMale = model.YouthMale;
                existingReport.YouthFemale = model.YouthFemale;
                existingReport.ElderlyMale = model.ElderlyMale;
                existingReport.ElderlyFemale = model.ElderlyFemale;
                existingReport.DisabledMale = model.DisabledMale;
                existingReport.DisabledFemale = model.DisabledFemale;
                existingReport.Remarks = model.Remarks;

                existingReport.SelectedDiseaseIds = SelectedDiseaseIds ?? new List<Guid>();
                await _service.UpdateReportAsync(existingReport);
                TempData["Success"] = "Animal health status report updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda.LocationName;
            await LoadDiseasesViewBag();
            return View(model);
        }

        // GET: Details
        public async Task<IActionResult> Details(Guid id)
        {
            var report = await _service.GetReportByIdAsync(id);
            if (report == null) return NotFound();

            report.DeserializeCropDiseases();
            var allDiseases = await _dbContext.AnimalDisease.ToListAsync();
            report.DiseaseNames = allDiseases
                .Where(d => report.SelectedDiseaseIds.Contains(d.Id))
                .Select(d => d.Name)
                .ToList();

            return View(report);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _service.DeleteReportAsync(id);
                TempData["Success"] = "Report deleted successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Submit
        [HttpPost]
        public async Task<IActionResult> Submit(Guid id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _service.SubmitReportAsync(id, currentUser?.Id);
                TempData["Success"] = "Report submitted for zone approval.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: ApproveByZone
        [Authorize(Roles = "ZoneAdmin")]
        [HttpPost]
        public async Task<IActionResult> ApproveByZone(Guid id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _service.ApproveByZoneAsync(id, currentUser.Id);
                TempData["Success"] = "Report approved by zone.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: ApproveByRegion
        [Authorize(Roles = "RegionAdmin")]
        [HttpPost]
        public async Task<IActionResult> ApproveByRegion(Guid id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _service.ApproveByRegionAsync(id, currentUser.Id);
                TempData["Success"] = "Report approved by region.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: RejectByZone
        [Authorize(Roles = "ZoneAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByZone(Guid id, string rejectionRemark)
        {
            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _service.RejectByZoneAsync(id, currentUser.Id, rejectionRemark);
                TempData["Success"] = "Report rejected at zone level.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: RejectByRegion
        [Authorize(Roles = "RegionAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByRegion(Guid id, string rejectionRemark)
        {
            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _service.RejectByRegionAsync(id, currentUser.Id, rejectionRemark);
                TempData["Success"] = "Report rejected at region level.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<Locations?> GetCurrentUserWoredaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _dbContext.Locations.FindAsync(user.LocationId);
        }

        private async Task LoadDiseasesViewBag()
        {
            var diseases = await _dbContext.AnimalDisease.ToListAsync();
            ViewBag.AnimalDiseases = diseases;
        }
    }
}