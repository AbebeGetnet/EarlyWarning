using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.Migration
{
    public class DeathDetail
    {
        [Key]
        public Guid DeathDetailId { get; set; }

        [Required]
        public Guid DeathId { get; set; }

        [ForeignKey("DeathId")]
        public virtual Death? Death { get; set; }

        [Display(Name = "የሞት ምክንያት")]
        [StringLength(500)]
        public string? DeathReason { get; set; }

        // የተሰደዱ ሰዎች በምድብ
        [Display(Name = "የሞተ አባወራ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleHouseholdHeads { get; set; }

        [Display(Name = "የሞተ እማወራ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleHouseholdHeads { get; set; }

        [Display(Name = "የሞተ ቤተሰብ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleFamilyMembers { get; set; }

        [Display(Name = "የሞተ ቤተሰብ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleFamilyMembers { get; set; }

        [Display(Name = "የሞተ ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleChildren { get; set; }

        [Display(Name = "የሞተ ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleChildren { get; set; }

        [Display(Name = "የሞተ ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleYouth { get; set; }

        [Display(Name = "የሞተ ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleYouth { get; set; }

        [Display(Name = "የሞተ አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleElderly { get; set; }

        [Display(Name = "የሞተ አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleElderly { get; set; }

        [Display(Name = "የሞተ አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleDisabled { get; set; }

        [Display(Name = "የሞተ አካል ጉዳተኞች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleDisabled { get; set; }

        //// Computed Properties
        //[Display(Name = "አጠቃላይ የተሰደዱ ወንዶች")]
        //public int TotalMales => MaleHouseholdHeads + MaleFamilyMembers + MaleChildren + MaleYouth + MaleElderly + MaleDisabled;

        //[Display(Name = "አጠቃላይ የተሰደዱ ሴቶች")]
        //public int TotalFemales => FemaleHouseholdHeads + FemaleFamilyMembers + FemaleChildren + FemaleYouth + FemaleElderly + FemaleDisabled;


        //[Display(Name = "አጠቃላይ የተሰደዱ ሰዎች")]
        //public int TotalMigrants => TotalMales + TotalFemales;

        [Display(Name = "ማስታወሻ")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
