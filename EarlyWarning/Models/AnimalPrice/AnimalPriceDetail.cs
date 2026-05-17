using EarlyWarning.Models.PriceofGrain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.AnimalPrice
{
    public class AnimalPriceDetail
    {
        [Key]
        public Guid AnimalPriceDetailId { get; set; }

        [Required]
        public Guid AnimalPriceId { get; set; }

        [Required]
        public Guid AnimalPriceIncreaseDecreaseId { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("AnimalPriceId")]
        public virtual AnimalPrice? AnimalPrice { get; set; }

        [ForeignKey("AnimalPriceIncreaseDecreaseId")]
        public virtual AnimalPriceIncreaseDecrease? AnimalPriceIncreaseDecrease { get; set; }
    }
}
