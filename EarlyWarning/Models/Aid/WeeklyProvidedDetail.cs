using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models.Aid
{
    public class WeeklyProvidedDetail
    {
        [Key]
        public Guid WeeklyProvidedDetailId { get; set; }

        public Guid WeeklyProvidedId { get; set; }

        // Navigation properties
        [ForeignKey("WeeklyProvidedId")]
        public virtual WeeklyProvided? WeeklyProvided { get; set; }

        [Display(Name = "የእህል አይነት")]
        [Required(ErrorMessage = "የእህል አይነት መምረጥ ያስፈልጋል")]
        public Guid SupplyTypeId { get; set; }
        [ForeignKey("SupplyTypeId")]
        public virtual SupplyType? SupplyType { get; set; }
        [Display(Name = "የመለኪያ አሃድ")]
        [Required(ErrorMessage = "የመለኪያ አሃድ መምረጥ ያስፈልጋል")]
        public Measurement Measurement { get; set; }

        [Display(Name = "የቀረበ መጠን")]
        [Required(ErrorMessage = "የቀረበ መጠን ያስፈልጋል")]
        [Range(0, double.MaxValue, ErrorMessage = "እባክዎ ትክክለኛ ቁጥር ያስገቡ")]
        public decimal ProvidedQuantity { get; set; }

        [Display(Name = "የተሰራጨ መጠን")]
        [Required(ErrorMessage = "የተሰራጨ መጠን ያስፈልጋል")]
        [Range(0, double.MaxValue, ErrorMessage = "እባክዎ ትክክለኛ ቁጥር ያስገቡ")]
        public decimal DistributedQuantity { get; set; }

        [Display(Name = "ከቀረበው ውስጥ የተሰራጨው መቶኛ")]
        [Range(0, 100)]
        public decimal PercentageOfDistributedFromProvided { get; set; }

        [Display(Name = "ለጋሽ / ድጋፍ ሰጪ")]
        [StringLength(200)]
        public string? Donor { get; set; }
    }
}
