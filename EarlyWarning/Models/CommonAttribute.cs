using EarlyWarning.Enums;
using System.ComponentModel;

namespace EarlyWarning.Models
{
    public class CommonAttribute
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public DateTime LasModifiedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        [DisplayName("ከ")]
        public DateTime? StartDate { get; set; }
        [DisplayName("እስከ")]
        public DateTime? EndDate { get; set; }
        [DisplayName("ሁኔታ")]
        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        [DisplayName("የተላከበት ቀን")]
        public DateTime? SubmittedAt { get; set; }

        [DisplayName("ያስገባው ተጠቃሚ")]
        public string? SubmittedById { get; set; }

        [DisplayName("በዞን የጸደቀበት ቀን")]
        public DateTime? ZoneApprovedAt { get; set; }

        [DisplayName("በዞን ያጸደቀው ተጠቃሚ")]
        public string? ZoneApprovedById { get; set; }

        [DisplayName("በክልል የጸደቀበት ቀን")]
        public DateTime? RegionApprovedAt { get; set; }

        [DisplayName("በክልል ያጸደቀው ተጠቃሚ")]
        public string? RegionApprovedById { get; set; }
    }
}
