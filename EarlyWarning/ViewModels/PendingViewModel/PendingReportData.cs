namespace EarlyWarning.ViewModels.PendingViewModel
{
    public class PendingReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid WoredaId { get; set; }
        public string ReportedBy { get; set; }
        public DateTime SavedAt { get; set; }
        public string ReportType { get; set; }
    }
}
