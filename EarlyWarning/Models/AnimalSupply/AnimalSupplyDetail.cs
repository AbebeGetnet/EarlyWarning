using EarlyWarning.Models.SupplyofGrain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.AnimalSupply
{
    public class AnimalSupplyDetail
    {
        [Key]
        public Guid AnimalSupplyDetailId { get; set; }

        [Required]
        public Guid AnimalSupplyId { get; set; }

        [Required]
        public Guid AnimalIncreaseDecreaseId { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("AnimalSupplyId")]
        public virtual AnimalSupply? AnimalSupply { get; set; }

        [ForeignKey("AnimalIncreaseDecreaseId")]
        public virtual AnimalIncreaseDecrease? AnimalIncreaseDecrease { get; set; }
    }
}
