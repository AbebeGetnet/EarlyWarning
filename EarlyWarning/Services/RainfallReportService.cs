using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EarlyWarning.Services
{
    public class RainfallReportService
    {
        private readonly IRainfallReportRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RainfallReportService(IRainfallReportRepository repository,
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

        public async Task<IEnumerable<RainfallReport>> GetAllReportsAsync()
        {
            return await _repository.GetAllAsync(r => r.Woreda);
        }

        public async Task<RainfallReport?> GetReportByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id, r => r.Woreda);
        }

        public async Task CreateReportAsync(RainfallReport report)
        {
            report.Status = ReportStatus.Draft;
            await _repository.AddAsync(report);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateReportAsync(RainfallReport report)
        {
            var existing = await _repository.GetByIdAsync(report.Id);
            if (existing == null) throw new Exception("Report not found");
            if (existing.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft reports can be edited.");

            // Update scalar properties
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task SaveReportAsync(RainfallReport report)
        {
            var existing = await _repository.GetByIdAsync(report.Id);
            if (existing == null) throw new Exception("Report not found");

            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task SubmitReportAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new Exception("Not found");
            if (report.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft reports can be submitted.");

            report.Status = ReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            report.SubmittedById = await GetCurrentUserIdAsync();
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByZoneAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new Exception("Not found");
            if (report.Status != ReportStatus.Submitted)
                throw new InvalidOperationException("Only submitted reports can be zone-approved.");

            report.Status = ReportStatus.ZoneApproved;
            report.ZoneApprovedAt = DateTime.UtcNow;
            report.ZoneApprovedById = await GetCurrentUserIdAsync();
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByRegionAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new Exception("Not found");
            if (report.Status != ReportStatus.ZoneApproved)
                throw new InvalidOperationException("Only zone-approved reports can be region-approved.");

            report.Status = ReportStatus.RegionApproved;
            report.RegionApprovedAt = DateTime.UtcNow;
            report.RegionApprovedById = await GetCurrentUserIdAsync();
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) return;
            if (report.Status != ReportStatus.Draft)
                throw new InvalidOperationException("Only draft reports can be deleted.");
            _repository.Delete(report);
            await _repository.SaveChangesAsync();
        }
    }
}