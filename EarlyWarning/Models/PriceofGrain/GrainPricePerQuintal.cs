using EarlyWarning.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models.PriceofGrain
{
    public class GrainPricePerQuintal
    {
        [Key] 
        public Guid GrainPricePerQuintalId { get; set; }
        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Display(Name = "የእህል አይነት")]
        [Required(ErrorMessage = "የእህል አይነት መምረጥ ያስፈልጋል")]
        public Guid GrainTypeId { get; set; }

        [ForeignKey("GrainTypeId")]
        public virtual GrainType? GrainType { get; set; }

        [Display(Name = "የሳምንቱ ዋጋ (በቅንጅት)")]
        [Required(ErrorMessage = "የእህል ዋጋ ያስፈልጋል")]
        [Range(0, 100000, ErrorMessage = "ዋጋ ከ0 እስከ 100,000 ብር መሆን አለበት")]
        [DataType(DataType.Currency)]
        public decimal WeeklyPrice { get; set; }

        [Display(Name = "የሳምንቱ የዋጋ ሁኔታ")]
        [Required(ErrorMessage = "የዋጋ ሁኔታ መምረጥ ያስፈልጋል")]
        public WeeklyStatus WeeklyMarketStatus { get; set; }

        [Display(Name = "ካለፈው ሳምንት ጋር ሲነጻጸር የዋጋ ልዩነት")]
        [Range(-100000, 100000)]
        public decimal? PriceDifference { get; set; }

        [Display(Name = "የዋጋ ለውጥ መቶኛ")]
        [Range(-100, 1000)]
        public decimal? PriceChangePercentage { get; set; }

        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }
        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "የሪፖርት ሁኔታ")]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.በሂደት_ላይ;
    }
    public enum WeeklyStatus
    {
        [Display(Name = "በመጨመር")]
        Increased = 1,

        [Display(Name = "በመቀነስ")]
        Decreased = 2,

        [Display(Name = "ተመሳሳይ ነው")]
        Same = 3
    }
}
