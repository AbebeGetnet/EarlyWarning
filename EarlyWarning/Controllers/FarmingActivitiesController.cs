using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using EarlyWarning.Services;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Controllers
{
    [Authorize]
    public class FarmingActivitiesController : Controller
    {
        private readonly FarmingActivityService _activityService;
        private readonly IFarmingActivityRepository _activityRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EarlyWarningDbContext _dbContext;

        public FarmingActivitiesController(FarmingActivityService activityService, 
            IFarmingActivityRepository activityRepository, 
            UserManager<ApplicationUser> userManager, 
            EarlyWarningDbContext dbContext)
        {
            _activityService = activityService;
            _activityRepository = activityRepository;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: FarmingActivities
        public async Task<IActionResult> Index()
        {
            var activities = await _activityService.GetAllActivitiesAsync();
            return View(activities);
        }

        // GET: FarmingActivities/Create
        public async Task<IActionResult> Create(RegistrationWithardViewModel model,DateTime SatrtDate, DateTime EndDate)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(RegistrationWithardViewModel model, DateTime StartDate, DateTime EndDate)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            var currentUser = await _userManager.GetUserAsync(User);

            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            if (model.WoredaId != woreda.Id)
            {
                ModelState.AddModelError("", "Invalid woreda selection.");
            }
         
            // Validate planned fields equal sum of other three attributes
            ValidateFarmingActivityPlanned(model.FarmingActivity);
             

            if (!ModelState.IsValid)
            {
                model.FarmingActivity.WoredaId = woreda.Id;
                model.FarmingActivity.UserId = currentUser.Id;
                model.FarmingActivity.StartDate = StartDate;
                model.FarmingActivity.EndDate = EndDate;
                model.FarmingActivity.Status = ReportStatus.Draft;

                await _activityService.CreateActivityAsync(model.FarmingActivity);
                //TempData["Success"] = "Farming activity created successfully.";
                return RedirectToAction("Create", "CropPestAndDeseasReports", new { model,model.StartDate, model.EndDate });
            }

            ViewBag.WoredaName = woreda.LocationName;
            return View(model.FarmingActivity);
        }

        // GET: FarmingActivities/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
                return NotFound();

            if (activity.Status != ReportStatus.Draft)
            {
                TempData["Error"] = "Only draft activities can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var woreda = await GetCurrentUserWoredaAsync();
            if (activity.WoredaId != woreda.Id)
            {
                TempData["Error"] = "You are not authorized to edit this activity.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda?.LocationName;
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FarmingActivity model)
        {
            if (id != model.Id)
                return BadRequest();

            var woreda = await GetCurrentUserWoredaAsync();
            if (model.WoredaId != woreda.Id)
            {
                ModelState.AddModelError("", "Invalid woreda selection.");
            }

            ValidateFarmingActivityPlanned(model);

            if (!ModelState.IsValid)
            {
                var existingActivity = await _activityService.GetActivityByIdAsync(id);
                if (existingActivity == null)
                    return NotFound();

                existingActivity.MeherFarmPlan = model.MeherFarmPlan;
                existingActivity.MeherPloughed = model.MeherPloughed;
                existingActivity.MeherSown = model.MeherSown;
                existingActivity.MeherHarvestingHHarvesting = model.MeherHarvestingHHarvesting;
                existingActivity.MeherSownWithResidualMoisture = model.MeherSownWithResidualMoisture;

                existingActivity.AutumnFarmPlan = model.AutumnFarmPlan;
                existingActivity.AutumnPloughed = model.AutumnPloughed;
                existingActivity.AutumnSown = model.AutumnSown;
                existingActivity.AutumnHarvestingHHarvesting = model.AutumnHarvestingHHarvesting;
                existingActivity.AutumnSownWithResidualMoisture = model.AutumnSownWithResidualMoisture;
                existingActivity.Remarks = model.Remarks;

                await _activityService.UpdateActivityAsync(existingActivity);
                TempData["Success"] = "Farming activity updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda?.LocationName;
            return View(model);
        }

        // GET: FarmingActivities/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null) 
                return NotFound();

            return View(activity);
        }

        // POST: FarmingActivities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _activityService.DeleteActivityAsync(id);
                TempData["Success"] = "Activity deleted successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Submit (change status to Submitted)
        [HttpPost]
        public async Task<IActionResult> Submit(Guid id)
        {
            try
            {
                await _activityService.SubmitActivityAsync(id);
                TempData["Success"] = "Activity submitted for zone approval.";
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
                await _activityService.ApproveByZoneAsync(id);
                TempData["Success"] = "Activity approved by zone.";
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
                await _activityService.ApproveByRegionAsync(id);
                TempData["Success"] = "Activity approved by region.";
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
            if (id == Guid.Empty)
            {
                TempData["Error"] = "Invalid activity ID.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }

            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
            {
                TempData["Error"] = "Activity not found.";
                return RedirectToAction(nameof(Index));
            }

            if (activity.Status != ReportStatus.Submitted)
            {
                TempData["Error"] = "Only submitted activities can be rejected by zone.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            activity.Status = ReportStatus.Rejected;
            activity.ZoneRejectionRemark = rejectionRemark;
            activity.ZoneRejectedAt = DateTime.UtcNow;
            activity.ZoneRejectedById = currentUser.Id;

            await _activityService.UpdateActivityAsync(activity);
            TempData["Success"] = "Activity rejected at zone level.";
            return RedirectToAction(nameof(Index));
        }

        // POST: RejectByRegion
        [Authorize(Roles = "RegionAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByRegion(Guid id, string rejectionRemark)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = "Invalid activity ID.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }

            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
            {
                TempData["Error"] = "Activity not found.";
                return RedirectToAction(nameof(Index));
            }

            if (activity.Status != ReportStatus.ZoneApproved)
            {
                TempData["Error"] = "Only zone-approved activities can be rejected by region.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            activity.Status = ReportStatus.Rejected;
            activity.RegionRejectionRemark = rejectionRemark;
            activity.RegionRejectedAt = DateTime.UtcNow;
            activity.RegionRejectedById = currentUser.Id;

            await _activityService.UpdateActivityAsync(activity);
            TempData["Success"] = "Activity rejected at region level.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidateFarmingActivityPlanned(FarmingActivity activity)
        {
            // Meher validation: MeherFarmPlan should equal sum of Ploughed, Sown, Harvesting, SownWithResidualMoisture
            float meherSum = activity.MeherPloughed + activity.MeherSown + activity.MeherHarvestingHHarvesting + activity.MeherSownWithResidualMoisture;
            if (Math.Abs(activity.MeherFarmPlan - meherSum) > 0.01) // Allow small floating point differences
            {
                ModelState.AddModelError("MeherFarmPlan", 
                    $"Meher Farm Plan must equal the sum of Ploughed ({activity.MeherPloughed}) + Sown ({activity.MeherSown}) + Harvesting ({activity.MeherHarvestingHHarvesting}) + Sown with Residual Moisture ({activity.MeherSownWithResidualMoisture}). Expected: {meherSum}");
            }

            // Autumn validation: AutumnFarmPlan should equal sum of Ploughed, Sown, Harvesting, SownWithResidualMoisture
            float autumnSum = activity.AutumnPloughed + activity.AutumnSown + activity.AutumnHarvestingHHarvesting + activity.AutumnSownWithResidualMoisture;
            if (Math.Abs(activity.AutumnFarmPlan - autumnSum) > 0.01) // Allow small floating point differences
            {
                ModelState.AddModelError("AutumnFarmPlan",
                    $"Autumn Farm Plan must equal the sum of Ploughed ({activity.AutumnPloughed}) + Sown ({activity.AutumnSown}) + Harvesting ({activity.AutumnHarvestingHHarvesting}) + Sown with Residual Moisture ({activity.AutumnSownWithResidualMoisture}). Expected: {autumnSum}");
            }
        }

        private async Task<Locations?> GetCurrentUserWoredaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var location = await _dbContext.Locations.FindAsync(user.LocationId);
            return location;
        }
    }
}
