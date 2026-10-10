using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.Models.EpidemicDisease;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EarlyWarning.Controllers
{
    [Authorize]
    public class RegisterationWizardController : Controller
    {
        private readonly EarlyWarningDbContext _context; // Consider changing to your exact ApplicationDbContext type later
        private readonly UserManager<ApplicationUser> _userManager;
        public RegisterationWizardController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Display the Multi-Step Data Entry Form
        [HttpGet]
        public async Task<IActionResult> Index(Guid? woredaId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var targetWoredaId = woredaId ?? currentUser.LocationId;
            if (targetWoredaId != currentUser.LocationId)
            {
                var target = await _context.Locations.Include(x => x.Parent).AsNoTracking().FirstOrDefaultAsync(x => x.Id == targetWoredaId);
                var allowed = target != null && ((User.IsInRole("ZoneAdmin") && target.ParentId == currentUser.LocationId) ||
                    (User.IsInRole("RegionAdmin") && target.Parent?.ParentId == currentUser.LocationId));
                if (!allowed) return Forbid();
            }
            var kebeles = await _context.Locations
                .Where(l => l.ParentId == targetWoredaId &&
                            l.Level == LocationLevel.ቀበሌ &&
                            l.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var defaultWeekStart = GetReportingWeekStart(DateTime.Today);
            var selectedStartDate = startDate?.Date ?? defaultWeekStart;
            var selectedEndDate = endDate?.Date ?? selectedStartDate.AddDays(2);
            var model = new RegistrationWithardViewModel
            {
                WoredaId = targetWoredaId,
                UserId = currentUser.Id,
                StartDate = selectedStartDate,
                EndDate = selectedEndDate,
                RainfallReport = new RainfallReport(),
                FarmingActivity = new FarmingActivity(),
                CropGrowth = new CropGrowth(),
                CropPestAndDeseaseReport = new CropPestAndDeseaseReport(),
                PastureStatus = new PastureStatus(),
                AnimalWaterSupplyStatus = new AnimalWaterSupplyStatus(),
                AnimalHealthStatus = new AnimalHealthStatus(),
                EpidemicDisease = new EpidemicDisease(),
                GrainSupply = new Models.SupplyofGrain.GrainSupply(),
                AnimalSupply = new Models.AnimalSupply.AnimalSupply(),
                GrainPrice = new Models.PriceofGrain.GrainPrice(),
                AnimalPrice = new Models.AnimalPrice.AnimalPrice(),
                WeeklyAccidents = new Models.WeeklyAccidents.WeeklyAccidents(),
                Migration = new Models.Migration.Migration(),
                Death = new Models.Migration.Death(),
                AssistanceRecipient = new AssistanceRecipient(),
                WeeklyProvided = new WeeklyProvided(),
                OtherProblem = new OtherProblem(),
                HumanDrinkWaterIssue = new HumanDrinkWaterIssue(),
                Kebelies = kebeles,
                AnimalDiseases = await _context.AnimalDisease.AsNoTracking().ToListAsync(),
                CropPestAndDeseases = await _context.CropPestAndDesease.AsNoTracking().ToListAsync(),
                EpidemicDiseaseTypes = await _context.EpidemicDiseaseTypes.Where(x => x.IsActive).AsNoTracking().ToListAsync(),
                AccidentTypes = await _context.AccidentTypes.Where(x => x.IsActive).AsNoTracking().ToListAsync()
            };

            model.MigrationLocations = await _context.Locations.Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive && x.Id != targetWoredaId).AsNoTracking().ToListAsync();
            model.AnimalMigrationRows = BuildAnimalMigrationRows();

            model.GrainTypes = await _context.GrainTypes.Where(x => x.IsActive).AsNoTracking().ToListAsync();
            model.AnimalTypes = await _context.AnimalTypes.Where(x => x.IsActive).AsNoTracking().ToListAsync();
            model.SupplyTypes = await _context.SupplyTypes.Where(x => x.IsActive).AsNoTracking().ToListAsync();
            model.GrainPriceRows = model.GrainTypes.Select(x => new Models.PriceofGrain.GrainPricePerQuintal { GrainTypeId = x.GrainTypeId }).ToList();
            model.AnimalPriceRows = model.AnimalTypes.Select(x => new Models.AnimalPrice.AnimalPricePerUnit { AnimalTypeId = x.AnimalTypeId }).ToList();
            model.AidDistributionDetails = model.SupplyTypes.Select(x => new WeeklyProvidedDetail { SupplyTypeId = x.SupplyTypeId }).ToList();
            model.CropPestImpactDetails = model.CropPestAndDeseases.Select(x => new CropPestImpactDetail { CropPestAndDeseaseId = x.Id }).ToList();
            model.AnimalDiseaseImpactDetails = model.AnimalDiseases.Select(x => new AnimalDiseaseImpactDetail { AnimalDiseaseId = x.Id }).ToList();
            model.EpidemicImpactDetails = model.EpidemicDiseaseTypes.Select(x => new EpidemicDiseaseSelection { EpidemicDiseaseTypeId = x.EpidemicDiseaseTypeId }).ToList();
            model.AccidentDetails = model.AccidentTypes.Select(x => new Models.WeeklyAccidents.AccidentDetail { AccidentTypeId = x.AccidentTypeId }).ToList();
            await PopulatePreviousPricesAsync(model, model.WoredaId, model.StartDate!.Value);

            var initialWeekStart = GetReportingWeekStart(model.StartDate.Value);
            var initialSubmission = await _context.WeeklyReportSubmissions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.WoredaId == targetWoredaId && x.ReportingWeekStart == initialWeekStart);
            if (initialSubmission != null)
                initialSubmission.Status = await NormalizeOrphanedSubmissionAsync(initialSubmission.Id, initialSubmission.Status);
            var isZoneReview = User.IsInRole("ZoneAdmin") && targetWoredaId != currentUser.LocationId;
            if (isZoneReview && (initialSubmission == null || initialSubmission.Status == WeeklySubmissionStatus.Draft))
                return NotFound();
            ViewBag.IsReviewMode = isZoneReview;
            ViewBag.ReviewSubmission = initialSubmission;
            ViewBag.CanZoneDecision = isZoneReview && initialSubmission?.Status is
                WeeklySubmissionStatus.SubmittedToZone or WeeklySubmissionStatus.RegionRejectedToZone;
            ViewBag.InitialDuplicateMessage = initialSubmission == null ? null
                : initialSubmission.Status == WeeklySubmissionStatus.Draft
                    ? "የዚህ ሳምንት ረቂቅ መረጃ አለ፤ ከቆሙበት ቀጥለው ማጠናቀቅ ይችላሉ።"
                : initialSubmission.Status == WeeklySubmissionStatus.ZoneRejectedToWoreda
                    ? "የዚህ ሳምንት ሪፖርት ለማስተካከያ ተመልሷል፤ ነባሩን መዝገብ ያርሙና እንደገና ይላኩ።"
                    : "ለዚህ ወረዳና የሪፖርት ሳምንት መረጃ ቀድሞ ተመዝግቧል፤ አዲስ መዝገብ መፍጠር አይቻልም።";

            ViewBag.TotalKebeles = kebeles.Count;
            ViewBag.WoredaName = await _context.Locations.Where(x => x.Id == targetWoredaId)
                .Select(x => x.LocationAmharicName ?? x.LocationName ?? "-").FirstOrDefaultAsync();
            return View(model);        
        }

        [HttpGet]
        public async Task<IActionResult> CheckWeeklySubmission(DateTime startDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            if (!User.IsInRole("Data Encoder") || currentUser.UserLocationLevl != LocationLevel.ወረዳ) return Forbid();
            var weekStart = GetReportingWeekStart(startDate.Date);
            var periodEnd = weekStart.AddDays(2);
            var submission = await _context.WeeklyReportSubmissions.AsNoTracking()
                .Where(x => x.WoredaId == currentUser.LocationId && x.ReportingWeekStart == weekStart)
                .Select(x => new
                {
                    x.Id, x.Status, x.StartDate, x.EndDate,
                    Woreda = x.Woreda.LocationAmharicName ?? x.Woreda.LocationName ?? "-"
                }).FirstOrDefaultAsync();
            if (submission == null) return Json(new { exists = false, weekStart });
            var status = await NormalizeOrphanedSubmissionAsync(submission.Id, submission.Status);
            var editable = status is WeeklySubmissionStatus.Draft or WeeklySubmissionStatus.ZoneRejectedToWoreda;
            return Json(new
            {
                exists = true, editable, status, weekStart, periodEnd,
                message = status == WeeklySubmissionStatus.Draft
                    ? "የዚህ ሳምንት ረቂቅ መረጃ ተቀምጧል፤ ከቆሙበት ቀጥለው ያጠናቅቁ።"
                    : editable ? "የዚህ ሳምንት ሪፖርት ለማስተካከያ ተመልሷል፤ ነባሩን መረጃ ያርሙና እንደገና ይላኩ።"
                    : $"ለ{submission.Woreda} የ{weekStart:yyyy-MM-dd} እስከ {periodEnd:yyyy-MM-dd} (ሰኞ–ረቡዕ) ሪፖርት ቀድሞ ተልኳል (ሁኔታ፡ {status})። ነባሩን ሪፖርት ለግምገማ ይክፈቱ።"
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            if (!User.IsInRole("Data Encoder") || currentUser.UserLocationLevl != LocationLevel.ወረዳ) return Forbid();
            var form = await Request.ReadFormAsync();
            if (!DateTime.TryParse(form[nameof(RegistrationWithardViewModel.StartDate)].FirstOrDefault(), out var startDate) ||
                !DateTime.TryParse(form[nameof(RegistrationWithardViewModel.EndDate)].FirstOrDefault(), out var endDate) || startDate > endDate)
                return BadRequest(new { message = "ትክክለኛ የሪፖርት ቀን ያስፈልጋል።" });
            var weekStart = GetReportingWeekStart(startDate.Date);
            if (startDate.Date != weekStart || endDate.Date != weekStart.AddDays(2))
                return BadRequest(new { message = "የሪፖርት ጊዜው ከሰኞ እስከ ረቡዕ መሆን አለበት።" });
            var submission = await _context.WeeklyReportSubmissions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.WoredaId == currentUser.LocationId && x.ReportingWeekStart == weekStart);
            if (submission != null)
                submission.Status = await NormalizeOrphanedSubmissionAsync(submission.Id, submission.Status);
            if (submission != null && submission.Status is not (WeeklySubmissionStatus.Draft or WeeklySubmissionStatus.ZoneRejectedToWoreda))
                return Conflict(new { message = "የዚህ ሳምንት ሪፖርት ቀድሞ ተልኳል።" });
            var now = DateTime.UtcNow;
            var fields = form.Where(x => x.Key != "__RequestVerificationToken")
                .ToDictionary(x => x.Key, x => x.Value.ToArray());
            var draftJson = JsonSerializer.Serialize(fields);

            if (submission != null)
            {
                // Do not use a tracked row-version update for autosave. Multiple
                // browser requests may overlap even after AbortController.abort().
                // The status predicate also prevents a late autosave from changing
                // a report after the final submission has locked it.
                var affected = await _context.WeeklyReportSubmissions
                    .Where(x => x.Id == submission.Id &&
                        (x.Status == WeeklySubmissionStatus.Draft || x.Status == WeeklySubmissionStatus.ZoneRejectedToWoreda))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.StartDate, startDate.Date)
                        .SetProperty(x => x.EndDate, endDate.Date)
                        .SetProperty(x => x.LastActionAt, now)
                        .SetProperty(x => x.DraftDataJson, draftJson));

                if (affected == 0)
                    return Conflict(new { message = "የዚህ ሳምንት ሪፖርት ቀድሞ ተልኳል ወይም ተሰርዟል።" });

                return Json(new { saved = true, submission.Id, savedAt = now });
            }

            submission = new WeeklyReportSubmission
            {
                WoredaId = currentUser.LocationId, StartDate = startDate.Date, EndDate = endDate.Date,
                ReportingWeekStart = weekStart, Status = WeeklySubmissionStatus.Draft,
                CreatedById = currentUser.Id, CreatedAt = now, LastActionAt = now,
                DraftDataJson = draftJson
            };
            _context.WeeklyReportSubmissions.Add(submission);
            _context.WeeklyReportWorkflowHistories.Add(new WeeklyReportWorkflowHistory
            {
                WeeklyReportSubmissionId = submission.Id, UserId = currentUser.Id, ActorLocationId = currentUser.LocationId,
                ActorLocationLevel = currentUser.UserLocationLevl, Action = WeeklyWorkflowAction.DraftSaved,
                PreviousStatus = null, NewStatus = WeeklySubmissionStatus.Draft, Comment = "ረቂቅ መረጃ ተጀምሯል።", PerformedAt = now
            });
            await _context.SaveChangesAsync();
            return Json(new { saved = true, submission.Id, savedAt = now });
        }

        [HttpGet]
        public async Task<IActionResult> GetExistingWizardData(Guid woredaId, DateTime startDate, DateTime endDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var target = await _context.Locations.Include(x => x.Parent).AsNoTracking().FirstOrDefaultAsync(x => x.Id == woredaId);
            var allowed = (User.IsInRole("Data Encoder") && currentUser.LocationId == woredaId) ||
                (User.IsInRole("ZoneAdmin") && target?.ParentId == currentUser.LocationId) ||
                (User.IsInRole("RegionAdmin") && target?.Parent?.ParentId == currentUser.LocationId);
            if (!allowed) return Forbid();
            var requestedStartDate = startDate.Date;
            var requestedEndDate = endDate.Date;
            var reportingWeekStart = GetReportingWeekStart(requestedStartDate);
            var workflow = await _context.WeeklyReportSubmissions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.ReportingWeekStart == reportingWeekStart);
            // A reporting week is unique even when the user reopens it using a
            // different date inside that week. Always hydrate using the dates
            // stored on the existing record so its domain rows are found.
            var sDate = workflow?.StartDate.Date ?? requestedStartDate;
            var eDate = workflow?.EndDate.Date ?? requestedEndDate;
            var latestWorkflowComment = workflow == null ? null : await _context.WeeklyReportWorkflowHistories.AsNoTracking()
                .Where(x => x.WeeklyReportSubmissionId == workflow.Id && x.Comment != null)
                .OrderByDescending(x => x.PerformedAt).Select(x => x.Comment).FirstOrDefaultAsync();

            // A draft has not populated the individual CRUD tables yet. Return
            // its form snapshot directly instead of running the submitted-report
            // graph loader, which is only relevant after final submission.
            if (workflow?.Status == WeeklySubmissionStatus.Draft && !string.IsNullOrWhiteSpace(workflow.DraftDataJson))
            {
                Dictionary<string, string[]> draftFields;
                try
                {
                    draftFields = JsonSerializer.Deserialize<Dictionary<string, string[]>>(workflow.DraftDataJson) ?? [];
                }
                catch (JsonException)
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity,
                        new { message = "የተቀመጠው ረቂቅ መረጃ ቅርጸት ተበላሽቷል።" });
                }

                return Json(new
                {
                    exists = true,
                    canEdit = User.IsInRole("Data Encoder"),
                    workflowStatus = workflow.Status.ToString(),
                    recordStartDate = sDate.ToString("yyyy-MM-dd"),
                    recordEndDate = eDate.ToString("yyyy-MM-dd"),
                    draftFields,
                    latestWorkflowComment
                });
            }

            // 1. Query all 6 independent domain tables concurrently
            var rainfall = await _context.RainfallReports.AsNoTracking()
                .FirstOrDefaultAsync(r => r.WoredaId == woredaId && r.StartDate == sDate && r.EndDate == eDate);
            rainfall?.DeserializeKebeles();
            rainfall?.DeserializeDroughtKebeles();

            var farming = await _context.FarmingActivities.AsNoTracking()
                .FirstOrDefaultAsync(f => f.WoredaId == woredaId && f.StartDate == sDate && f.EndDate == eDate);
            var cropGrowth = await _context.CropGrowths.AsNoTracking()
                .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);

            var pest = await _context.CropPestAndDeseaseReports.AsNoTracking().Include(x => x.ImpactDetails)
                .FirstOrDefaultAsync(p => p.WoredaId == woredaId && p.StartDate == sDate && p.EndDate == eDate);

            var pasture = await _context.PastureStatuses.AsNoTracking()
                .FirstOrDefaultAsync(p => p.WoredaId == woredaId && p.StartDate == sDate && p.EndDate == eDate);

            var water = await _context.AnimalWaterSupplyStatuses.AsNoTracking()
                .FirstOrDefaultAsync(w => w.WoredaId == woredaId && w.StartDate == sDate && w.EndDate == eDate);

            var health = await _context.AnimalHealthStatuses.AsNoTracking().Include(x => x.ImpactDetails)
                .FirstOrDefaultAsync(h => h.WoredaId == woredaId && h.StartDate == sDate && h.EndDate == eDate);

            var humanWater = await _context.HumanDrinkWaterIssues.Include(x => x.HumanDrinkWaterKebeles).FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var epidemic = await _context.EpidemicDiseases.Include(x => x.EpidemicDiseaseSelections).FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var grainSupply = await _context.GrainSupplys.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var animalSupply = await _context.AnimalSupplys.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var grainPrice = await _context.GrainPrices.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var animalPrice = await _context.AnimalPrice.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var accidents = await _context.WeeklyAccidents.Include(x => x.AccidentDetails).FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var migration = await _context.Migrations.Include(x => x.MigrationDetails).FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var death = await _context.Deaths.Include(x => x.DeathDetails).FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var recipient = await _context.AssistanceRecipients.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var aid = await _context.WeeklyProvideds.Include(x => x.WeeklyProvidedDetail).FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var other = await _context.OtherProblems.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var school = await _context.SchoolReports.AsNoTracking().FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate);
            var grainPriceRows = await _context.GrainPricePerQuintals.Where(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate).ToListAsync();
            var animalPriceRows = await _context.AnimalPricePerUnits.Where(x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate).ToListAsync();
            var animalMigrations = workflow == null ? [] : await _context.AnimalMigrationRecords.AsNoTracking()
                .Where(x => x.WeeklyReportSubmissionId == workflow.Id).ToListAsync();

            // 2. Check if data exists in ANY of the domains
            bool dataExists = (rainfall != null || farming != null || cropGrowth != null || pest != null ||
                               pasture != null || water != null || health != null || humanWater != null || epidemic != null ||
                               grainSupply != null || animalSupply != null || grainPrice != null || animalPrice != null || accidents != null ||
                               migration != null || death != null || recipient != null || aid != null || other != null || school != null);

            var extendedFields = new Dictionary<string, object?>();
            AddScalarFields(extendedFields, "HumanDrinkWaterIssue", humanWater);
            AddScalarFields(extendedFields, "EpidemicDisease", epidemic);
            AddScalarFields(extendedFields, "GrainSupply", grainSupply);
            AddScalarFields(extendedFields, "AnimalSupply", animalSupply);
            AddScalarFields(extendedFields, "GrainPrice", grainPrice);
            AddScalarFields(extendedFields, "AnimalPrice", animalPrice);
            AddScalarFields(extendedFields, "WeeklyAccidents", accidents);
            AddScalarFields(extendedFields, "AccidentDetail", accidents?.AccidentDetails?.FirstOrDefault());
            AddScalarFields(extendedFields, "Migration", migration);
            AddScalarFields(extendedFields, "MigrationDetail", migration?.MigrationDetails?.FirstOrDefault());
            AddScalarFields(extendedFields, "Death", death);
            AddScalarFields(extendedFields, "DeathDetail", death?.DeathDetails?.FirstOrDefault());
            AddScalarFields(extendedFields, "AssistanceRecipient", recipient);
            AddScalarFields(extendedFields, "WeeklyProvided", aid);
            AddScalarFields(extendedFields, "OtherProblem", other);
            AddScalarFields(extendedFields, "SchoolReport", school);

            if (!dataExists && workflow == null)
            {
                return Json(new { exists = false });
            }

            //// 3. Fetch relational many-to-many join lists if parent entities exist
            //var kebeleIds = rainfall != null
            //    ? await _context.ReportKebelies.Where(k => k.RainfallReportId == rainfall.Id).Select(k => k.KebeleId).ToListAsync()
            //    : new List<Guid>();

            //var pestIds = pest != null
            //    ? await _context.ReportPests.Where(p => p.CropPestReportId == pest.Id).Select(p => p.PestId).ToListAsync()
            //    : new List<int>();

            //var diseaseIds = health != null
            //    ? await _context.AnimalDisease.Where(d => d.Id == health.Id).Select(d => d.Id).ToListAsync()
            //    : new List<int>();

            // 4. Map the independent layers into a unified payload for the Wizard script
            return Json(new
            {
                exists = true,
                canEdit = User.IsInRole("Data Encoder") && (workflow == null || workflow.Status is WeeklySubmissionStatus.Draft or WeeklySubmissionStatus.ZoneRejectedToWoreda),
                workflowStatus = workflow?.Status.ToString(),
                recordStartDate = sDate.ToString("yyyy-MM-dd"),
                recordEndDate = eDate.ToString("yyyy-MM-dd"),
                draftFields = string.IsNullOrWhiteSpace(workflow?.DraftDataJson)
                    ? null : JsonSerializer.Deserialize<Dictionary<string, string[]>>(workflow.DraftDataJson),
                latestWorkflowComment,
                // Since there's no single parent ID, we pass a truthy state flag to JavaScript
                id = 1,
                remarks = pest?.Remarks ?? pasture?.Remarks ?? health?.Remarks ?? "", // Fallback remark locator
                extendedFields,
                selectedHumanWaterKebeleIds = humanWater?.HumanDrinkWaterKebeles?.Select(x => x.KebeleId),
                selectedEpidemicDiseaseTypeIds = epidemic?.EpidemicDiseaseSelections?.Select(x => x.EpidemicDiseaseTypeId),
                selectedAccidentTypeIds = accidents?.AccidentDetails?.Select(x => x.AccidentTypeId),
                grainPriceRows = grainPriceRows.Select(x => new { x.GrainTypeId, PreviousWeekPrice = x.PriceDifference.HasValue ? x.WeeklyPrice - x.PriceDifference.Value : (decimal?)null, x.WeeklyPrice, x.WeeklyMarketStatus, x.PriceDifference, x.PriceChangePercentage, x.Notes }),
                animalPriceRows = animalPriceRows.Select(x => new { x.AnimalTypeId, PreviousWeekPrice = x.PriceDifference.HasValue ? x.WeeklyPrice - x.PriceDifference.Value : (decimal?)null, x.WeeklyPrice, x.WeeklyMarketStatus, x.PriceDifference, x.PriceChangePercentage, x.Notes }),
                aidDistributionDetails = aid?.WeeklyProvidedDetail?.Select(x => new { x.SupplyTypeId, x.Measurement, x.ProvidedQuantity, x.DistributedQuantity, x.PercentageOfDistributedFromProvided, x.Donor }),
                cropPestImpactDetails = pest?.ImpactDetails,
                animalDiseaseImpactDetails = health?.ImpactDetails,
                epidemicImpactDetails = epidemic?.EpidemicDiseaseSelections,
                accidentImpactDetails = accidents?.AccidentDetails,
                animalMigrationRows = animalMigrations.Select(x => new
                {
                    animalType = x.AnimalType.ToString(),
                    x.OtherAnimalType,
                    x.NumberOfAnimals
                }),
                animalMigrationDirection = animalMigrations.FirstOrDefault()?.Direction.ToString(),
                inwardAnimalMigrationOriginId = animalMigrations.FirstOrDefault(x => x.Direction == AnimalMigrationDirection.Inward)?.OriginLocationId,
                outwardAnimalMigrationDestinationId = animalMigrations.FirstOrDefault(x => x.Direction == AnimalMigrationDirection.Outward)?.DestinationLocationId,

                // Table 1: Rainfall
                rainfall = rainfall == null ? null : new
                {
                    full = rainfall.FullCoverageKebeles,
                    partial = rainfall.PartialCoverageKebeles,
                    none = rainfall.NoRainKebeles,
                    noData = rainfall.NoDataKebeles,
                    high = rainfall.HighAmountKebeles,
                    amountNoData = rainfall.NoDataForRainAmountKebeles,
                    medium = rainfall.MediumAmountKebeles,
                    mediumHigh = rainfall.MediumHighAmountKebeles,
                    low = rainfall.LowAmountKebeles,
                    lowMedium = rainfall.LowMediumAmountKebeles,
                    kebeleIds = rainfall.SelectedKebeleIds,
                    droughtKebeleIds = rainfall.SelectedDroughtKebeleIds
                },

                cropGrowth = cropGrowth == null ? null : new
                {
                    seed = cropGrowth.AtSeedStage,
                    germination = cropGrowth.AtGerminationStage,
                    growth = cropGrowth.AtGrowthStage,
                    fruit = cropGrowth.AtFruitStage,
                    harvesting = cropGrowth.AtHarvestingStage
                },

                // Table 2: Farming Activity
                farming = farming == null ? null : new
                {
                    meherPlan = farming.MeherFarmPlan,
                    meherPloughed = farming.MeherPloughed,
                    meherSown = farming.MeherSown,
                    meherHarvesting = farming.MeherHarvestingHHarvesting,
                    meherResidual = farming.MeherSownWithResidualMoisture,
                    autumnPlan = farming.AutumnFarmPlan,
                    autumnPloughed = farming.AutumnPloughed,
                    autumnSown = farming.AutumnSown,
                    autumnHarvesting = farming.AutumnHarvestingHHarvesting,
                    autumnResidual = farming.AutumnSownWithResidualMoisture
                },

                // Table 3: Crop Pest & Disease
                pest = pest == null ? null : new
                {
                    hasOccured = pest.HasPestAndDeseasOccured,
                    hectars = pest.AffectedLandInHectar,
                    cropType = pest.TypeOfCropAffected,
                    pestIds = pest.ImpactDetails.Select(x => x.CropPestAndDeseaseId),
                    mHH = pest.MaleHouseHold,
                    fHH = pest.FemaleHouseHold,
                    mFam = pest.MaleFamily,
                    fFam = pest.FemaleFamily,
                    mChild = pest.ChildhMale,
                    fChild = pest.ChildFemale,
                    mYouth = pest.YouthMale,
                    fYouth = pest.YouthFemale,
                    mEld = pest.ElderlyMale,
                    fEld = pest.ElderlyFemale,
                    mDis = pest.DisabledMale,
                    fDis = pest.DisabledFemale
                },

                // Table 4: Pasture Status
                pasture = pasture == null ? null : new
                {
                    enough = pasture.Enough,
                    animalsAffected = pasture.NumberOfAnimalsAffected,
                    mHH = pasture.MaleHouseHold,
                    fHH = pasture.FemaleHouseHold,
                    mFam = pasture.MaleFamily,
                    fFam = pasture.FemaleFamily,
                    mChild = pasture.ChildhMale,
                    fChild = pasture.ChildFemale,
                    mYouth = pasture.YouthMale,
                    fYouth = pasture.YouthFemale,
                    mEld = pasture.ElderlyMale,
                    fEld = pasture.ElderlyFemale,
                    mDis = pasture.DisabledMale,
                    fDis = pasture.DisabledFemale
                },

                // Table 5: Animal Water Supply
                water = water == null ? null : new
                {
                    enough = water.Enough,
                    kebelesCount = water.NoOfKebeliesWithWaterSupply,
                    pastureKebelesCount = water.NoOfKebeliesWithPastureShortage,
                    bodyCondition = water.AnimalBodyCondition?.ToString(),
                    bodyConditionReason = water.LowBodyConditionReason,
                    bodyConditionAnimalsAffected = water.LowBodyConditionAnimalsAffected,
                    mHH = water.MaleHouseHold,
                    fHH = water.FemaleHouseHold,
                    mFam = water.MaleFamily,
                    fFam = water.FemaleFamily,
                    mChild = water.ChildhMale,
                    fChild = water.ChildFemale,
                    mYouth = water.YouthMale,
                    fYouth = water.YouthFemale,
                    mEld = water.ElderlyMale,
                    fEld = water.ElderlyFemale,
                    mDis = water.DisabledMale,
                    fDis = water.DisabledFemale
                },

                // Table 6: Animal Health
                health = health == null ? null : new
                {
                    enough = health.Enough,
                    infectedMale = health.NoOfKebeliesWithWaterSupply,
                    diseaseType = health.DiseaseType,
                    diseaseIds = health.ImpactDetails.Select(x => x.AnimalDiseaseId),
                    mHH = health.MaleHouseHold,
                    fHH = health.FemaleHouseHold,
                    mFam = health.MaleFamily,
                    fFam = health.FemaleFamily,
                    mChild = health.ChildhMale,
                    fChild = health.ChildFemale,
                    mYouth = health.YouthMale,
                    fYouth = health.YouthFemale,
                    mEld = health.ElderlyMale,
                    fEld = health.ElderlyFemale,
                    mDis = health.DisabledMale,
                    fDis = health.DisabledFemale
                }
            });
        }
        private static void AddScalarFields(Dictionary<string, object?> fields, string prefix, object? source)
        {
            if (source == null) return;
            foreach (var property in source.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(Guid) || type == typeof(DateTime))
                    fields[$"{prefix}.{property.Name}"] = property.GetValue(source);
            }
        }
        // Process the Consolidated Multi-Step Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWizard(RegistrationWithardViewModel model, List<Guid> SelectedKebeliesIds, List<Guid> SelectedDroughtKebeleIds, List<Guid> SelectedCropDiseaseIds, List<Guid> SelectedAnimalDiseaseIds)
        {
            //if (!ModelState.IsValid)
            //{
            //    // Repopulate ViewBags/SelectLists if validation fails to safely redisplay the form
            //    ViewBag.WoredaSelectList = new SelectList(_context.Locations, "Id", "LocationName", model.WoredaId);
            //    return View("Index", model);
            //}

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            if (!User.IsInRole("Data Encoder") || currentUser.UserLocationLevl != LocationLevel.ወረዳ) return Forbid();
            var woredaId = currentUser.LocationId;
            model.WoredaId = woredaId;
            var sDate = model.StartDate;
            var eDate = model.EndDate;
            model.SelectedCropDiseaseIds = SelectedCropDiseaseIds ?? [];
            model.SelectedAnimalDiseaseIds = SelectedAnimalDiseaseIds ?? [];
            // Child CRUD entities receive shared date/woreda metadata below, so
            // validate the consolidated workflow rules instead of their standalone form metadata.
            var bindingErrors = ModelState
                .Where(x => x.Value?.Errors.Any(e => e.Exception != null || e.ErrorMessage.Contains("not valid", StringComparison.OrdinalIgnoreCase)) == true)
                .Select(x => x.Key).ToList();
            var postedValues = ModelState.Where(x => x.Value?.RawValue != null)
                .ToDictionary(x => x.Key, x => (x.Value!.RawValue, x.Value.AttemptedValue));
            ModelState.Clear();
            foreach (var posted in postedValues)
            {
                ModelState.SetModelValue(posted.Key, posted.Value.RawValue, posted.Value.AttemptedValue);
                // SetModelValue restores the attempted value as Unvalidated. Leaving it
                // in that state makes ModelState.IsValid false even when no error exists.
                ModelState.MarkFieldValid(posted.Key);
            }
            foreach (var field in bindingErrors)
                ModelState.AddModelError(field, "ያስገቡት ቁጥር ከተፈቀደው መጠን በላይ ነው ወይም ትክክለኛ ቁጥር አይደለም።");

            ValidateNumericValues(model.RainfallReport, "የዝናብ መረጃ");
            ValidateNumericValues(model.FarmingActivity, "የእርሻ መረጃ");
            ValidateNumericValues(model.CropGrowth, "የሰብል እድገት ሁኔታ");
            ValidateNumericValues(model.CropPestAndDeseaseReport, "የሰብል ተባይ/በሽታ");
            ValidateNumericValues(model.PastureStatus, "የመኖ መረጃ");
            ValidateNumericValues(model.AnimalWaterSupplyStatus, "የእንስሳት ውሃ");
            ValidateNumericValues(model.AnimalHealthStatus, "የእንስሳት ጤና");
            ValidateNumericValues(model.HumanDrinkWaterIssue, "የሰው መጠጥ ውሃ");
            ValidateNumericValues(model.EpidemicDisease, "የሰው ጤና ወረርሽኝ");
            ValidateNumericValues(model.GrainSupply, "የእህል አቅርቦት");
            ValidateNumericValues(model.AnimalSupply, "የእንስሳት አቅርቦት");
            ValidateNumericValues(model.GrainPrice, "የእህል ዋጋ");
            ValidateNumericValues(model.AnimalPrice, "የእንስሳት ዋጋ");
            ValidateNumericValues(model.WeeklyAccidents, "የሳምንቱ አደጋ");
            ValidateNumericValues(model.Migration, "ስደት");
            ValidateNumericValues(model.Death, "ሞት");
            ValidateNumericValues(model.AssistanceRecipient, "የዕርዳታ ተጠቃሚ");
            ValidateNumericValues(model.WeeklyProvided, "የቀረበ ዕርዳታ");
            ValidateNumericValues(model.OtherProblem, "ሌላ ችግር");
            ValidateNumericValues(model.SchoolReport, "የትምህርት ቤት ሁኔታ");
            foreach (var row in model.CropPestImpactDetails) ValidateNumericValues(row, "የሰብል ተጎጂ ዝርዝር");
            foreach (var row in model.AnimalDiseaseImpactDetails) ValidateNumericValues(row, "የእንስሳት በሽታ ተጎጂ ዝርዝር");
            foreach (var row in model.EpidemicImpactDetails) ValidateNumericValues(row, "የሰው ጤና ተጎጂ ዝርዝር");
            var selectedAccidentDetails = model.AccidentDetails
                .Where(x => model.SelectedAccidentTypeIds.Contains(x.AccidentTypeId))
                .ToList();
            foreach (var row in selectedAccidentDetails) ValidateNumericValues(row, "የአደጋ ዝርዝር");
            foreach (var row in model.MigrationDetails) ValidateNumericValues(row, "የስደት ዝርዝር");
            foreach (var row in model.DeathDetails) ValidateNumericValues(row, "የሞት ዝርዝር");
            foreach (var row in model.GrainPriceRows) ValidateNumericValues(row, "የእህል ዋጋ");
            foreach (var row in model.AnimalPriceRows) ValidateNumericValues(row, "የእንስሳት ዋጋ");
            foreach (var row in model.AidDistributionDetails) ValidateNumericValues(row, "የዕርዳታ ዝርዝር");
            if (selectedAccidentDetails.Any(x => x.DamageRateInPercent is < 0 or > 100))
                ModelState.AddModelError(nameof(model.AccidentDetails), "የጉዳት መጠን (%) ከ0 እስከ 100 ብቻ መሆን አለበት።");

            if (woredaId == Guid.Empty) ModelState.AddModelError(nameof(model.WoredaId), "ወረዳ ያስፈልጋል።");
            if (!sDate.HasValue || !eDate.HasValue || sDate > eDate) ModelState.AddModelError(nameof(model.StartDate), "ትክክለኛ የሪፖርት ቀን ክልል ያስፈልጋል።");
            var reportingWeekStart = sDate.HasValue ? GetReportingWeekStart(sDate.Value) : (DateTime?)null;
            if (sDate.HasValue && eDate.HasValue &&
                (sDate.Value.Date != reportingWeekStart!.Value || eDate.Value.Date != reportingWeekStart.Value.AddDays(2)))
                ModelState.AddModelError(nameof(model.EndDate), "የሪፖርት ጊዜው ከሰኞ እስከ ረቡዕ መሆን አለበት።");
            var workflow = reportingWeekStart.HasValue
                ? await _context.WeeklyReportSubmissions.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.ReportingWeekStart == reportingWeekStart.Value)
                : null;
            if (workflow != null)
                workflow.Status = await NormalizeOrphanedSubmissionAsync(workflow.Id, workflow.Status);
            if (workflow != null && workflow.Status is not (WeeklySubmissionStatus.Draft or WeeklySubmissionStatus.ZoneRejectedToWoreda))
                ModelState.AddModelError(string.Empty, "ይህ ሪፖርት ተልኳል እና ተቆልፏል። የዞን አስተያየት ካልመለሰው በስተቀር ማስተካከል አይቻልም።");
            if (model.HumanDrinkWaterIssue.HasWaterProblem && model.SelectedHumanWaterKebeleIds.Count != model.HumanDrinkWaterIssue.NumberOfAffectedKebeles)
                ModelState.AddModelError(nameof(model.SelectedHumanWaterKebeleIds), "የተመረጡ ቀበሌዎች ብዛት ከተመዘገበው ቁጥር ጋር መመሳሰል አለበት።");
            var woredaKebeleIds = await _context.Locations
                .Where(x => x.ParentId == woredaId && x.Level == LocationLevel.ቀበሌ && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();
            var woredaKebeleCount = woredaKebeleIds.Count;
            var rainfallKebeleCounts = new Dictionary<string, int>
            {
                ["RainfallReport.FullCoverageKebeles"] = model.RainfallReport.FullCoverageKebeles,
                ["RainfallReport.PartialCoverageKebeles"] = model.RainfallReport.PartialCoverageKebeles,
                ["RainfallReport.NoRainKebeles"] = model.RainfallReport.NoRainKebeles,
                ["RainfallReport.NoDataKebeles"] = model.RainfallReport.NoDataKebeles,
                ["RainfallReport.HighAmountKebeles"] = model.RainfallReport.HighAmountKebeles,
                ["RainfallReport.NoDataForRainAmountKebeles"] = model.RainfallReport.NoDataForRainAmountKebeles,
                ["RainfallReport.MediumAmountKebeles"] = model.RainfallReport.MediumAmountKebeles,
                ["RainfallReport.MediumHighAmountKebeles"] = model.RainfallReport.MediumHighAmountKebeles,
                ["RainfallReport.LowAmountKebeles"] = model.RainfallReport.LowAmountKebeles,
                ["RainfallReport.LowMediumAmountKebeles"] = model.RainfallReport.LowMediumAmountKebeles
            };
            foreach (var rainfallCount in rainfallKebeleCounts.Where(x => x.Value > woredaKebeleCount))
                ModelState.AddModelError(rainfallCount.Key,
                    $"የቀበሌዎች ቁጥር ከወረዳው ጠቅላላ {woredaKebeleCount} ቀበሌዎች ሊበልጥ አይችልም።");
            if ((SelectedKebeliesIds ?? []).Any(id => !woredaKebeleIds.Contains(id)) ||
                (SelectedDroughtKebeleIds ?? []).Any(id => !woredaKebeleIds.Contains(id)) ||
                model.SelectedHumanWaterKebeleIds.Any(id => !woredaKebeleIds.Contains(id)))
                ModelState.AddModelError(nameof(model.Kebelies), "ለዚህ ወረዳ ያልተመደበ ቀበሌ ተመርጧል።");
            if (model.HumanDrinkWaterIssue.NumberOfAffectedKebeles > woredaKebeleCount)
                ModelState.AddModelError("HumanDrinkWaterIssue.NumberOfAffectedKebeles", $"የቀበሌዎች ቁጥር ከወረዳው ጠቅላላ {woredaKebeleCount} ቀበሌዎች ሊበልጥ አይችልም።");
            if (model.AnimalWaterSupplyStatus.NoOfKebeliesWithWaterSupply > woredaKebeleCount)
                ModelState.AddModelError("AnimalWaterSupplyStatus.NoOfKebeliesWithWaterSupply", $"የቀበሌዎች ቁጥር ከወረዳው ጠቅላላ {woredaKebeleCount} ቀበሌዎች ሊበልጥ አይችልም።");
            if (model.AnimalWaterSupplyStatus.NoOfKebeliesWithPastureShortage > woredaKebeleCount)
                ModelState.AddModelError("AnimalWaterSupplyStatus.NoOfKebeliesWithPastureShortage", $"የቀበሌዎች ቁጥር ከወረዳው ጠቅላላ {woredaKebeleCount} ቀበሌዎች ሊበልጥ አይችልም።");
            if (model.AnimalWaterSupplyStatus.AnimalBodyCondition == AnimalBodyCondition.ዝቅተኛ && string.IsNullOrWhiteSpace(model.AnimalWaterSupplyStatus.LowBodyConditionReason))
                ModelState.AddModelError("AnimalWaterSupplyStatus.LowBodyConditionReason", "የእንስሳት አቋም ዝቅተኛ ከሆነ ምክንያቱን ይግለጹ።");
            if (model.EpidemicDisease.NumberOfAffectedKebeles > woredaKebeleCount)
                ModelState.AddModelError("EpidemicDisease.NumberOfAffectedKebeles", $"የቀበሌዎች ቁጥር ከወረዳው ጠቅላላ {woredaKebeleCount} ቀበሌዎች ሊበልጥ አይችልም።");
            if (model.EpidemicDisease.HasEpidemicDisease && model.SelectedEpidemicDiseaseTypeIds.Count == 0)
                ModelState.AddModelError(nameof(model.SelectedEpidemicDiseaseTypeIds), "ቢያንስ አንድ የሰው በሽታ ይምረጡ።");
            if (model.WeeklyAccidents.HasAccident && model.SelectedAccidentTypeIds.Count == 0)
                ModelState.AddModelError(nameof(model.SelectedAccidentTypeIds), "ቢያንስ አንድ የአደጋ አይነት ይምረጡ።");
            if (model.Migration.HasMigration && model.MigrationDetail.IsInward && model.MigrationDetail.OriginLocationId == null)
                ModelState.AddModelError("MigrationDetail.OriginLocationId", "ወደ ወረዳው ለገባ ስደት መነሻ ወረዳ ያስፈልጋል።");
            if (model.Migration.HasMigration && !model.MigrationDetail.IsInward && model.MigrationDetail.DestinationLocationId == null)
                ModelState.AddModelError("MigrationDetail.DestinationLocationId", "ከወረዳው ለወጣ ስደት መድረሻ ወረዳ ያስፈልጋል።");
            if (model.AidDistributionDetails.Any(x => x.DistributedQuantity > x.ProvidedQuantity))
                ModelState.AddModelError(nameof(model.AidDistributionDetails), "የተሰራጨው መጠን ከቀረበው መጠን ሊበልጥ አይችልም።");
            var inwardAnimalRows = model.AnimalMigrationDirection == AnimalMigrationDirection.Inward
                ? model.AnimalMigrationRows.Where(x => x.NumberOfAnimals > 0).ToList() : [];
            var outwardAnimalRows = model.AnimalMigrationDirection == AnimalMigrationDirection.Outward
                ? model.AnimalMigrationRows.Where(x => x.NumberOfAnimals > 0).ToList() : [];
            if (inwardAnimalRows.Count > 0 && model.InwardAnimalMigrationOriginId == null)
                ModelState.AddModelError(nameof(model.InwardAnimalMigrationOriginId), "ወደ ወረዳው ለገቡ እንስሳት መነሻ ወረዳ ይምረጡ።");
            if (outwardAnimalRows.Count > 0 && model.OutwardAnimalMigrationDestinationId == null)
                ModelState.AddModelError(nameof(model.OutwardAnimalMigrationDestinationId), "ከወረዳው ለወጡ እንስሳት መድረሻ ወረዳ ይምረጡ።");
            if (model.AnimalMigrationRows.Any(x => x.NumberOfAnimals < 0 || x.NumberOfAnimals > 1_000_000_000))
                ModelState.AddModelError(nameof(model.AnimalMigrationRows), "የእንስሳት ብዛት በ0 እና 1,000,000,000 መካከል መሆን አለበት።");
            if (model.AnimalMigrationRows.Any(x => x.AnimalType == MigratingAnimalType.Other && x.NumberOfAnimals > 0 && string.IsNullOrWhiteSpace(x.OtherAnimalType)))
                ModelState.AddModelError(nameof(model.AnimalMigrationRows), "'Other' ሲመረጥ የእንስሳውን አይነት ይጻፉ።");
            if (!ModelState.IsValid)
            {
                ViewBag.SubmissionFailed = true;
                var submissionErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(error =>
                    {
                        var message = !string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? error.ErrorMessage
                            : error.Exception?.GetBaseException().Message;
                        if (string.IsNullOrWhiteSpace(message))
                            message = "የገባው መረጃ ትክክለኛ አይደለም።";
                        return string.IsNullOrWhiteSpace(x.Key) ? message : $"{x.Key}: {message}";
                    }))
                    .ToList();
                if (submissionErrors.Count == 0)
                    submissionErrors.Add("ያልታወቀ የማረጋገጫ ስህተት ተፈጥሯል። እባክዎ የገጹን መረጃ ያረጋግጡ።");
                ViewBag.SubmissionErrors = submissionErrors;
                await PopulateLookupListsAsync(model);
                ViewBag.TotalKebeles = model.Kebelies.Count;
                return View("Index", model);
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // ----------------------------------------------------
            // 1. DOMAIN LAYER: RAINFALL REPORT (UPSERT)
            // ----------------------------------------------------
            var existingRain = await _context.RainfallReports
                .FirstOrDefaultAsync(r => r.WoredaId == woredaId && r.StartDate == sDate && r.EndDate == eDate);

            if (existingRain != null)
            {
                existingRain.FullCoverageKebeles = model.RainfallReport.FullCoverageKebeles;
                existingRain.PartialCoverageKebeles = model.RainfallReport.PartialCoverageKebeles;
                existingRain.NoRainKebeles = model.RainfallReport.NoRainKebeles;
                existingRain.NoDataKebeles = model.RainfallReport.NoDataKebeles;
                existingRain.HighAmountKebeles = model.RainfallReport.HighAmountKebeles;
                existingRain.NoDataForRainAmountKebeles = model.RainfallReport.NoDataForRainAmountKebeles;
                existingRain.MediumAmountKebeles = model.RainfallReport.MediumAmountKebeles;
                existingRain.MediumHighAmountKebeles = model.RainfallReport.MediumHighAmountKebeles;
                existingRain.LowAmountKebeles = model.RainfallReport.LowAmountKebeles;
                existingRain.LowMediumAmountKebeles = model.RainfallReport.LowMediumAmountKebeles;
                existingRain.SelectedKebeleIds = SelectedKebeliesIds ?? [];
                existingRain.SerializeKebeles();
                existingRain.SelectedDroughtKebeleIds = SelectedDroughtKebeleIds ?? [];
                existingRain.SerializeDroughtKebeles();
            }
            else
            {
                model.RainfallReport.WoredaId = woredaId;
                model.RainfallReport.StartDate = sDate;
                model.RainfallReport.EndDate = eDate;
                model.RainfallReport.SelectedKebeleIds = SelectedKebeliesIds ?? [];
                model.RainfallReport.SerializeKebeles();
                model.RainfallReport.SelectedDroughtKebeleIds = SelectedDroughtKebeleIds ?? [];
                model.RainfallReport.SerializeDroughtKebeles();
                _context.RainfallReports.Add(model.RainfallReport);
            }

            // ----------------------------------------------------
            // 2. DOMAIN LAYER: FARMING ACTIVITY (UPSERT)
            // ----------------------------------------------------
            var existingFarm = await _context.FarmingActivities
                .FirstOrDefaultAsync(f => f.WoredaId == woredaId && f.StartDate == sDate && f.EndDate == eDate);

            if (existingFarm != null)
            {
                existingFarm.MeherFarmPlan = model.FarmingActivity.MeherFarmPlan;
                existingFarm.MeherPloughed = model.FarmingActivity.MeherPloughed;
                existingFarm.MeherSown = model.FarmingActivity.MeherSown;
                existingFarm.MeherHarvestingHHarvesting = model.FarmingActivity.MeherHarvestingHHarvesting;
                existingFarm.MeherSownWithResidualMoisture = model.FarmingActivity.MeherSownWithResidualMoisture;

                existingFarm.AutumnFarmPlan = model.FarmingActivity.AutumnFarmPlan;
                existingFarm.AutumnPloughed = model.FarmingActivity.AutumnPloughed;
                existingFarm.AutumnSown = model.FarmingActivity.AutumnSown;
                existingFarm.AutumnHarvestingHHarvesting = model.FarmingActivity.AutumnHarvestingHHarvesting;
                existingFarm.AutumnSownWithResidualMoisture = model.FarmingActivity.AutumnSownWithResidualMoisture;

            }
            else
            {
                model.FarmingActivity.WoredaId = woredaId;
                model.FarmingActivity.StartDate = sDate;
                model.FarmingActivity.EndDate = eDate;
                _context.FarmingActivities.Add(model.FarmingActivity);
            }

            await UpsertSimpleAsync(_context.CropGrowths, model.CropGrowth,
                x => x.WoredaId == woredaId && x.StartDate == sDate && x.EndDate == eDate,
                x => { x.WoredaId = woredaId; x.StartDate = sDate; x.EndDate = eDate; });

            // ----------------------------------------------------
            // 3. DOMAIN LAYER: CROP PEST AND DISEASE REPORT (UPSERT)
            // ----------------------------------------------------
            var existingPest = await _context.CropPestAndDeseaseReports
                .FirstOrDefaultAsync(p => p.WoredaId == woredaId && p.StartDate == sDate && p.EndDate == eDate);

            if (existingPest != null)
            {
                existingPest.HasPestAndDeseasOccured = model.CropPestAndDeseaseReport.HasPestAndDeseasOccured;
                existingPest.AffectedLandInHectar = model.CropPestAndDeseaseReport.AffectedLandInHectar;
                existingPest.TypeOfCropAffected = model.CropPestAndDeseaseReport.TypeOfCropAffected;
                existingPest.Remarks = model.Remarks; // Aggregate overall remarks value directly down to segments

                // Demographic Columns Map Injection Updates
                MapDemographicProperties(existingPest, model.CropPestAndDeseaseReport);


                await _context.CropPestImpactDetails
                    .Where(x => x.CropPestAndDeseaseReportId == existingPest.Id)
                    .ExecuteDeleteAsync();
                existingPest.ImpactDetails = BuildCropImpactDetails(model, existingPest.Id);
            }
            else
            {
                model.CropPestAndDeseaseReport.WoredaId = woredaId;
                model.CropPestAndDeseaseReport.StartDate = sDate;
                model.CropPestAndDeseaseReport.EndDate = eDate;
                model.CropPestAndDeseaseReport.Remarks = model.Remarks;
                _context.CropPestAndDeseaseReports.Add(model.CropPestAndDeseaseReport);
                model.CropPestAndDeseaseReport.ImpactDetails = BuildCropImpactDetails(model, model.CropPestAndDeseaseReport.Id);
            }

            // ----------------------------------------------------
            // 4. DOMAIN LAYER: PASTURE STATUS REPORT (UPSERT)
            // ----------------------------------------------------
            var existingPasture = await _context.PastureStatuses
                .FirstOrDefaultAsync(p => p.WoredaId == woredaId && p.StartDate == sDate && p.EndDate == eDate);

            if (existingPasture != null)
            {
                existingPasture.Enough = model.PastureStatus.Enough;
                existingPasture.NumberOfAnimalsAffected = model.PastureStatus.NumberOfAnimalsAffected;
                existingPasture.Remarks = model.Remarks;

                MapDemographicProperties(existingPasture, model.PastureStatus);
            }
            else
            {
                model.PastureStatus.WoredaId = woredaId;
                model.PastureStatus.StartDate = sDate;
                model.PastureStatus.EndDate = eDate;
                model.PastureStatus.Remarks = model.Remarks;
                _context.PastureStatuses.Add(model.PastureStatus);
            }

            // ----------------------------------------------------
            // 5. DOMAIN LAYER: ANIMAL WATER SUPPLY STATUS (UPSERT)
            // ----------------------------------------------------
            // Affected-animal count only applies when body condition is low.
            if (model.AnimalWaterSupplyStatus.AnimalBodyCondition != AnimalBodyCondition.ዝቅተኛ)
                model.AnimalWaterSupplyStatus.LowBodyConditionAnimalsAffected = 0;

            var existingWater = await _context.AnimalWaterSupplyStatuses
                .FirstOrDefaultAsync(w => w.WoredaId == woredaId && w.StartDate == sDate && w.EndDate == eDate);

            if (existingWater != null)
            {
                existingWater.Enough = model.AnimalWaterSupplyStatus.Enough;
                existingWater.NoOfKebeliesWithWaterSupply = model.AnimalWaterSupplyStatus.NoOfKebeliesWithWaterSupply;
                existingWater.NoOfKebeliesWithPastureShortage = model.AnimalWaterSupplyStatus.NoOfKebeliesWithPastureShortage;
                existingWater.AnimalBodyCondition = model.AnimalWaterSupplyStatus.AnimalBodyCondition;
                existingWater.LowBodyConditionReason = model.AnimalWaterSupplyStatus.LowBodyConditionReason;
                existingWater.LowBodyConditionAnimalsAffected = model.AnimalWaterSupplyStatus.LowBodyConditionAnimalsAffected;

                MapDemographicProperties(existingWater, model.AnimalWaterSupplyStatus);
            }
            else
            {
                model.AnimalWaterSupplyStatus.WoredaId = woredaId;
                model.AnimalWaterSupplyStatus.StartDate = sDate;
                model.AnimalWaterSupplyStatus.EndDate = eDate;
                _context.AnimalWaterSupplyStatuses.Add(model.AnimalWaterSupplyStatus);
            }

            // ----------------------------------------------------
            // 6. DOMAIN LAYER: ANIMAL HEALTH STATUS REPORT (UPSERT)
            // ----------------------------------------------------
            var existingHealth = await _context.AnimalHealthStatuses
                .FirstOrDefaultAsync(h => h.WoredaId == woredaId && h.StartDate == sDate && h.EndDate == eDate);

            if (existingHealth != null)
            {
                existingHealth.Enough = model.AnimalHealthStatus.Enough;
                existingHealth.NoOfKebeliesWithWaterSupply = model.AnimalHealthStatus.NoOfKebeliesWithWaterSupply;
                existingHealth.DiseaseType = model.AnimalHealthStatus.DiseaseType;

                MapDemographicProperties(existingHealth, model.AnimalHealthStatus);

                await _context.AnimalDiseaseImpactDetails
                    .Where(x => x.AnimalHealthStatusId == existingHealth.Id)
                    .ExecuteDeleteAsync();
                existingHealth.ImpactDetails = BuildAnimalImpactDetails(model, existingHealth.Id);
            }
            else
            {
                model.AnimalHealthStatus.WoredaId = woredaId;
                model.AnimalHealthStatus.StartDate = sDate;
                model.AnimalHealthStatus.EndDate = eDate;
                _context.AnimalHealthStatuses.Add(model.AnimalHealthStatus);
                model.AnimalHealthStatus.ImpactDetails = BuildAnimalImpactDetails(model, model.AnimalHealthStatus.Id);
            }

            // Final Single Unit-of-Work Database Commit Transaction
            await UpsertExtendedReportsAsync(model, woredaId, sDate!.Value, eDate!.Value);
            var now = DateTime.UtcNow;
            var action = workflow == null || workflow.Status == WeeklySubmissionStatus.Draft ? WeeklyWorkflowAction.Submit : WeeklyWorkflowAction.Resubmit;
            var previousStatus = workflow?.Status;
            if (workflow == null)
            {
                workflow = new WeeklyReportSubmission
                {
                    WoredaId = woredaId, StartDate = sDate.Value.Date, EndDate = eDate.Value.Date, ReportingWeekStart = reportingWeekStart!.Value,
                    Status = WeeklySubmissionStatus.SubmittedToZone, CreatedById = currentUser.Id,
                    CreatedAt = now, LastActionAt = now
                };
                _context.WeeklyReportSubmissions.Add(workflow);
            }
            else
            {
                // Atomically lock the workflow before saving the rest of the
                // submission. An autosave may have advanced RowVersion since this
                // request loaded the row; a tracked update would then throw a
                // DbUpdateConcurrencyException during the final SaveChanges.
                var workflowId = workflow.Id;
                _context.Entry(workflow).State = EntityState.Detached;
                var affected = await _context.WeeklyReportSubmissions
                    .Where(x => x.Id == workflowId &&
                        (x.Status == WeeklySubmissionStatus.Draft || x.Status == WeeklySubmissionStatus.ZoneRejectedToWoreda))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Status, WeeklySubmissionStatus.SubmittedToZone)
                        .SetProperty(x => x.DraftDataJson, (string?)null)
                        .SetProperty(x => x.LastActionAt, now));
                if (affected == 0)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "ይህ ሪፖርት በሌላ ጥያቄ ተቀይሯል ወይም ቀድሞ ተልኳል። እባክዎ ገጹን እንደገና ይክፈቱ።");
                    await PopulateLookupListsAsync(model);
                    ViewBag.TotalKebeles = model.Kebelies.Count;
                    return View("Index", model);
                }
            }
            _context.WeeklyReportWorkflowHistories.Add(new WeeklyReportWorkflowHistory
            {
                WeeklyReportSubmissionId = workflow.Id, UserId = currentUser.Id,
                ActorLocationId = currentUser.LocationId, ActorLocationLevel = currentUser.UserLocationLevl,
                Action = action, PreviousStatus = previousStatus, NewStatus = WeeklySubmissionStatus.SubmittedToZone,
                Comment = model.Remarks, PerformedAt = now
            });
            await _context.AnimalMigrationRecords
                .Where(x => x.WeeklyReportSubmissionId == workflow.Id)
                .ExecuteDeleteAsync();
            foreach (var row in model.AnimalMigrationRows.Where(x => x.NumberOfAnimals > 0))
            {
                var inward = model.AnimalMigrationDirection == AnimalMigrationDirection.Inward;
                _context.AnimalMigrationRecords.Add(new AnimalMigrationRecord
                {
                    WeeklyReportSubmissionId = workflow.Id, WoredaId = woredaId, Direction = model.AnimalMigrationDirection,
                    OriginLocationId = inward ? model.InwardAnimalMigrationOriginId!.Value : woredaId,
                    DestinationLocationId = inward ? woredaId : model.OutwardAnimalMigrationDestinationId!.Value,
                    AnimalType = row.AnimalType, OtherAnimalType = row.AnimalType == MigratingAnimalType.Other ? row.OtherAnimalType?.Trim() : null,
                    NumberOfAnimals = row.NumberOfAnimals
                });
            }
            try
            {
                const int maxSaveAttempts = 3;
                for (var attempt = 1; attempt <= maxSaveAttempts; attempt++)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        break;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var canRetry = true;
                        foreach (var entry in ex.Entries)
                        {
                            var databaseValues = await entry.GetDatabaseValuesAsync();
                            if (databaseValues != null)
                            {
                                // Preserve this submission's values, but use the
                                // latest database values for concurrency checks.
                                entry.OriginalValues.SetValues(databaseValues);
                            }
                            else if (entry.State == EntityState.Deleted)
                            {
                                // ExecuteDelete already removed this child row.
                                // Detach EF's duplicate tracked delete.
                                entry.State = EntityState.Detached;
                            }
                            else
                            {
                                // A row that this request intended to update was
                                // genuinely removed. Retrying would lose data.
                                canRetry = false;
                            }
                        }

                        if (canRetry && attempt < maxSaveAttempts)
                            continue;

                        await transaction.RollbackAsync();
                        var entityNames = string.Join(", ", ex.Entries
                            .Select(x => x.Metadata.ClrType.Name).Distinct());
                        ModelState.AddModelError(string.Empty,
                            $"መረጃው በሌላ ጥያቄ ተቀይሯል ({entityNames})። እባክዎ ገጹን እንደገና ይክፈቱና ይሞክሩ።");
                        await PopulateLookupListsAsync(model);
                        ViewBag.TotalKebeles = model.Kebelies.Count;
                        return View("Index", model);
                    }
                }
                await transaction.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_WeeklyReportSubmissions_WoredaId_ReportingWeekStart", StringComparison.OrdinalIgnoreCase) == true)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "ለዚህ ወረዳና የሪፖርት ሳምንት መረጃ ቀድሞ ተመዝግቧል። ድጋሚ መመዝገብ አይቻልም።");
                await PopulateLookupListsAsync(model);
                ViewBag.TotalKebeles = model.Kebelies.Count;
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "የሳምንቱ መረጃ ለዞን ግምገማ ተልኳል እና ተቆልፏል።";
            return RedirectToAction("Index");
        }

        private static DateTime GetReportingWeekStart(DateTime date)
        {
            var value = date.Date;
            var daysSinceMonday = ((int)value.DayOfWeek + 6) % 7;
            return value.AddDays(-daysSinceMonday);
        }

        /// <summary>
        /// Repairs workflow rows that carry a locked status without a corresponding
        /// submit/resubmit event. These rows can be left by older code or failed
        /// submissions and must remain editable instead of falsely blocking a week.
        /// </summary>
        private async Task<WeeklySubmissionStatus> NormalizeOrphanedSubmissionAsync(
            Guid submissionId, WeeklySubmissionStatus status)
        {
            if (status is WeeklySubmissionStatus.Draft or WeeklySubmissionStatus.ZoneRejectedToWoreda)
                return status;

            var wasActuallySubmitted = await _context.WeeklyReportWorkflowHistories.AsNoTracking()
                .AnyAsync(x => x.WeeklyReportSubmissionId == submissionId &&
                    (x.Action == WeeklyWorkflowAction.Submit || x.Action == WeeklyWorkflowAction.Resubmit));
            if (wasActuallySubmitted)
                return status;

            await _context.WeeklyReportSubmissions
                .Where(x => x.Id == submissionId && x.Status == status)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, WeeklySubmissionStatus.Draft)
                    .SetProperty(x => x.LastActionAt, DateTime.UtcNow));

            return WeeklySubmissionStatus.Draft;
        }

        private void ValidateNumericValues(object? source, string section)
        {
            if (source == null) return;
            foreach (var property in source.GetType().GetProperties().Where(x => x.CanRead && x.GetIndexParameters().Length == 0))
            {
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var value = property.GetValue(source);
                if (value == null) continue;
                if (type == typeof(int) || type == typeof(long))
                {
                    var number = Convert.ToInt64(value);
                    if (number < 0 || number > 1_000_000_000)
                        ModelState.AddModelError(property.Name, $"{section}፡ {property.Name} በ0 እና 1,000,000,000 መካከል መሆን አለበት።");
                }
                else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
                {
                    decimal number;
                    try { number = Convert.ToDecimal(value); }
                    catch (OverflowException) { ModelState.AddModelError(property.Name, $"{section}፡ {property.Name} ቁጥሩ በጣም ትልቅ ነው።"); continue; }
                    // Price differences and percentage changes are derived values;
                    // a price decrease is correctly represented by a negative number.
                    var allowsNegative = property.Name is nameof(Models.PriceofGrain.GrainPricePerQuintal.PriceDifference)
                        or nameof(Models.PriceofGrain.GrainPricePerQuintal.PriceChangePercentage);
                    if ((!allowsNegative && number < 0) || number < -999_999_999_999.99m || number > 999_999_999_999.99m)
                        ModelState.AddModelError(property.Name, $"{section}፡ {property.Name} ትክክለኛና በተፈቀደው ወሰን ውስጥ መሆን አለበት።");
                }
            }
        }

        // Reflection-free Demographic Mapping Helper to reduce boilerplate code duplicate footprint
        private void MapDemographicProperties(dynamic target, dynamic source)
        {
            target.MaleHouseHold = source.MaleHouseHold;
            target.FemaleHouseHold = source.FemaleHouseHold;
            target.MaleFamily = source.MaleFamily;
            target.FemaleFamily = source.FemaleFamily;
            target.ChildhMale = source.ChildhMale;
            target.ChildFemale = source.ChildFemale;
            target.YouthMale = source.YouthMale;
            target.YouthFemale = source.YouthFemale;
            target.ElderlyMale = source.ElderlyMale;
            target.ElderlyFemale = source.ElderlyFemale;
            target.DisabledMale = source.DisabledMale;
            target.DisabledFemale = source.DisabledFemale;
        }

        private async Task UpsertExtendedReportsAsync(
            RegistrationWithardViewModel model, Guid woredaId, DateTime startDate, DateTime endDate)
        {
            var reportedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;

            model.HumanDrinkWaterIssue.WoredaId = woredaId;
            model.HumanDrinkWaterIssue.StartDate = startDate;
            model.HumanDrinkWaterIssue.EndDate = endDate;
            model.HumanDrinkWaterIssue.ReportedBy = reportedBy;
            var humanWater = await _context.HumanDrinkWaterIssues
                .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate);
            if (humanWater == null) { humanWater = model.HumanDrinkWaterIssue; _context.HumanDrinkWaterIssues.Add(humanWater); }
            else
            {
                _context.Entry(humanWater).CurrentValues.SetValues(model.HumanDrinkWaterIssue);
                await _context.HumanDrinkWaterKebeles.Where(x => x.HumanDrinkWaterIssueId == humanWater.HumanDrinkWaterIssueId).ExecuteDeleteAsync();
            }
            if (humanWater.HasWaterProblem)
                humanWater.HumanDrinkWaterKebeles = model.SelectedHumanWaterKebeleIds.Distinct().Select(id => new HumanDrinkWaterKebele { KebeleId = id }).ToList();

            model.EpidemicDisease.WoredaId = woredaId;
            model.EpidemicDisease.StartDate = startDate;
            model.EpidemicDisease.EndDate = endDate;
            model.EpidemicDisease.ReportedBy = reportedBy;
            var epidemic = await _context.EpidemicDiseases
                .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate);
            if (epidemic == null)
            {
                epidemic = model.EpidemicDisease;
                _context.EpidemicDiseases.Add(epidemic);
            }
            else
            {
                _context.Entry(epidemic).CurrentValues.SetValues(model.EpidemicDisease);
                await _context.EpidemicDiseaseSelections.Where(x => x.EpidemicDiseaseId == epidemic.EpidemicDiseaseId).ExecuteDeleteAsync();
            }
            if (epidemic.HasEpidemicDisease)
            {
                epidemic.EpidemicDiseaseSelections = model.EpidemicImpactDetails
                    .Where(x => model.SelectedEpidemicDiseaseTypeIds.Contains(x.EpidemicDiseaseTypeId))
                    .Select(x => { x.EpidemicDiseaseSelectionId = Guid.NewGuid(); return x; }).ToList();
            }

            await UpsertSimpleAsync(_context.GrainSupplys, model.GrainSupply, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x => { x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate; x.ReportedBy = reportedBy; });
            await UpsertSimpleAsync(_context.AnimalSupplys, model.AnimalSupply, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x => { x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate; x.ReportedBy = reportedBy; });
            await UpsertSimpleAsync(_context.GrainPrices, model.GrainPrice, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x => { x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate; x.ReportedBy = reportedBy; });
            await UpsertSimpleAsync(_context.AnimalPrice, model.AnimalPrice, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x => { x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate; x.ReportedBy = reportedBy; });

            await _context.GrainPricePerQuintals.Where(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate).ExecuteDeleteAsync();
            foreach (var row in model.GrainPriceRows.Where(x => x.GrainTypeId != Guid.Empty))
            {
                row.GrainPricePerQuintalId = Guid.NewGuid(); row.WoredaId = woredaId; row.StartDate = startDate; row.EndDate = endDate; row.ReportedBy = reportedBy;
                var previous = await _context.GrainPricePerQuintals.Where(x => x.WoredaId == woredaId && x.GrainTypeId == row.GrainTypeId && x.EndDate < startDate).OrderByDescending(x => x.EndDate).FirstOrDefaultAsync();
                if (previous != null) { row.PriceDifference = row.WeeklyPrice - previous.WeeklyPrice; row.PriceChangePercentage = previous.WeeklyPrice == 0 ? null : row.PriceDifference / previous.WeeklyPrice * 100; row.WeeklyMarketStatus = row.WeeklyPrice > previous.WeeklyPrice ? Models.PriceofGrain.WeeklyStatus.Increased : row.WeeklyPrice < previous.WeeklyPrice ? Models.PriceofGrain.WeeklyStatus.Decreased : Models.PriceofGrain.WeeklyStatus.Same; }
                _context.GrainPricePerQuintals.Add(row);
            }
            await _context.AnimalPricePerUnits.Where(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate).ExecuteDeleteAsync();
            foreach (var row in model.AnimalPriceRows.Where(x => x.AnimalTypeId != Guid.Empty))
            {
                row.AnimalPricePerUnitId = Guid.NewGuid(); row.WoredaId = woredaId; row.StartDate = startDate; row.EndDate = endDate; row.ReportedBy = reportedBy;
                var previous = await _context.AnimalPricePerUnits.Where(x => x.WoredaId == woredaId && x.AnimalTypeId == row.AnimalTypeId && x.EndDate < startDate).OrderByDescending(x => x.EndDate).FirstOrDefaultAsync();
                if (previous != null) { row.PriceDifference = row.WeeklyPrice - previous.WeeklyPrice; row.PriceChangePercentage = previous.WeeklyPrice == 0 ? null : row.PriceDifference / previous.WeeklyPrice * 100; row.WeeklyMarketStatus = row.WeeklyPrice > previous.WeeklyPrice ? Models.PriceofGrain.WeeklyStatus.Increased : row.WeeklyPrice < previous.WeeklyPrice ? Models.PriceofGrain.WeeklyStatus.Decreased : Models.PriceofGrain.WeeklyStatus.Same; }
                _context.AnimalPricePerUnits.Add(row);
            }

            model.WeeklyAccidents.WoredaId = woredaId;
            model.WeeklyAccidents.StartDate = startDate;
            model.WeeklyAccidents.EndDate = endDate;
            model.WeeklyAccidents.ReportedBy = reportedBy;
            var accident = await _context.WeeklyAccidents
                .FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate);
            if (accident == null) { accident = model.WeeklyAccidents; _context.WeeklyAccidents.Add(accident); }
            else { _context.Entry(accident).CurrentValues.SetValues(model.WeeklyAccidents); await _context.AccidentDetails.Where(x => x.WeeklyAccidentsId == accident.WeeklyAccidentsId).ExecuteDeleteAsync(); }
            if (accident.HasAccident)
                accident.AccidentDetails = model.AccidentDetails.Where(x => model.SelectedAccidentTypeIds.Contains(x.AccidentTypeId)).Select(x => { x.AccidentDetailId = Guid.NewGuid(); return x; }).ToList();

            model.Migration.WoredaId = woredaId; model.Migration.StartDate = startDate; model.Migration.EndDate = endDate; model.Migration.ReportedBy = reportedBy;
            var migration = await _context.Migrations.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate);
            if (migration == null) { migration = model.Migration; _context.Migrations.Add(migration); }
            else { _context.Entry(migration).CurrentValues.SetValues(model.Migration); await _context.MigrationDetails.Where(x => x.MigrationId == migration.MigrationId).ExecuteDeleteAsync(); }
            if (migration.HasMigration)
            {
                if (model.MigrationDetail.IsInward) model.MigrationDetail.DestinationLocationId = woredaId;
                else model.MigrationDetail.OriginLocationId = woredaId;
                migration.MigrationDetails = [model.MigrationDetail];
            }

            model.Death.WoredaId = woredaId; model.Death.StartDate = startDate; model.Death.EndDate = endDate; model.Death.ReportedBy = reportedBy;
            var death = await _context.Deaths.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate);
            if (death == null) { death = model.Death; _context.Deaths.Add(death); }
            else { _context.Entry(death).CurrentValues.SetValues(model.Death); await _context.DeathDetails.Where(x => x.DeathId == death.DeathId).ExecuteDeleteAsync(); }
            if (death.HasDeath) death.DeathDetails = [model.DeathDetail];

            await UpsertSimpleAsync(_context.AssistanceRecipients, model.AssistanceRecipient, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x => { x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate; x.ReportedBy = reportedBy; });
            model.WeeklyProvided.WoredaId = woredaId; model.WeeklyProvided.StartDate = startDate; model.WeeklyProvided.EndDate = endDate; model.WeeklyProvided.ReportedBy = reportedBy;
            var weeklyProvided = await _context.WeeklyProvideds.FirstOrDefaultAsync(x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate);
            if (weeklyProvided == null) { weeklyProvided = model.WeeklyProvided; _context.WeeklyProvideds.Add(weeklyProvided); }
            else { _context.Entry(weeklyProvided).CurrentValues.SetValues(model.WeeklyProvided); await _context.WeeklyProvidedDetails.Where(x => x.WeeklyProvidedId == weeklyProvided.WeeklyProvidedId).ExecuteDeleteAsync(); }
            weeklyProvided.WeeklyProvidedDetail = model.AidDistributionDetails.Where(x => x.SupplyTypeId != Guid.Empty).Select(x => new WeeklyProvidedDetail { SupplyTypeId = x.SupplyTypeId, Measurement = x.Measurement, ProvidedQuantity = x.ProvidedQuantity, DistributedQuantity = x.DistributedQuantity, PercentageOfDistributedFromProvided = x.ProvidedQuantity == 0 ? 0 : x.DistributedQuantity / x.ProvidedQuantity * 100, Donor = x.Donor }).ToList();
            await UpsertSimpleAsync(_context.OtherProblems, model.OtherProblem, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x => { x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate; x.ReportedBy = reportedBy; if (string.IsNullOrWhiteSpace(x.ProblemName)) x.ProblemName = "የለም"; });
            await UpsertSimpleAsync(_context.SchoolReports, model.SchoolReport, x => x.WoredaId == woredaId && x.StartDate == startDate && x.EndDate == endDate,
                x =>
                {
                    x.WoredaId = woredaId; x.StartDate = startDate; x.EndDate = endDate;
                    if (!x.HasTeachingLearningProblem)
                    {
                        x.Cause = null; x.MaleStudentDropouts = 0; x.FemaleStudentDropouts = 0; x.AverageDropoutAge = null; x.DropoutRatePercent = null; x.ClosedSchools = 0;
                        x.MajorSolutionOne = null; x.MajorSolutionTwo = null; x.MajorSolutionThree = null; x.WasActionSatisfactory = null;
                        x.StudentRelatedSolution = false; x.TeacherExpenseSolution = false; x.ClassroomMaterialsSolution = false;
                        x.SchoolBuildingServiceSolution = false; x.OtherSolution = false; x.OtherSolutionDescription = null;
                    }
                    else if (!x.OtherSolution) x.OtherSolutionDescription = null;
                });
        }

        private async Task UpsertSimpleAsync<TEntity>(DbSet<TEntity> set, TEntity incoming,
            System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, Action<TEntity> prepare) where TEntity : class
        {
            prepare(incoming);
            var existing = await set.FirstOrDefaultAsync(predicate);
            if (existing == null) set.Add(incoming);
            else _context.Entry(existing).CurrentValues.SetValues(incoming);
        }

        private static Models.WeeklyAccidents.AccidentDetail CopyAccidentDetail(
            Models.WeeklyAccidents.AccidentDetail source, Guid typeId) => new()
        {
            AccidentTypeId = typeId,
            DamagedLandInHectares = source.DamagedLandInHectares,
            DamageRateInPercent = source.DamageRateInPercent,
            AffectedHouseholdsMale = source.AffectedHouseholdsMale,
            AffectedHouseholdsFemale = source.AffectedHouseholdsFemale,
            AffectedChildrenMale = source.AffectedChildrenMale,
            AffectedChildrenFemale = source.AffectedChildrenFemale,
            AffectedYouthMale = source.AffectedYouthMale,
            AffectedYouthFemale = source.AffectedYouthFemale,
            AffectedElderlyMale = source.AffectedElderlyMale,
            AffectedElderlyFemale = source.AffectedElderlyFemale,
            AffectedDisabledMale = source.AffectedDisabledMale,
            AffectedDisabledFemale = source.AffectedDisabledFemale,
            Notes = source.Notes
        };

        private static List<CropPestImpactDetail> BuildCropImpactDetails(RegistrationWithardViewModel model, Guid reportId) =>
            model.CropPestImpactDetails.Where(x => model.SelectedCropDiseaseIds.Contains(x.CropPestAndDeseaseId)).Select(x =>
            {
                x.Id = Guid.NewGuid(); x.CropPestAndDeseaseReportId = reportId; return x;
            }).ToList();

        private static List<AnimalDiseaseImpactDetail> BuildAnimalImpactDetails(RegistrationWithardViewModel model, Guid reportId) =>
            model.AnimalDiseaseImpactDetails.Where(x => model.SelectedAnimalDiseaseIds.Contains(x.AnimalDiseaseId)).Select(x =>
            {
                x.Id = Guid.NewGuid(); x.AnimalHealthStatusId = reportId; return x;
            }).ToList();
        private async Task PopulateLookupListsAsync(RegistrationWithardViewModel model)
        {
            ViewBag.WoredaSelectList = new SelectList(
                await _context.Set<Locations>().Where(l => l.Level == LocationLevel.ወረዳ && l.IsActive).ToListAsync(),
                "Id", "LocationAmharicName", model.WoredaId
            );

            model.Kebelies = await _context.Set<Locations>()
                .Where(l => l.ParentId == model.WoredaId &&
                            l.Level == LocationLevel.ቀበሌ &&
                            l.IsActive)
                .AsNoTracking()
                .ToListAsync();
            model.AnimalDiseases = await _context.Set<AnimalDisease>().ToListAsync();
            model.CropPestAndDeseases = await _context.Set<CropPestAndDesease>().ToListAsync();
            model.EpidemicDiseaseTypes = await _context.EpidemicDiseaseTypes.Where(x => x.IsActive).ToListAsync();
            model.AccidentTypes = await _context.AccidentTypes.Where(x => x.IsActive).ToListAsync();
            model.GrainTypes = await _context.GrainTypes.Where(x => x.IsActive).ToListAsync();
            model.AnimalTypes = await _context.AnimalTypes.Where(x => x.IsActive).ToListAsync();
            model.SupplyTypes = await _context.SupplyTypes.Where(x => x.IsActive).ToListAsync();
            model.MigrationLocations = await _context.Locations.Where(x => x.Level == LocationLevel.ወረዳ && x.IsActive && x.Id != model.WoredaId).ToListAsync();
            if (model.AnimalMigrationRows.Count == 0) model.AnimalMigrationRows = BuildAnimalMigrationRows();
            ViewBag.WoredaName = await _context.Locations.Where(x => x.Id == model.WoredaId).Select(x => x.LocationAmharicName ?? x.LocationName ?? "-").FirstOrDefaultAsync();
        }

        private static List<AnimalMigrationEntryViewModel> BuildAnimalMigrationRows() =>
            Enum.GetValues<MigratingAnimalType>()
                .Select(type => new AnimalMigrationEntryViewModel { AnimalType = type }).ToList();

        private async Task PopulatePreviousPricesAsync(RegistrationWithardViewModel model, Guid woredaId, DateTime startDate)
        {
            foreach (var row in model.GrainPriceRows)
                row.PreviousWeekPrice = (await _context.GrainPricePerQuintals.Where(x => x.WoredaId == woredaId && x.GrainTypeId == row.GrainTypeId && x.EndDate < startDate).OrderByDescending(x => x.EndDate).Select(x => (decimal?)x.WeeklyPrice).FirstOrDefaultAsync());
            foreach (var row in model.AnimalPriceRows)
                row.PreviousWeekPrice = (await _context.AnimalPricePerUnits.Where(x => x.WoredaId == woredaId && x.AnimalTypeId == row.AnimalTypeId && x.EndDate < startDate).OrderByDescending(x => x.EndDate).Select(x => (decimal?)x.WeeklyPrice).FirstOrDefaultAsync());
        }
    }
}
