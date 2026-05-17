using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.PriceofGrain
{
    public class GrainPriceItem
    {
        
        [Required(ErrorMessage = "የእህል አይነት መምረጥ ያስፈልጋል")]
        [Display(Name = "የእህል አይነት")]
        public Guid GrainTypeId { get; set; }

        [Required(ErrorMessage = "የእህል ዋጋ ያስፈልጋል")]
        [Display(Name = "ዋጋ (በኩንታል)")]
        [Range(0, 100000, ErrorMessage = "ዋጋ ከ0 እስከ 100,000 ብር መሆን አለበት")]
        [DataType(DataType.Currency)]
        public decimal WeeklyPrice { get; set; }

        [Required(ErrorMessage = "የዋጋ ሁኔታ መምረጥ ያስፈልጋል")]
        [Display(Name = "የዋጋ ሁኔታ")]
        public WeeklyStatus WeeklyMarketStatus { get; set; }

        [Display(Name = "ዋጋ ማስታወሻ")]
        [StringLength(500)]
        public string? PriceNote { get; set; }
    }

        
    
}
