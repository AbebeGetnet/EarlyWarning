using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using EarlyWarning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Controllers
{
    [Authorize]
    public class CropGrowthsController : Controller
    {
        private readonly CropGrowthService _cropGrowthService;
        private readonly ICropGrowthRepository _cropGrowthRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EarlyWarningDbContext _dbContext;

        public CropGrowthsController(CropGrowthService cropGrowthService,
            ICropGrowthRepository cropGrowthRepository,
            UserManager<ApplicationUser> userManager,
            EarlyWarningDbContext dbContext)
        {
            _cropGrowthService = cropGrowthService;
            _cropGrowthRepository = cropGrowthRepository;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: CropGrowths
        public async Task<IActionResult> Index()
        {
            var cropGrowths = await _cropGrowthService.GetAllCropGrowthsAsync();
            return View(cropGrowths);
        }

        // GET: CropGrowths/Create
        public async Task<IActionResult> Create()
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CropGrowth
            {
                WoredaId = woreda.Id
            };

            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CropGrowth model)
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            if (model.WoredaId != woreda.Id)
            {
                ModelState.AddModelError("", "Invalid woreda selection.");
            }

            if (!ModelState.IsValid)
            {
                model.WoredaId = woreda.Id;
                model.Status = ReportStatus.Draft;

                await _cropGrowthService.CreateCropGrowthAsync(model);
                TempData["Success"] = "Crop growth record created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda.LocationName;
            return View(model);
        }

        // GET: CropGrowths/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var cropGrowth = await _cropGrowthService.GetCropGrowthByIdAsync(id);
            if (cropGrowth == null)
                return NotFound();

            if (cropGrowth.Status != ReportStatus.Draft)
            {
                TempData["Error"] = "Only draft records can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var woreda = await GetCurrentUserWoredaAsync();
            if (cropGrowth.WoredaId != woreda.Id)
            {
                TempData["Error"] = "You are not authorized to edit this record.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda?.LocationName;
            return View(cropGrowth);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CropGrowth model)
        {
            if (id != model.Id)
                return BadRequest();

            var woreda = await GetCurrentUserWoredaAsync();
            if (model.WoredaId != woreda.Id)
            {
                ModelState.AddModelError("", "Invalid woreda selection.");
            }

            if (!ModelState.IsValid)
            {
                var existingCropGrowth = await _cropGrowthService.GetCropGrowthByIdAsync(id);
                if (existingCropGrowth == null)
                    return NotFound();

                existingCropGrowth.AtSeedStage = model.AtSeedStage;
                existingCropGrowth.AtGerminationStage = model.AtGerminationStage;
                existingCropGrowth.AtGrowthStage = model.AtGrowthStage;
                existingCropGrowth.AtFruitStage = model.AtFruitStage;
                existingCropGrowth.AtHarvestingStage = model.AtHarvestingStage;
                existingCropGrowth.Remarks = model.Remarks;

                await _cropGrowthService.UpdateCropGrowthAsync(existingCropGrowth);
                TempData["Success"] = "Crop growth record updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WoredaName = woreda?.LocationName;
            return View(model);
        }

        // GET: CropGrowths/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var cropGrowth = await _cropGrowthService.GetCropGrowthByIdAsync(id);
            if (cropGrowth == null)
                return NotFound();

            return View(cropGrowth);
        }

        // POST: CropGrowths/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _cropGrowthService.DeleteCropGrowthAsync(id);
                TempData["Success"] = "Crop growth record deleted successfully.";
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
                await _cropGrowthService.SubmitCropGrowthAsync(id);
                TempData["Success"] = "Crop growth record submitted for zone approval.";
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
                await _cropGrowthService.ApproveByZoneAsync(id);
                TempData["Success"] = "Crop growth record approved by zone.";
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
                await _cropGrowthService.ApproveByRegionAsync(id);
                TempData["Success"] = "Crop growth record approved by region.";
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
                TempData["Error"] = "Invalid record ID.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }

            var cropGrowth = await _cropGrowthService.GetCropGrowthByIdAsync(id);
            if (cropGrowth == null)
            {
                TempData["Error"] = "Crop growth record not found.";
                return RedirectToAction(nameof(Index));
            }

            if (cropGrowth.Status != ReportStatus.Submitted)
            {
                TempData["Error"] = "Only submitted records can be rejected by zone.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            cropGrowth.Status = ReportStatus.Rejected;
            cropGrowth.ZoneRejectionRemark = rejectionRemark;
            cropGrowth.ZoneRejectedAt = DateTime.UtcNow;
            cropGrowth.ZoneRejectedById = currentUser.Id;

            await _cropGrowthService.SaveCropGrowthAsync(cropGrowth);
            TempData["Success"] = "Crop growth record rejected at zone level.";
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
                TempData["Error"] = "Invalid record ID.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }

            var cropGrowth = await _cropGrowthService.GetCropGrowthByIdAsync(id);
            if (cropGrowth == null)
            {
                TempData["Error"] = "Crop growth record not found.";
                return RedirectToAction(nameof(Index));
            }

            if (cropGrowth.Status != ReportStatus.ZoneApproved)
            {
                TempData["Error"] = "Only zone-approved records can be rejected by region.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            cropGrowth.Status = ReportStatus.Rejected;
            cropGrowth.RegionRejectionRemark = rejectionRemark;
            cropGrowth.RegionRejectedAt = DateTime.UtcNow;
            cropGrowth.RegionRejectedById = currentUser.Id;

            await _cropGrowthService.SaveCropGrowthAsync(cropGrowth);
            TempData["Success"] = "Crop growth record rejected at region level.";
            return RedirectToAction(nameof(Index));
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
