using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using Microsoft.AspNetCore.Identity;

namespace EarlyWarning.Services
{
    public class CropGrowthService
    {
        private readonly ICropGrowthRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CropGrowthService(ICropGrowthRepository repository,
                                 UserManager<ApplicationUser> userManager,
                                 IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user?.Id ?? throw new UnauthorizedAccessException();
        }

        public async Task<IEnumerable<CropGrowth>> GetAllCropGrowthsAsync()
        {
            return await _repository.GetAllAsync(c => c.Woreda);
        }

        public async Task<CropGrowth?> GetCropGrowthByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id, c => c.Woreda);
        }

        public async Task CreateCropGrowthAsync(CropGrowth cropGrowth)
        {
            cropGrowth.Status = ReportStatus.Draft;
            await _repository.AddAsync(cropGrowth);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCropGrowthAsync(CropGrowth cropGrowth)
        {
            var existing = await _repository.GetByIdAsync(cropGrowth.Id);
            if (existing == null) throw new Exception("Crop growth record not found");
            if (existing.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft records can be edited.");

            _repository.Update(cropGrowth);
            await _repository.SaveChangesAsync();
        }

        public async Task SaveCropGrowthAsync(CropGrowth cropGrowth)
        {
            var existing = await _repository.GetByIdAsync(cropGrowth.Id);
            if (existing == null) throw new Exception("Crop growth record not found");

            _repository.Update(cropGrowth);
            await _repository.SaveChangesAsync();
        }

        public async Task SubmitCropGrowthAsync(Guid id)
        {
            var cropGrowth = await _repository.GetByIdAsync(id);
            if (cropGrowth == null) throw new Exception("Not found");
            if (cropGrowth.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft records can be submitted.");

            cropGrowth.Status = ReportStatus.Submitted;
            cropGrowth.SubmittedAt = DateTime.UtcNow;
            cropGrowth.SubmittedById = await GetCurrentUserIdAsync();
            _repository.Update(cropGrowth);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByZoneAsync(Guid id)
        {
            var cropGrowth = await _repository.GetByIdAsync(id);
            if (cropGrowth == null) throw new Exception("Not found");
            if (cropGrowth.Status != ReportStatus.Submitted)
                throw new InvalidOperationException("Only submitted records can be zone-approved.");

            cropGrowth.Status = ReportStatus.ZoneApproved;
            cropGrowth.ZoneApprovedAt = DateTime.UtcNow;
            cropGrowth.ZoneApprovedById = await GetCurrentUserIdAsync();
            _repository.Update(cropGrowth);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByRegionAsync(Guid id)
        {
            var cropGrowth = await _repository.GetByIdAsync(id);
            if (cropGrowth == null) throw new Exception("Not found");
            if (cropGrowth.Status != ReportStatus.ZoneApproved)
                throw new InvalidOperationException("Only zone-approved records can be region-approved.");

            cropGrowth.Status = ReportStatus.RegionApproved;
            cropGrowth.RegionApprovedAt = DateTime.UtcNow;
            cropGrowth.RegionApprovedById = await GetCurrentUserIdAsync();
            _repository.Update(cropGrowth);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteCropGrowthAsync(Guid id)
        {
            var cropGrowth = await _repository.GetByIdAsync(id);
            if (cropGrowth == null) return;
            if (cropGrowth.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft records can be deleted.");
            _repository.Delete(cropGrowth);
            await _repository.SaveChangesAsync();
        }
    }
}
