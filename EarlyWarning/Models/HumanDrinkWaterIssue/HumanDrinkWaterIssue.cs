using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models.HumanDrinkWaterIssue
{
    public class HumanDrinkWaterIssue
    {
            [Key]
        public Guid HumanDrinkWaterIssueId { get; set; }

        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "የሰው መጠጥ ውሃ ችግር አለ?")]
        public bool HasWaterProblem { get; set; }

        [Display(Name = "ችግር ያለባቸው ቀበሌዎች ቁጥር")]
        [Range(0, int.MaxValue, ErrorMessage = "እባክዎ ትክክለኛ ቁጥር ያስገቡ")]
        public int NumberOfAffectedKebeles { get; set; }


        [Display(Name = "በችግር የተጎዱ አባወራ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleHouseholdHeads { get; set; }

        [Display(Name = "በችግር የተጎዱ እማወራ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleHouseholdHeads { get; set; }

        [Display(Name = "በችግር የተጎዱ ቤተሰብ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleFamilyMembers { get; set; }

        [Display(Name = "በችግር የተጎዱ ቤተሰብ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleFamilyMembers { get; set; }

        [Display(Name = "በችግር የተጎዱ ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleChildren { get; set; }

        [Display(Name = "በችግር የተጎዱ ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleChildren { get; set; }

        [Display(Name = "በችግር የተጎዱ ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleYouth { get; set; }

        [Display(Name = "በችግር የተጎዱ ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleYouth { get; set; }

        [Display(Name = "በችግር የተጎዱ አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleElderly { get; set; }

        [Display(Name = "በችግር የተጎዱ አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleElderly { get; set; }

        [Display(Name = "በችግር የተጎዱ አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleDisabled { get; set; }

        [Display(Name = "በችግር የተጎዱ አካል ጉዳተኞች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleDisabled { get; set; }

        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }
        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }
        public List<Guid>? AffectedKebeleIds { get; set; }
        public ICollection<Locations>? AffectedKebeles { get; set; }
        // Navigation property for many-to-many
        public ICollection<HumanDrinkWaterKebele>? HumanDrinkWaterKebeles { get; set; }

        // 🆕 የወረዳ መለያ (Woreda ID)
        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [Display(Name = "ወረዳ")]
        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }

        public ApprovalStatus ZonalApproval { get; set; }
        [Display(Name = "ሪፖርት ያጸደቀው ሰው")]
        public string? ZoneReportedBy { get; set; }
        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? ZoneAdditionalNotes { get; set; }
        public ApprovalStatus RegionApproval { get; set; }
        [Display(Name = "ሪፖርት ያጸደቀው ሰው")]
        public string? RegionalReportedBy { get; set; }
        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? RegionAdditionalNotes { get; set; }
    }
}
