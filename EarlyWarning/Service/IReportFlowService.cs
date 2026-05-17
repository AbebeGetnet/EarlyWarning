using EarlyWarning.ViewModels.PendingViewModel;

namespace EarlyWarning.Service
{
    public interface IReportFlowService
    {
        void SavePendingReport(string reportType, DateTime startDate, DateTime endDate, Guid woredaId, string reportedBy);
        PendingReportData GetPendingReport(string reportType);
        void ClearPendingReport(string reportType);
        bool HasPendingReport(string reportType);
        void ClearAllPendingReports();
    }
}
