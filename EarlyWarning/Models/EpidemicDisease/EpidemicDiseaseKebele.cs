using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.EpidemicDisease
{
    public class EpidemicDiseaseKebele
    {
        [Key]
        public Guid EpidemicDiseaseKebeleId { get; set; }

        [Required]
        public Guid EpidemicDiseaseReportId { get; set; }

        [Required]
        public Guid KebeleId { get; set; }

        // Navigation properties
        [ForeignKey("EpidemicDiseaseReportId")]
        public virtual EpidemicDisease? EpidemicDisease { get; set; }

        [ForeignKey("KebeleId")]
        public virtual Locations? Kebele { get; set; }


        [Display(Name = "የተመዘገበበት ቀን")]
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
    }
}
