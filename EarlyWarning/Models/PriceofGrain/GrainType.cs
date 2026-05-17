using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.PriceofGrain
{
    public class GrainType
    {
        [Key]
        public Guid GrainTypeId { get; set; }

        [Required(ErrorMessage = "የእህል ስም ያስፈልጋል")]
        [Display(Name = "የእህል ስም")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "የእህል ስም ከ2 እስከ 100 ቁምፊዎች መሆን አለበት")]
        public string GrainName { get; set; } = string.Empty;

        [Display(Name = "ንቁ ነው?")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<GrainPricePerQuintal>? GrainPrices { get; set; }
    }
}
