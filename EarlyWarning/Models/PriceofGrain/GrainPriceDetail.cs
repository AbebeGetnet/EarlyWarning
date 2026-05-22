using EarlyWarning.Models.SupplyofGrain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.PriceofGrain
{
    public class GrainPriceDetail
    {
        [Key]
        public Guid GrainPriceDetailId { get; set; }

        [Required]
        public Guid GrainPriceId { get; set; }

        [Required]
        public Guid GrainPriceIncreaseDecreaseId { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("GrainPriceId")]
        public virtual GrainPrice? GrainPrice { get; set; }

        [ForeignKey("GrainPriceIncreaseDecreaseId")]
        public virtual GrainPriceIncreaseDecrease? GrainPriceIncreaseDecrease { get; set; }
    }
}
