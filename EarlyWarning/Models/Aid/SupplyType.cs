using EarlyWarning.Models.SupplyofGrain;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.Aid
{
    public class SupplyType
    {
        [Key]
        public Guid SupplyTypeId { get; set; }

        [Required(ErrorMessage = "የአቅርቦት አይነት ስም ያስፈልጋል")]
        [Display(Name = "የአቅርቦት አይነት ስም")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "የአቅርቦት አይነት ስም ከ2 እስከ 100 ቁምፊዎች መሆን አለበት")]
        public string SupplyTypeName { get; set; } = string.Empty;

        [Display(Name = "ንቁ ነው?")]
        public bool IsActive { get; set; } = true;
        // Navigation property
        public ICollection<WeeklyProvided>? WeeklyProvided { get; set; }
    }
}
