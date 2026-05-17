using EarlyWarning.ViewModels.PendingViewModel;
using System.Text.Json;

namespace EarlyWarning.Service
{
    public class ReportFlowService : IReportFlowService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKeyPrefix = "PendingReport_";

        public ReportFlowService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SavePendingReport(string reportType, DateTime startDate, DateTime endDate, Guid woredaId, string reportedBy)
        {
            var data = new PendingReportData
            {
                ReportType = reportType,
                StartDate = startDate,
                EndDate = endDate,
                WoredaId = woredaId,
                ReportedBy = reportedBy,
                SavedAt = DateTime.Now
            };

            var json = JsonSerializer.Serialize(data);
            _httpContextAccessor.HttpContext.Session.SetString(SessionKeyPrefix + reportType, json);
        }

        public PendingReportData GetPendingReport(string reportType)
        {
            var json = _httpContextAccessor.HttpContext.Session.GetString(SessionKeyPrefix + reportType);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<PendingReportData>(json);
        }

        public void ClearPendingReport(string reportType)
        {
            _httpContextAccessor.HttpContext.Session.Remove(SessionKeyPrefix + reportType);
        }

        public bool HasPendingReport(string reportType)
        {
            return !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString(SessionKeyPrefix + reportType));
        }

        public void ClearAllPendingReports()
        {
            var pendingReports = new[] { "GrainSupply", "GrainPricePerQuintal", "EpidemicDisease" };
            foreach (var report in pendingReports)
            {
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeyPrefix + report);
            }
        }
    }
}
