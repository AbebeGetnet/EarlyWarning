using EarlyWarning.Enums;
using EarlyWarning.Models.AnimalSupply;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace EarlyWarning.Models.Aid
{
    public class WeeklyProvided
    {
        public Guid WeeklyProvidedId { get; set; }
        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }
        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }
        

        [Display(Name = "ችግር")]
        public Problem? IfProblem { get; set; }
        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "የሪፖርት ሁኔታ")]
        public ApprovalStatus ReportStatus { get; set; } = ApprovalStatus.በሂደት_ላይ;
        public ICollection<WeeklyProvidedDetail>? WeeklyProvidedDetail { get; set; }
        

         // ለኋላ ተኳሃኝነት እንዲቆይ
    }
}

    public enum Problem
    {
        [Display(Name = "የለም")]
        No = 1,

        [Display(Name = "የዕለት እርዳታ በወቅቱ አለመቅረብ")]
        Problem1 = 2,

        [Display(Name = "የዕለት እርዳታ ሙሉ ሁኖ አለመቅረቡ")]
        Problem2 = 3,
        [Display(Name = "ከገባ በኋላ አለማሰራጨት")]
        Problem3 = 4
    }

