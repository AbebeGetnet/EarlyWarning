using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.EpidemicDisease
{
    public class EpidemicDisease
    {
        [Key]
        public Guid EpidemicDiseaseId { get; set; }

        [Required(ErrorMessage = "የመጀመሪያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መነሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "የማብቂያ ቀን ያስፈልጋል")]
        [Display(Name = "የሪፖርት መጨረሻ ቀን")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "በሳምንቱ ወረርሽኝ መልኩ የተከሰተ በሽታ አለ?")]
        public bool HasEpidemicDisease { get; set; }

        [Display(Name = "በሽታው የተስፋፋባቸው ቀበሌዎች ቁጥር")]
        [Range(0, int.MaxValue)]
        public int NumberOfAffectedKebeles { get; set; }

        // በበሽታው የተጎዱ ሰዎች በምድብ
        [Display(Name = "የተጎዱ አባወራ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleHouseholdHeads { get; set; }

        [Display(Name = "የተጎዱ እማወራ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleHouseholdHeads { get; set; }

        [Display(Name = "የተጎዱ ቤተሰብ (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleFamilyMembers { get; set; }

        [Display(Name = "የተጎዱ ቤተሰብ (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleFamilyMembers { get; set; }

        [Display(Name = "የተጎዱ ህጻናት (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleChildren { get; set; }

        [Display(Name = "የተጎዱ ህጻናት (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleChildren { get; set; }

        [Display(Name = "የተጎዱ ወጣቶች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleYouth { get; set; }

        [Display(Name = "የተጎዱ ወጣቶች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleYouth { get; set; }

        [Display(Name = "የተጎዱ አረጋውያን (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleElderly { get; set; }

        [Display(Name = "የተጎዱ አረጋውያን (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleElderly { get; set; }

        [Display(Name = "የተጎዱ አካል ጉዳተኞች (ወንድ)")]
        [Range(0, int.MaxValue)]
        public int MaleDisabled { get; set; }

        [Display(Name = "የተጎዱ አካል ጉዳተኞች (ሴት)")]
        [Range(0, int.MaxValue)]
        public int FemaleDisabled { get; set; }

        // 🆕 አጥቢ እናቶች ብዛት (Lactating Mothers)
        [Display(Name = "አጥቢ እናቶች ብዛት")]
        [Range(0, int.MaxValue, ErrorMessage = "እባክዎ ትክክለኛ ቁጥር ያስገቡ")]
        public int LactatingMothers { get; set; }

        // 🆕 ነፍሰጡር እናቶች ብዛት (Pregnant Women)
        [Display(Name = "ነፍሰጡር እናቶች ብዛት")]
        [Range(0, int.MaxValue, ErrorMessage = "እባክዎ ትክክለኛ ቁጥር ያስገቡ")]
        public int PregnantWomen { get; set; }
        //// Computed Properties
        //[Display(Name = "አጠቃላይ የተጎዱ ወንዶች")]
        //public int TotalMales => MaleHouseholdHeads + MaleFamilyMembers + MaleChildren + MaleYouth + MaleElderly + MaleDisabled;

        //[Display(Name = "አጠቃላይ የተጎዱ ሴቶች")]
        //public int TotalFemales => FemaleHouseholdHeads + FemaleFamilyMembers + FemaleChildren + FemaleYouth + FemaleElderly + FemaleDisabled;

        //[Display(Name = "አጠቃላይ የተጎዳ ህዝብ ብዛት")]
        //public int TotalAffectedPopulation => TotalMales + TotalFemales;

        // የወረዳ መረጃ
        [Display(Name = "ወረዳ")]
        [Required(ErrorMessage = "ወረዳ መምረጥ ያስፈልጋል")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations? Woreda { get; set; }

        // ሪፖርት ማዘጋጃ መረጃ
        [Display(Name = "ሪፖርት የተዘጋጀበት ቀን")]
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Display(Name = "ሪፖርት ያዘጋጀ ሰው")]
        public string? ReportedBy { get; set; }

        [Display(Name = "ተጨማሪ ማስታወሻ")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        public string? Notes { get; set; }

        //public Guid EpidemicDiseaseTypeId { get; set; }

        //[ForeignKey("EpidemicDiseaseTypeId")]
        //public EpidemicDiseaseType EpidemicDiseaseType { get; set; }
        // Navigation properties
        public ICollection<EpidemicDiseaseKebele>? EpidemicDiseaseKebeles { get; set; }
        // 🆕 Navigation property for EpidemicDiseaseSelection (One-to-Many)
        public virtual ICollection<EpidemicDiseaseSelection>? EpidemicDiseaseSelections { get; set; }
    }
}
