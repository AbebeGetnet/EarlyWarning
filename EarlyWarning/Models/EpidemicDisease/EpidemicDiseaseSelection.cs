using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.EpidemicDisease
{
    public class EpidemicDiseaseSelection
    {
        [Key]
        public Guid EpidemicDiseaseSelectionId { get; set; }

        [Required]
        public Guid EpidemicDiseaseId { get; set; }
        [ForeignKey("EpidemicDiseaseId")]
        public virtual EpidemicDisease? EpidemicDisease { get; set; }

        [Required]
        public Guid EpidemicDiseaseTypeId { get; set; }
        [ForeignKey("EpidemicDiseaseTypeId")]
        public virtual EpidemicDiseaseType? EpidemicDiseaseType { get; set; }

        

        [Display(Name = "ማስታወሻ")]
        [StringLength(500)]
        public string? Note { get; set; }

        
        }
}
