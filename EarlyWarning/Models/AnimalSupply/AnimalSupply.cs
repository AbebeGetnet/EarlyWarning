using EarlyWarning.Enums;
using EarlyWarning.Models.SupplyofGrain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.AnimalSupply
{
    public class AnimalSupply
    {
        [Key]
        public Guid GrainSupplyId { get; set; }
        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // የእህል አቅርቦት ሁኔታ
        [Display(Name = "የእንስሳት አቅርቦት ከባለፈው ሳምንት ጋር ሲነጻጸር")]
        [Required(ErrorMessage = "እባክዎ የእንስሳት አቅርቦት ሁኔታ ይምረጡ")]
        public SupplyStatus GrainSupplyStatus { get; set; }

        // የወረዳ መረጃ
        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }
        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }
        // ሪፖርት ማዘጋጃ መረጃ
        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "የሪፖርት ሁኔታ")]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.በሂደት_ላይ;
        public ICollection<AnimalSupplyDetail>? AnimalSupplyDetails { get; set; }
    }
    public enum SupplyStatus
    {
        [Display(Name = "ጨምሯል")]
        Increased = 1,

        [Display(Name = "ቀንሷል")]
        Decreased = 2,

        [Display(Name = "ተመሳሳይ ነው")]
        Same = 3
    }
}
