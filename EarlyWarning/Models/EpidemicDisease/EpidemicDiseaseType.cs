using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.EpidemicDisease
{
    public class EpidemicDiseaseType
    {
        [Key]
        public Guid EpidemicDiseaseTypeId { get; set; }
        

        [Required(ErrorMessage = "የበሽታ ስም ያስፈልጋል")]
        [Display(Name = "የበሽታ ስም")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "የበሽታ ስም ከ2 እስከ 100 ቁምፊዎች መሆን አለበት")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "መግለጫ")]
        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public string? Description { get; set; }

        
        [Display(Name = "ንቁ ነው?")]
        public bool IsActive { get; set; } = true;

        // 🆕 Navigation property to EpidemicDiseaseSelection
        public virtual ICollection<EpidemicDiseaseSelection>? EpidemicDiseaseSelections { get; set; }

    }
}
