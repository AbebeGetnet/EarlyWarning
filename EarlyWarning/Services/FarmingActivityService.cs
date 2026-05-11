using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using Microsoft.AspNetCore.Identity;

namespace EarlyWarning.Services
{
    public class FarmingActivityService
    {
        private readonly IFarmingActivityRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FarmingActivityService(IFarmingActivityRepository repository,
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

        public async Task<IEnumerable<FarmingActivity>> GetAllActivitiesAsync()
        {
            return await _repository.GetAllAsync(a => a.Woreda);
        }

        public async Task<FarmingActivity?> GetActivityByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id, a => a.Woreda);
        }

        public async Task CreateActivityAsync(FarmingActivity activity)
        {
            activity.Status = ReportStatus.Draft;
            await _repository.AddAsync(activity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateActivityAsync(FarmingActivity activity)
        {
            var existing = await _repository.GetByIdAsync(activity.Id);
            if (existing == null) throw new Exception("Activity not found");
            if (existing.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft activities can be edited.");

            _repository.Update(activity);
            await _repository.SaveChangesAsync();
        }

        public async Task SubmitActivityAsync(Guid id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null) throw new Exception("Not found");
            if (activity.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft activities can be submitted.");

            activity.Status = ReportStatus.Submitted;
            activity.SubmittedAt = DateTime.UtcNow;
            activity.SubmittedById = await GetCurrentUserIdAsync();
            _repository.Update(activity);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByZoneAsync(Guid id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null) throw new Exception("Not found");
            if (activity.Status != ReportStatus.Submitted)
                throw new InvalidOperationException("Only submitted activities can be zone-approved.");

            activity.Status = ReportStatus.ZoneApproved;
            activity.ZoneApprovedAt = DateTime.UtcNow;
            activity.ZoneApprovedById = await GetCurrentUserIdAsync();
            _repository.Update(activity);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByRegionAsync(Guid id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null) throw new Exception("Not found");
            if (activity.Status != ReportStatus.ZoneApproved)
                throw new InvalidOperationException("Only zone-approved activities can be region-approved.");

            activity.Status = ReportStatus.RegionApproved;
            activity.RegionApprovedAt = DateTime.UtcNow;
            activity.RegionApprovedById = await GetCurrentUserIdAsync();
            _repository.Update(activity);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteActivityAsync(Guid id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null) return;
            if (activity.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft activities can be deleted.");
            _repository.Delete(activity);
            await _repository.SaveChangesAsync();
        }
    }
}
