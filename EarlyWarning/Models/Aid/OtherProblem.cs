using EarlyWarning.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models.Aid
{
    public class OtherProblem
    {
        public Guid OtherProblemId { get; set; }
        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Display(Name = "ያጋጠመ ችግር አለ?")]
        [Required(ErrorMessage = "እባክዎ ያጋጠመ ችግር መኖሩን ይምረጡ")]
        public bool HasOtherProblem { get; set; }
        [Display(Name = "ያጋጠመ ችግር")]
        [Required(ErrorMessage = "እባክዎ ያጋጠመ ችግር ይጻፉ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string ProblemName { get; set; } = string.Empty;
        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }
        // የተሰደዱ ሰዎች በምድብ
        [Display(Name = "የተጎዳ አባወራ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleHouseholdHeads { get; set; }

        [Display(Name = "የተጎዳ እማወራ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleHouseholdHeads { get; set; }

        [Display(Name = "የተጎዳ ቤተሰብ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleFamilyMembers { get; set; }

        [Display(Name = "የተጎዳ ቤተሰብ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleFamilyMembers { get; set; }

        [Display(Name = "የተጎዳ ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleChildren { get; set; }

        [Display(Name = "የተጎዳ ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleChildren { get; set; }

        [Display(Name = "የተጎዳ ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleYouth { get; set; }

        [Display(Name = "የተጎዳ ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleYouth { get; set; }

        [Display(Name = "የተጎዳ አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleElderly { get; set; }

        [Display(Name = "የተጎዳ አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleElderly { get; set; }

        [Display(Name = "የተጎዳ አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleDisabled { get; set; }

        [Display(Name = "የተጎዳ አካል ጉዳተኞች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleDisabled { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? GeneralNotes { get; set; }

        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "የሪፖርት ሁኔታ")]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.በሂደት_ላይ;

    }
}
