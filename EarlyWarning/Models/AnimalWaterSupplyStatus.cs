using EarlyWarning.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EarlyWarning.Models
{
    public class AnimalWaterSupplyStatus : CommonAttribute
    {
        [DisplayName("በቂ ነው")]
        public bool Enough { get; set; }
               
        [DisplayName("በሳምንቱ የግጦሽ ችግር ያለባቸው ቀበሌዎች ብዛት")]
        public int NoOfKebeliesWithPastureShortage { get; set; }

        [DisplayName("በሳምንቱ የውሃ ችግር ያለባቸው ቀበሌዎች በቁጥር")]
        public int NoOfKebeliesWithWaterSupply { get; set; }

        [DisplayName("ወንድ አባወራ")]
        public int MaleHouseHold { get; set; }

        [DisplayName("ሴት እማወራ")]
        public int FemaleHouseHold { get; set; }

        [DisplayName("ወንድ ቤተሰብ")]
        public int MaleFamily { get; set; }

        [DisplayName("ሴት ቤተሰብ")]
        public int FemaleFamily { get; set; }
        [DisplayName("ከ5 አመት በታች ህጻናት (ወንድ)")]
        public int ChildhMale { get; set; }

        [DisplayName("ህጻናት (ሴት)")]
        public int ChildFemale { get; set; }

        [DisplayName("ወጣቶች (ወንድ)")]
        public int YouthMale { get; set; }

        [DisplayName("ወጣቶች (ሴት)")]
        public int YouthFemale { get; set; }

        [DisplayName("አረጋውያን (ወንድ)")]
        public int ElderlyMale { get; set; }

        [DisplayName("አረጋውያን (ሴት)")]
        public int ElderlyFemale { get; set; }

        [DisplayName("አካል ጉዳተኞች (ወንድ)")]
        public int DisabledMale { get; set; }
        [NotMapped]
        public int TotalMale => MaleHouseHold + MaleFamily + ChildhMale + YouthMale + ElderlyMale + DisabledMale;

        [DisplayName("አካል ጉዳተኞች (ሴት)")]
        public int DisabledFemale { get; set; }
        [NotMapped]
        public int TotalFemale => FemaleHouseHold + MaleFamily + ChildFemale + YouthFemale + ElderlyFemale + DisabledFemale;
        [NotMapped]
        public int TotalAffected => TotalFemale + TotalMale;

        [Required]
        [DisplayName("ወረዳ")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations Woreda { get; set; } = null!;
        // ሁኔታ እና ማጽደቂያዎች

        [DisplayName("ሁኔታ")]
        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        [DisplayName("የተላከበት ቀን")]
        public DateTime? SubmittedAt { get; set; }

        [DisplayName("ያስገባው ተጠቃሚ")]
        public string? SubmittedById { get; set; }

        [DisplayName("በዞን የጸደቀበት ቀን")]
        public DateTime? ZoneApprovedAt { get; set; }

        [DisplayName("በዞን ያጸደቀው ተጠቃሚ")]
        public string? ZoneApprovedById { get; set; }

        [DisplayName("በክልል የጸደቀበት ቀን")]
        public DateTime? RegionApprovedAt { get; set; }

        [DisplayName("በክልል ያጸደቀው ተጠቃሚ")]
        public string? RegionApprovedById { get; set; }

        // Zone rejection details
        [DisplayName("የዞን ውድቅ ማብራሪያ")]
        public string? ZoneRejectionRemark { get; set; }

        [DisplayName("በዞን ውድቅ የተደረገበት ቀን")]
        public DateTime? ZoneRejectedAt { get; set; }

        [DisplayName("በዞን ውድቅ ያደረገው ተጠቃሚ")]
        public string? ZoneRejectedById { get; set; }

        // Region rejection details
        [DisplayName("የክልል ውድቅ ማብራሪያ")]
        public string? RegionRejectionRemark { get; set; }

        [DisplayName("በክልል ውድቅ የተደረገበት ቀን")]
        public DateTime? RegionRejectedAt { get; set; }

        [DisplayName("በክልል ውድቅ ያደረገው ተጠቃሚ")]
        public string? RegionRejectedById { get; set; }

        // ተጨማሪ ማብራሪያ

        [DisplayName("ማስታወሻ")]
        [MaxLength(500)]
        public string? Remarks { get; set; }

    }
}
