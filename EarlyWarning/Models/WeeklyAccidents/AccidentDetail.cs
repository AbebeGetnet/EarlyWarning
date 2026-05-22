using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.WeeklyAccidents
{
    public class AccidentDetail
    {
        [Key]
        public Guid AccidentDetailId { get; set; }

        [Required]
        public Guid WeeklyAccidentsId { get; set; }

        [ForeignKey("WeeklyAccidentsId")]
        public virtual WeeklyAccidents? WeeklyAccidents { get; set; }

        [Display(Name = "የአደጋ አይነት")]
        [Required(ErrorMessage = "የአደጋ አይነት መምረጥ ያስፈልጋል")]
        public Guid AccidentTypeId { get; set; }

        [ForeignKey("AccidentTypeId")]
        public virtual AccidentType? AccidentType { get; set; }

        // የተጎዳ መሬት
        [Display(Name = "የተጎዳ መሬት (በሄክታር)")]
        [Range(0, 1000000)]
        public decimal? DamagedLandInHectares { get; set; }

        [Display(Name = "የጉዳት መጠን (%)")]
        [Range(0, 100)]
        public decimal? DamageRateInPercent { get; set; }

        // የተጎዱ ቤተሰቦች
        [Display(Name = "የተጎዱ ቤተሰቦች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int AffectedHouseholdsMale { get; set; }

        [Display(Name = "የተጎዱ ቤተሰቦች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int AffectedHouseholdsFemale { get; set; }

        // ህጻናት
        [Display(Name = "የተጎዱ ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int AffectedChildrenMale { get; set; }

        [Display(Name = "የተጎዱ ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int AffectedChildrenFemale { get; set; }

        // ወጣቶች
        [Display(Name = "የተጎዱ ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int AffectedYouthMale { get; set; }

        [Display(Name = "የተጎዱ ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int AffectedYouthFemale { get; set; }

        // አረጋውያን
        [Display(Name = "የተጎዱ አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int AffectedElderlyMale { get; set; }

        [Display(Name = "የተጎዱ አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int AffectedElderlyFemale { get; set; }

        // አካል ጉዳተኞች
        [Display(Name = "የተጎዱ አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int AffectedDisabledMale { get; set; }

        [Display(Name = "የተጎዱ አካል ጉዳተኞች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int AffectedDisabledFemale { get; set; }

        // ማስታወሻ
        [Display(Name = "ማስታወሻ")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Computed Properties
        [Display(Name = "አጠቃላይ የተጎዱ ቤተሰቦች")]
        public int TotalAffectedHouseholds => AffectedHouseholdsMale + AffectedHouseholdsFemale;

        [Display(Name = "አጠቃላይ የተጎዱ ህጻናት")]
        public int TotalAffectedChildren => AffectedChildrenMale + AffectedChildrenFemale;

        [Display(Name = "አጠቃላይ የተጎዱ ወጣቶች")]
        public int TotalAffectedYouth => AffectedYouthMale + AffectedYouthFemale;

        [Display(Name = "አጠቃላይ የተጎዱ አረጋውያን")]
        public int TotalAffectedElderly => AffectedElderlyMale + AffectedElderlyFemale;

        [Display(Name = "አጠቃላይ የተጎዱ አካል ጉዳተኞች")]
        public int TotalAffectedDisabled => AffectedDisabledMale + AffectedDisabledFemale;

    }
}
