using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EarlyWarning.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var model = new RegistrationWithardViewModel
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today,
                RainfallReport = new RainfallReport(),
                FarmingActivity = new FarmingActivity(),
                CropPestAndDeseaseReport = new CropPestAndDeseaseReport(),
                AnimalWaterSupplyStatus = new AnimalWaterSupplyStatus(),
                AnimalHealthStatus = new AnimalHealthStatus()
            };

            var woredaQuery = _context.Locations.Where(l => l.ParentId == currentUser.LocationId && l.Level == LocationLevel.ቀበሌ).AsNoTracking();
            ViewBag.TotalKebeles = woredaQuery.Count();
            return View(model);        
        }

        [HttpGet]
        public async Task<IActionResult> GetExistingWizardData(Guid woredaId, DateTime startDate, DateTime endDate)
        {
            var sDate = startDate.Date;
            var eDate = endDate.Date;

            // 1. Query all 6 independent domain tables concurrently
            var rainfallTask = _context.RainfallReports
                .FirstOrDefaultAsync(r => r.WoredaId == woredaId && r.StartDate == sDate && r.EndDate == eDate);

            var farmingTask = _context.FarmingActivities
                .FirstOrDefaultAsync(f => f.WoredaId == woredaId && f.StartDate == sDate && f.EndDate == eDate);

            var pestTask = _context.CropPestAndDeseaseReports
                .FirstOrDefaultAsync(p => p.WoredaId == woredaId && p.StartDate == sDate && p.EndDate == eDate);

            var pastureTask = _context.PastureStatuses
                .FirstOrDefaultAsync(p => p.WoredaId == woredaId && p.StartDate == sDate && p.EndDate == eDate);

            var waterTask = _context.AnimalWaterSupplyStatuses
                .FirstOrDefaultAsync(w => w.WoredaId == woredaId && w.StartDate == sDate && w.EndDate == eDate);

            var healthTask = _context.AnimalHealthStatuses
                .FirstOrDefaultAsync(h => h.WoredaId == woredaId && h.StartDate == sDate && h.EndDate == eDate);

            // Wait for all database operations to complete
            await Task.WhenAll(rainfallTask, farmingTask, pestTask, pastureTask, waterTask, healthTask);

            var rainfall = await rainfallTask;
            var farming = await farmingTask;
            var pest = await pestTask;
            var pasture = await pastureTask;
            var water = await waterTask;
            var health = await healthTask;

            // 2. Check if data exists in ANY of the domains
            bool dataExists = (rainfall != null || farming != null || pest != null ||
                               pasture != null || water != null || health != null);

            if (!dataExists)
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
                // Since there's no single parent ID, we pass a truthy state flag to JavaScript
                id = 1,
                remarks = pest?.Remarks ?? pasture?.Remarks ?? health?.Remarks ?? "", // Fallback remark locator

                // Table 1: Rainfall
                rainfall = rainfall == null ? null : new
                {
                    full = rainfall.FullCoverageKebeles,
                    partial = rainfall.PartialCoverageKebeles,
                    none = rainfall.NoRainKebeles,
                    high = rainfall.HighAmountKebeles,
                    //kebeleIds = kebeleIds
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
                    //pestIds = pestIds,
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
                   // diseaseIds = diseaseIds,
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
        // Process the Consolidated Multi-Step Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWizard(RegistrationWithardViewModel model, List<Guid> SelectedKebeliesIds, List<Guid> SelectedCropDiseaseIds, List<Guid> SelectedAnimalDiseaseIds)
        {
            //if (!ModelState.IsValid)
            //{
            //    // Repopulate ViewBags/SelectLists if validation fails to safely redisplay the form
            //    ViewBag.WoredaSelectList = new SelectList(_context.Locations, "Id", "LocationName", model.WoredaId);
            //    return View("Index", model);
            //}

            var woredaId = model.WoredaId;
            var sDate = model.StartDate;
            var eDate = model.EndDate;

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
                existingRain.HighAmountKebeles = model.RainfallReport.HighAmountKebeles;
                _context.RainfallReports.Update(existingRain);

                // Sync Kebele Checkboxes (Remove old mapping, insert new ones)
                var oldKebeles = _context.Locations.Where(k => k.Id == existingRain.Id);
                _context.Locations.RemoveRange(oldKebeles);

                if (SelectedKebeliesIds != null)
                {
                    foreach (var kebeleId in SelectedKebeliesIds)
                    {
                        _context.Locations.Add(new Locations { Id = existingRain.Id, ParentId = kebeleId });
                    }
                }
            }
            else
            {
                model.RainfallReport.WoredaId = woredaId;
                model.RainfallReport.StartDate = sDate;
                model.RainfallReport.EndDate = eDate;
                _context.RainfallReports.Add(model.RainfallReport);
                await _context.SaveChangesAsync(); // Generates ID for newly added Rainfall Report tracking dependencies

                if (SelectedKebeliesIds != null)
                {
                    foreach (var kebeleId in SelectedKebeliesIds)
                    {
                        _context.Locations.Add(new Locations { Id = model.RainfallReport.WoredaId, ParentId = kebeleId });
                    }
                }
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

                _context.FarmingActivities.Update(existingFarm);
            }
            else
            {
                model.FarmingActivity.WoredaId = woredaId;
                model.FarmingActivity.StartDate = sDate;
                model.FarmingActivity.EndDate = eDate;
                _context.FarmingActivities.Add(model.FarmingActivity);
            }

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

                _context.CropPestAndDeseaseReports.Update(existingPest);

                var oldPests = _context.CropPestAndDeseaseReports.Where(p => p.CropDiseasesJson == existingPest.Id.ToString());
                _context.CropPestAndDeseaseReports.RemoveRange(oldPests);

                if (SelectedCropDiseaseIds != null && model.CropPestAndDeseaseReport.HasPestAndDeseasOccured)
                {
                    foreach (var pestId in SelectedCropDiseaseIds)
                    {
                        _context.CropPestAndDeseaseReports.Add(new CropPestAndDeseaseReport { Id = existingPest.Id, CropDiseasesJson = pestId.ToString() });
                    }
                }
            }
            else
            {
                model.CropPestAndDeseaseReport.WoredaId = woredaId;
                model.CropPestAndDeseaseReport.StartDate = sDate;
                model.CropPestAndDeseaseReport.EndDate = eDate;
                model.CropPestAndDeseaseReport.Remarks = model.Remarks;
                _context.CropPestAndDeseaseReports.Add(model.CropPestAndDeseaseReport);
                await _context.SaveChangesAsync();

                if (SelectedCropDiseaseIds != null && model.CropPestAndDeseaseReport.HasPestAndDeseasOccured)
                {
                    foreach (var pestId in SelectedCropDiseaseIds)
                    {
                        _context.CropPestAndDeseaseReports.Add(new CropPestAndDeseaseReport { Id = model.CropPestAndDeseaseReport.Id, CropDiseasesJson = pestId.ToString() });
                    }
                }
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
                _context.PastureStatuses.Update(existingPasture);
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
            var existingWater = await _context.AnimalWaterSupplyStatuses
                .FirstOrDefaultAsync(w => w.WoredaId == woredaId && w.StartDate == sDate && w.EndDate == eDate);

            if (existingWater != null)
            {
                existingWater.Enough = model.AnimalWaterSupplyStatus.Enough;
                existingWater.NoOfKebeliesWithWaterSupply = model.AnimalWaterSupplyStatus.NoOfKebeliesWithWaterSupply;

                MapDemographicProperties(existingWater, model.AnimalWaterSupplyStatus);
                _context.AnimalWaterSupplyStatuses.Update(existingWater);
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

                MapDemographicProperties(existingHealth, model.AnimalHealthStatus);
                _context.AnimalHealthStatuses.Update(existingHealth);

                var oldDiseases = _context.AnimalHealthStatuses.Where(d => d.Id == existingHealth.Id);
                _context.AnimalHealthStatuses.RemoveRange(oldDiseases);

                if (SelectedAnimalDiseaseIds != null && !model.AnimalHealthStatus.Enough)
                {
                    foreach (var diseaseId in SelectedAnimalDiseaseIds)
                    {
                        _context.AnimalHealthStatuses.Add(new AnimalHealthStatus { Id = existingHealth.Id, AnimalDiseaseJson = diseaseId.ToString() });
                    }
                }
            }
            else
            {
                model.AnimalHealthStatus.WoredaId = woredaId;
                model.AnimalHealthStatus.StartDate = sDate;
                model.AnimalHealthStatus.EndDate = eDate;
                _context.AnimalHealthStatuses.Add(model.AnimalHealthStatus);
                await _context.SaveChangesAsync();

                if (SelectedAnimalDiseaseIds != null && !model.AnimalHealthStatus.Enough)
                {
                    foreach (var diseaseId in SelectedAnimalDiseaseIds)
                    {
                        _context.AnimalHealthStatuses.Add(new AnimalHealthStatus { Id = model.AnimalHealthStatus.Id, AnimalDiseaseJson = diseaseId.ToString() });
                    }
                }
            }

            // Final Single Unit-of-Work Database Commit Transaction
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "የሳምንቱ መረጃ በተሳካ ሁኔታ ተቀምጧል/ተስተካክሏል!";
            return RedirectToAction("Index");
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
        private async Task PopulateLookupListsAsync(RegistrationWithardViewModel model)
        {
            ViewBag.WoredaSelectList = new SelectList(
                await _context.Set<Locations>().Where(l => l.Level == LocationLevel.ወረዳ && l.IsActive).ToListAsync(),
                "Id", "LocationAmharicName", model.WoredaId
            );

            model.Kebelies = await _context.Set<Locations>().Where(l => l.Level == LocationLevel.ቀበሌ && l.IsActive).ToListAsync();
            model.AnimalDiseases = await _context.Set<AnimalDisease>().ToListAsync();
            model.CropPestAndDeseases = await _context.Set<CropPestAndDesease>().ToListAsync();
        }
    }
}