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

        [Range(0, int.MaxValue)] public int MaleHouseholdHeads { get; set; }
        [Range(0, int.MaxValue)] public int FemaleHouseholdHeads { get; set; }
        [Range(0, int.MaxValue)] public int MaleFamilyMembers { get; set; }
        [Range(0, int.MaxValue)] public int FemaleFamilyMembers { get; set; }
        [Range(0, int.MaxValue)] public int MaleChildren { get; set; }
        [Range(0, int.MaxValue)] public int FemaleChildren { get; set; }
        [Range(0, int.MaxValue)] public int MaleYouth { get; set; }
        [Range(0, int.MaxValue)] public int FemaleYouth { get; set; }
        [Range(0, int.MaxValue)] public int MaleElderly { get; set; }
        [Range(0, int.MaxValue)] public int FemaleElderly { get; set; }
        [Range(0, int.MaxValue)] public int MaleDisabled { get; set; }
        [Range(0, int.MaxValue)] public int FemaleDisabled { get; set; }
        [Range(0, int.MaxValue)] public int LactatingMothers { get; set; }
        [Range(0, int.MaxValue)] public int PregnantWomen { get; set; }

        
        }
}
