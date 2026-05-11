using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using EarlyWarning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EarlyWarning.Controllers
{
    [Authorize]
    public class RainfallReportsController : Controller
    {
        private readonly RainfallReportService _reportService;
        private readonly IRainfallReportRepository _reportRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EarlyWarningDbContext _dbContext;

        public RainfallReportsController(RainfallReportService reportService, IRainfallReportRepository reportRepository, UserManager<ApplicationUser> userManager, EarlyWarningDbContext dbContext)
        {
            _reportService = reportService;
            _reportRepository = reportRepository;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: RainfallReports
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var reports = await _reportService.GetAllReportsAsync();
            return View(reports);
        }
      
        // GET: RainfallReports/Create
        public async Task<IActionResult> Create()
        {
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            // Count kebeles under this woreda (where Level = Kebele)
            int totalKebeles = await _dbContext.Locations
                .CountAsync(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive);

            var kebeles = await _dbContext.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();

            var droughtAffectedKebeles = await _dbContext.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();

            var model = new RainfallReport
            {
                WoredaId = woreda.Id,
                SelectedKebeleIds = new List<Guid>()
            };

            ViewBag.WoredaName = woreda.LocationName;
            ViewBag.TotalKebeles = totalKebeles;
            ViewBag.Kebeles = kebeles;
            ViewBag.DroughtAffectedKebeles = droughtAffectedKebeles;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RainfallReport model, List<Guid> SelectedKebeleIds,List<Guid> SelectedDroughtAffectedKebeleIds)
        {
            // 1. Get the current user's woreda
            var woreda = await GetCurrentUserWoredaAsync();
            if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            {
                TempData["Error"] = "Your user account is not linked to a valid woreda.";
                return RedirectToAction(nameof(Index));
            }

            // Ensure the submitted woreda ID matches the user's woreda (tamper prevention)
            if (model.WoredaId != woreda.Id)
            {
                ModelState.AddModelError("", "Invalid woreda selection.");
            }

            // 2. Validate date range (already done by model validation, but double‑check)
            if (model.StartDate.HasValue && model.EndDate.HasValue)
            {
                var daysDiff = (model.EndDate.Value - model.StartDate.Value).Days;
                if (daysDiff >= 8)
                {
                    ModelState.AddModelError("", "The date range must be less than 8 days (max 7 days).");
                }
            }

            // 3. Check for overlapping report with the same woreda
            if (!ModelState.IsValid && model.StartDate.HasValue && model.EndDate.HasValue)
            {
                var overlappingExists = await _reportRepository.ExistsOverlappingReportAsync(
                    model.WoredaId,
                    model.StartDate.Value,
                    model.EndDate.Value
                );
                if (overlappingExists)
                {
                    ModelState.AddModelError("", "ለዚህ ወረዳ እና የተመረጠ የዝናብ ቀን ክልል ሪፖርት ቀድሞ ተመዝግቧል። እባክዎ የተለየ ቀን ይምረጡ።");
                }
            }

            // 4. If model is valid, save the report
            if (!ModelState.IsValid)
            {
                // Assign the woreda ID (already set from the model)
                model.WoredaId = woreda.Id;

                // Save the selected kebele IDs as JSON
                model.SelectedKebeleIds = SelectedKebeleIds ?? new List<Guid>();
                model.SerializeKebeles();

                model.SelectedDroughtKebeleIds = SelectedDroughtAffectedKebeleIds ?? new List<Guid>();
                model.SerializeDroughtKebeles();
                // Set initial status (CommonAttribute already has Status = Draft as default)
                model.Status = ReportStatus.Draft;

                await _reportService.CreateReportAsync(model);
                TempData["Success"] = "Rainfall report created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // 5. If we get here, ModelState has errors – reload the view data
            // Reload total kebele count and kebele list for the woreda
            int totalKebeles = await _dbContext.Locations
                .CountAsync(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive);
            var kebeles = await _dbContext.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();

            ViewBag.WoredaName = woreda.LocationName;
            ViewBag.TotalKebeles = totalKebeles;
            ViewBag.Kebeles = kebeles;

            return View(model);
        }

        // GET: RainfallReports/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            // Get the report including the Woreda navigation property
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound();

            // Only draft reports can be edited
            if (report.Status != ReportStatus.Draft)
            {
                TempData["Error"] = "Only draft reports can be edited.";
                return RedirectToAction(nameof(Index));
            }

            // Get the current user's woreda
            var woreda = await GetCurrentUserWoredaAsync();
            //if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            //{
            //    TempData["Error"] = "Your user account is not linked to a valid woreda.";
            //    return RedirectToAction(nameof(Index));
            //}

            // Ensure the report belongs to the user's woreda (security)
            if (report.WoredaId != woreda.Id)
            {
                TempData["Error"] = "You are not authorized to edit this report.";
                return RedirectToAction(nameof(Index));
            }

            // Deserialize the stored JSON lists into the NotMapped properties
            report.DeserializeKebeles();         // Flood‑affected kebeles
            report.DeserializeDroughtKebeles();  // Drought‑affected kebeles

            // Total kebeles under this woreda
            int totalKebeles = await _dbContext.Locations
                .CountAsync(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive);

            // List of all kebeles (for checkboxes)
            var kebeles = await _dbContext.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();

            ViewBag.WoredaName = woreda.LocationName;
            ViewBag.TotalKebeles = totalKebeles;
            ViewBag.Kebeles = kebeles;
            ViewBag.DroughtAffectedKebeles = kebeles; // same list, but separate if needed

            return View(report);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RainfallReport model,
        List<Guid> SelectedKebeleIds, List<Guid> SelectedDroughtAffectedKebeleIds)
        {
            // Verify the ID matches
            if (id != model.Id)
                return BadRequest();

            // Get current user's woreda
            var woreda = await GetCurrentUserWoredaAsync();
            //if (woreda == null || woreda.Level != LocationLevel.ወረዳ)
            //{
            //    TempData["Error"] = "Your user account is not linked to a valid woreda.";
            //    return RedirectToAction(nameof(Index));
            //}

            // Ensure the report belongs to the user's woreda
            if (model.WoredaId != woreda.Id)
            {
                ModelState.AddModelError("", "Invalid woreda selection.");
            }

            // 1. Validate date range (max 7 days)
            if (model.StartDate.HasValue && model.EndDate.HasValue)
            {
                var daysDiff = (model.EndDate.Value - model.StartDate.Value).Days;
                if (daysDiff >= 8)
                {
                    ModelState.AddModelError("", "The date range must be less than 8 days (max 7 days).");
                }
            }

            // 2. Check for overlapping reports (excluding this one)
            if (!ModelState.IsValid && model.StartDate.HasValue && model.EndDate.HasValue)
            {
                var overlappingExists = await _reportRepository.ExistsOverlappingReportAsync(
                    model.WoredaId,
                    model.StartDate.Value,
                    model.EndDate.Value,
                    excludeReportId: model.Id  // exclude current report
                );
                if (overlappingExists)
                {
                    ModelState.AddModelError("", "ለዚህ ወረዳ እና የተመረጠ የዝናብ ቀን ክልል ሌላ ሪፖርት ቀድሞ ተመዝግቧል። እባክዎ የተለየ ቀን ይምረጡ።");
                }
            }

            // 3. If model is valid, update
            if (!ModelState.IsValid)
            {
                // Fetch the existing report from the database (to preserve audit fields)
                var existingReport = await _reportService.GetReportByIdAsync(id);
                if (existingReport == null)
                    return NotFound();

                // Update scalar properties
                existingReport.StartDate = model.StartDate;
                existingReport.EndDate = model.EndDate;
                existingReport.FullCoverageKebeles = model.FullCoverageKebeles;
                existingReport.PartialCoverageKebeles = model.PartialCoverageKebeles;
                existingReport.NoRainKebeles = model.NoRainKebeles;
                existingReport.NoDataKebeles = model.NoDataKebeles;
                existingReport.LowAmountKebeles = model.LowAmountKebeles;
                existingReport.LowMediumAmountKebeles = model.LowMediumAmountKebeles;
                existingReport.MediumAmountKebeles = model.MediumAmountKebeles;
                existingReport.MediumHighAmountKebeles = model.MediumHighAmountKebeles;
                existingReport.HighAmountKebeles = model.HighAmountKebeles;
                existingReport.Remarks = model.Remarks;
                // Do not change Status (remains whatever it was – must be Draft for Edit)

                // Update the JSON lists
                existingReport.SelectedKebeleIds = SelectedKebeleIds ?? new List<Guid>();
                existingReport.SerializeKebeles();          // flood list
                existingReport.SelectedDroughtKebeleIds = SelectedDroughtAffectedKebeleIds ?? new List<Guid>();
                existingReport.SerializeDroughtKebeles();   // drought list

                // Save
                await _reportService.UpdateReportAsync(existingReport);
                TempData["Success"] = "Rainfall report updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // 4. If we reach here, ModelState has errors – reload view data
            int totalKebeles = await _dbContext.Locations
                .CountAsync(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive);
            var kebeles = await _dbContext.Locations
                .Where(l => l.ParentId == woreda.Id && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .OrderBy(l => l.LocationName)
                .ToListAsync();

            ViewBag.WoredaName = woreda.LocationName;
            ViewBag.TotalKebeles = totalKebeles;
            ViewBag.Kebeles = kebeles;
            ViewBag.DroughtAffectedKebeles = kebeles;

            return View(model);
        }
        // GET: RainfallReports/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var report = await _reportService.GetReportByIdAsync(id);            
            if (report == null) return NotFound();

            // Deserialize the JSON list of kebele IDs
            report.DeserializeKebeles(); // uses your existing method

            //if (report.SelectedKebeleIds.Any())
            //{
            //    // Fetch kebele names from Locations table (only Name and AmharicName)
            //    var kebeleNames = await _dbContext.Locations
            //        .Where(l => report.SelectedKebeleIds.Contains(l.Id) && l.Level == LocationLevel.ቀበሌ)
            //        .Select(l => new { l.Id, l.LocationName, l.LocationAmharicName })
            //        .ToListAsync();
            //    ViewBag.KebeleNames = kebeleNames;
            //}
            //else
            //{
            //    ViewBag.KebeleNames = new List<object>();
            //}

            var kebeleNames = await _dbContext.Locations
            .Where(l => report.SelectedKebeleIds.Contains(l.Id) && l.Level == LocationLevel.ቀበሌ)
            .Select(l => new { l.Id, l.LocationName, l.LocationAmharicName })
            .ToListAsync();

            var droughtKebeleNames = await _dbContext.Locations
            .Where(l => report.SelectedKebeleIds.Contains(l.Id) && l.Level == LocationLevel.ቀበሌ)
            .Select(l => new { l.Id, l.LocationName, l.LocationAmharicName })
            .ToListAsync();

            ViewBag.KebeleNames = kebeleNames.Select(k => new
            {
                k.LocationName,
                k.LocationAmharicName
            }).ToList();

            ViewBag.DroughtKebeleNames = kebeleNames.Select(k => new
            {
                k.LocationName,
                k.LocationAmharicName
            }).ToList();

            return View(report);
        }

        // POST: RainfallReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _reportService.DeleteReportAsync(id);
                TempData["Success"] = "Deleted  successfully.";
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
                await _reportService.SubmitReportAsync(id);
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
                await _reportService.ApproveByZoneAsync(id);
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
                await _reportService.ApproveByRegionAsync(id);
                TempData["Success"] = "Report approved by region.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "ZoneAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByZone(Guid id, string rejectionRemark)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = "Invalid report ID.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(rejectionRemark))
            {
                TempData["Error"] = "Rejection remark is required.";
                return RedirectToAction(nameof(Index));
            }

            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
            {
                TempData["Error"] = "Report not found.";
                return RedirectToAction(nameof(Index));
            }

            if (report.Status != ReportStatus.Submitted)
            {
                TempData["Error"] = "Only submitted reports can be rejected by zone.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            report.Status = ReportStatus.Rejected;
            report.ZoneRejectionRemark = rejectionRemark;
            report.ZoneRejectedAt = DateTime.UtcNow;
            report.ZoneRejectedById = currentUser.Id;

            await _reportService.SaveReportAsync(report);
            TempData["Success"] = "Report rejected at zone level.";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "RegionAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByRegion(Guid id, string rejectionRemark)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null) return NotFound();

            if (report.Status != ReportStatus.ZoneApproved)
            {
                TempData["Error"] = "Only zone-approved reports can be rejected by region.";
                return RedirectToAction(nameof(Index));
            }

            report.Status = ReportStatus.Rejected;
            report.RegionRejectionRemark = rejectionRemark;
            report.RegionRejectedAt = DateTime.UtcNow;
            report.RegionRejectedById = (await _userManager.GetUserAsync(User)).Id;

            await _reportService.SaveReportAsync(report);
            TempData["Success"] = "Report rejected at region level.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetKebelesByWoreda(Guid woredaId)
        {
            var kebeles = await _dbContext.Locations
                .Where(l => l.ParentId == woredaId && l.Level == LocationLevel.ቀበሌ && l.IsActive)
                .Select(l => new { l.Id, l.LocationName, l.LocationAmharicName })
                .ToListAsync();
            return Ok(kebeles);
        }
        private async Task<Locations?> GetCurrentUserWoredaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var location = await _dbContext.Locations.FindAsync(user.LocationId);
            // Ensure it's a woreda (or you can validate)
            return location;
        }
    }
}