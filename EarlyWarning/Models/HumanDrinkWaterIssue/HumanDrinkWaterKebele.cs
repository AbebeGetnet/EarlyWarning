using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.HumanDrinkWaterIssue
{
    public class HumanDrinkWaterKebele
    {
        [Key]
        public Guid HumanDrinkWaterKebeleId { get; set; }

        [Required]
        public Guid HumanDrinkWaterIssueId { get; set; }

        // Navigation properties
        [ForeignKey("HumanDrinkWaterIssueId")]
        public virtual HumanDrinkWaterIssue? HumanDrinkWaterIssues { get; set; }
        [Required]
        public Guid KebeleId { get; set; }

        [ForeignKey("KebeleId")]
        public virtual Locations? Kebele { get; set; }


        [Display(Name = "የተመዘገበበት ቀን")]
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
    }
}
