using EarlyWarning.Enums;
using EarlyWarning.Models.PriceofGrain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models.WeeklyAccidents
{
    public class WeeklyAccidents
    {
        public Guid WeeklyAccidentsId { get; set; }
        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // 🆕 አደጋ አለ/የለም
        [Display(Name = "በሳምንቱ አደጋ ተከስቷል?")]
        [Required(ErrorMessage = "እባክዎ አደጋ መከሰቱን ይምረጡ")]
        public bool HasAccident { get; set; } = false;


        [Display(Name = "የወረዳ መረጃ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "የሪፖርት ሁኔታ")]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.በሂደት_ላይ;

        // Navigation property for multiple accidents
        public ICollection<AccidentDetail>? AccidentDetails { get; set; }
    }
}
