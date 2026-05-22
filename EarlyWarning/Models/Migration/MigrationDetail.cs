using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.Migration
{
    public class MigrationDetail
    {
        [Key]
        public Guid MigrationDetailId { get; set; }

        [Required]
        public Guid MigrationId { get; set; }

        [ForeignKey("MigrationId")]
        public virtual Migration? Migration { get; set; }

        [Display(Name = "የተሰደዱበት ቦታ (ከየት)")]
        [Required(ErrorMessage = "የተሰደዱበት ቦታ ያስፈልጋል")]
        public Guid? OriginLocationId { get; set; }

        [ForeignKey("OriginLocationId")]
        public virtual Locations? OriginLocation { get; set; }

        [Display(Name = "የስደት ምክንያት")]
        [StringLength(500)]
        public string? MigrationReason { get; set; }

        // የተሰደዱ ሰዎች በምድብ
        [Display(Name = "የተሰደዱ አባወራ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleHouseholdHeads { get; set; }

        [Display(Name = "የተሰደዱ እማወራ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleHouseholdHeads { get; set; }

        [Display(Name = "የተሰደዱ ቤተሰብ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleFamilyMembers { get; set; }

        [Display(Name = "የተሰደዱ ቤተሰብ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleFamilyMembers { get; set; }

        [Display(Name = "የተሰደዱ ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleChildren { get; set; }

        [Display(Name = "የተሰደዱ ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleChildren { get; set; }

        [Display(Name = "የተሰደዱ ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleYouth { get; set; }

        [Display(Name = "የተሰደዱ ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleYouth { get; set; }

        [Display(Name = "የተሰደዱ አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleElderly { get; set; }

        [Display(Name = "የተሰደዱ አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleElderly { get; set; }

        [Display(Name = "የተሰደዱ አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleDisabled { get; set; }

        [Display(Name = "የተሰደዱ አካል ጉዳተኞች (ሴት)")]
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
