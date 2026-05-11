using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using Microsoft.AspNetCore.Identity;

namespace EarlyWarning.Services
{
    public class AnimalHealthStatusService
    {
        private readonly IAnimalHealthStatusRepository _repository;
        public AnimalHealthStatusService(IAnimalHealthStatusRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AnimalHealthStatus>> GetAllReportsAsync()
        {
            return await _repository.GetAllAsync(r => r.Woreda);
        }

        public async Task<AnimalHealthStatus?> GetReportByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id, r => r.Woreda);
        }

        public async Task CreateReportAsync(AnimalHealthStatus report)
        {
            if (report.Enough) // disease exists -> serialize selected IDs
                report.SerializeCropDiseases();
            else
                report.AnimalDiseaseJson = null;

            await _repository.AddAsync(report);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateReportAsync(AnimalHealthStatus report)
        {
            if (report.Enough)
                report.SerializeCropDiseases();
            else
                report.AnimalDiseaseJson = null;

            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null)
                throw new InvalidOperationException("Report not found.");
            if (report.Status != ReportStatus.Draft && report.Status != ReportStatus.Rejected)
                throw new InvalidOperationException("Only draft or rejected reports can be deleted.");
            _repository.Delete(report);
            await _repository.SaveChangesAsync();
        }

        public async Task SubmitReportAsync(Guid id, string submittedById)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new InvalidOperationException("Report not found.");
            if (report.Status != ReportStatus.Draft) throw new InvalidOperationException("Only draft reports can be submitted.");
            report.Status = ReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            report.SubmittedById = submittedById;
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByZoneAsync(Guid id, string zoneAdminId)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new InvalidOperationException("Report not found.");
            if (report.Status != ReportStatus.Submitted) throw new InvalidOperationException("Only submitted reports can be approved by zone.");
            report.Status = ReportStatus.ZoneApproved;
            report.ZoneApprovedAt = DateTime.UtcNow;
            report.ZoneApprovedById = zoneAdminId;
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByRegionAsync(Guid id, string regionAdminId)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new InvalidOperationException("Report not found.");
            if (report.Status != ReportStatus.ZoneApproved) throw new InvalidOperationException("Only zone-approved reports can be approved by region.");
            report.Status = ReportStatus.RegionApproved;
            report.RegionApprovedAt = DateTime.UtcNow;
            report.RegionApprovedById = regionAdminId;
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task RejectByZoneAsync(Guid id, string zoneAdminId, string rejectionRemark)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new InvalidOperationException("Report not found.");
            if (report.Status != ReportStatus.Submitted) throw new InvalidOperationException("Only submitted reports can be rejected by zone.");
            report.Status = ReportStatus.Rejected;
            report.ZoneRejectionRemark = rejectionRemark;
            report.ZoneRejectedAt = DateTime.UtcNow;
            report.ZoneRejectedById = zoneAdminId;
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }

        public async Task RejectByRegionAsync(Guid id, string regionAdminId, string rejectionRemark)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new InvalidOperationException("Report not found.");
            if (report.Status != ReportStatus.ZoneApproved) throw new InvalidOperationException("Only zone-approved reports can be rejected by region.");
            report.Status = ReportStatus.Rejected;
            report.RegionRejectionRemark = rejectionRemark;
            report.RegionRejectedAt = DateTime.UtcNow;
            report.RegionRejectedById = regionAdminId;
            _repository.Update(report);
            await _repository.SaveChangesAsync();
        }
    }
}