using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.SupplyofGrain
{
    public class GrainSupplyDetail
    {
        [Key]
        public Guid GrainSupplyDetailId { get; set; }

        [Required]
        public Guid GrainSupplyId { get; set; }

        [Required]
        public Guid GrainIncreaseDecreaseId { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("GrainSupplyId")]
        public virtual GrainSupply? GrainSupply { get; set; }

        [ForeignKey("GrainIncreaseDecreaseId")]
        public virtual GrainIncreaseDecrease? GrainIncreaseDecrease { get; set; }
    }
}
