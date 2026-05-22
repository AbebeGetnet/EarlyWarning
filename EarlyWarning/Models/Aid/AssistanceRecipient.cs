using EarlyWarning.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models.Aid
{
    public class AssistanceRecipient
    {
        [Key]
        public Guid AssistanceRecipientId { get; set; }

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

        // የእርዳታ ተጠቃሚዎች በምድብ
        [Display(Name = "አባወራ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleHouseholdHeads { get; set; }

        [Display(Name = "እማወራ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleHouseholdHeads { get; set; }

        [Display(Name = "ቤተሰብ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleFamilyMembers { get; set; }

        [Display(Name = "ቤተሰብ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleFamilyMembers { get; set; }

        [Display(Name = "ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleChildren { get; set; }

        [Display(Name = "ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleChildren { get; set; }

        [Display(Name = "ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleYouth { get; set; }

        [Display(Name = "ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleYouth { get; set; }

        [Display(Name = "አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleElderly { get; set; }

        [Display(Name = "አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleElderly { get; set; }

        [Display(Name = "አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleDisabled { get; set; }

        [Display(Name = "አካል ጉዳተኞች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleDisabled { get; set; }

        // Metadata
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
    }
}
