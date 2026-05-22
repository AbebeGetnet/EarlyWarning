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
    public class AnimalWaterSupplyStatusController : Controller
    {
        private readonly AnimalWaterSupplyStatusService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EarlyWarningDbContext _dbContext;

        public AnimalWaterSupplyStatusController(AnimalWaterSupplyStatusService service, UserManager<ApplicationUser> userManager, EarlyWarningDbContext dbContext)
        {
            _service = service;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: AnimalWaterSupplyStatus
        public async Task<IActionResult> Index()
        {
            var reports = await _service.GetAllReportsAsync();
            return View(reports);
        }

        // GET: AnimalWaterSupplyStatus/Create
        public async Task<IActionResult> Create(RegistrationWithardViewModel model)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(RegistrationWithardViewModel model, DateTime StartDate, DateTime EndDate)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            if (model.AnimalWaterSupplyStatus.WoredaId != woreda.Id)
                ModelState.AddModelError("", "Invalid woreda selection.");

            if (!ModelState.IsValid)
            {
                model.AnimalWaterSupplyStatus.Status = ReportStatus.Draft;
                await _service.CreateReportAsync(model.AnimalWaterSupplyStatus);
                //TempData["Success"] = "Animal water supply status report created successfully.";
                return RedirectToAction("Create", "AnimalHealthStatus", new {model});
            }

            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }

        // GET: AnimalWaterSupplyStatus/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var report = await _service.GetReportByIdAsync(id);
            if (report == null)
                return NotFound();

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

            ViewBag.WoredaName = woreda?.LocationName;
            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AnimalWaterSupplyStatus model)
        {
            if (id != model.Id)
                return BadRequest();

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
                if (existingReport == null)
                    return NotFound();

                if (existingReport.Status != ReportStatus.Draft)
                {
                    TempData["Error"] = "Only draft reports can be edited.";
                    return RedirectToAction(nameof(Index));
                }

                // Update scalar properties
                existingReport.Enough = model.Enough;
                existingReport.NoOfKebeliesWithPastureShortage = model.NoOfKebeliesWithPastureShortage;
                existingReport.NoOfKebeliesWithWaterSupply = model.NoOfKebeliesWithWaterSupply;
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

                await _service.UpdateReportAsync(existingReport);
                TempData["Success"] = "Animal water supply status report updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }

        // GET: AnimalWaterSupplyStatus/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var report = await _service.GetReportByIdAsync(id);
            if (report == null)
                return NotFound();

            return View(report);
        }

        // POST: AnimalWaterSupplyStatus/Delete/{id}
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
    }
}