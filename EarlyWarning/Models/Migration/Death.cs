using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models.Migration
{
    public class Death
    {
        [Key]
        public Guid DeathId { get; set; }

        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "ሞት አለ?")]
        [Required(ErrorMessage = "እባክዎ ሞት መኖሩን ይምረጡ")]
        public bool HasDeath { get; set; }

        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }

        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "የሪፖርት ሁኔታ")]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.በሂደት_ላይ;
        // Navigation property for Death details
        public ICollection<DeathDetail>? DeathDetails { get; set; }
    }
}
