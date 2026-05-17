using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.PriceofGrain
{
    public class GrainPriceIncreaseDecrease
    {
        [Key]
        public Guid GrainIncreaseDecreaseId { get; set; }

        [Required(ErrorMessage = "የምክንያት ስም ያስፈልጋል")]
        [Display(Name = "የምክንያት ስም")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "የምክንያት ስም ከ2 እስከ 200 ቁምፊዎች መሆን አለበት")]
        public string ReasonName { get; set; } = string.Empty;

        [Display(Name = "የምክንያት አይነት")]
        [Required(ErrorMessage = "የምክንያት አይነት መምረጥ ያስፈልጋል")]
        public ReasonType ReasonType { get; set; }
    }
    // ለጨመረ ምክንያት Enum
    public enum ReasonType
    {
        [Display(Name = "የጨመረ")]
        Increase = 1,

        [Display(Name = "ቀነሰ")]
        Decrease = 2
    }
}
