using EarlyWarning.Models.PriceofGrain;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.AnimalPrice
{
    public class AnimalType
    {

        [Key]
        public Guid AnimalTypeId { get; set; }

        [Required(ErrorMessage = "የእንስሳት ስም ያስፈልጋል")]
        [Display(Name = "የእንስሳት ስም")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "የእንስሳት ስም ከ2 እስከ 100 ቁምፊዎች መሆን አለበት")]
        public string AnimalTypeName { get; set; } = string.Empty;

        [Display(Name = "ንቁ ነው?")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<AnimalPricePerUnit>? AnimalPrices { get; set; }
    }
}
