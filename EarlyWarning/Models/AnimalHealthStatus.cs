using EarlyWarning.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EarlyWarning.Models
{
    public class AnimalHealthStatus : CommonAttribute
    {
        [DisplayName("አለ")]
        public bool Enough { get; set; }

        [DisplayName("ወንድ አባወራ")]
        public int MaleHouseHold { get; set; }

        [DisplayName("ሴት እማወራ")]
        public int FemaleHouseHold { get; set; }
        [NotMapped]
        public int TotalHouseHold => FemaleHouseHold + MaleHouseHold;

        [DisplayName("ወንድ ቤተሰብ")]
        public int MaleFamily { get; set; }

        [DisplayName("ሴት ቤተሰብ")]
        public int FemaleFamily { get; set; }
        [NotMapped]
        public int TotalFamily => MaleFamily + FemaleFamily;
        [DisplayName("ከ5 አመት በታች ህጻናት (ወንድ)")]
        public int ChildhMale { get; set; }

        [DisplayName("ህጻናት (ሴት)")]
        public int ChildFemale { get; set; }
        [NotMapped]
        public int TotalChild => ChildFemale + ChildhMale;

        [DisplayName("ወጣቶች (ወንድ)")]
        public int YouthMale { get; set; }

        [DisplayName("ወጣቶች (ሴት)")]
        public int YouthFemale { get; set; }
        [NotMapped]
        public int Totalyouth => YouthFemale + YouthMale;
        [DisplayName("አረጋውያን (ወንድ)")]
        public int ElderlyMale { get; set; }

        [DisplayName("አረጋውያን (ሴት)")]
        public int ElderlyFemale { get; set; }
        [NotMapped]
        public int TotalElderly => ElderlyFemale + ElderlyMale;

        [DisplayName("አካል ጉዳተኞች (ወንድ)")]
        public int DisabledMale { get; set; }
        [DisplayName("አካል ጉዳተኞች (ሴት)")]
        public int DisabledFemale { get; set; }
        [NotMapped]
        public int TotalDisable => DisabledFemale + DisabledMale;
        [NotMapped]
        public int TotalMale => MaleHouseHold + MaleFamily + ChildhMale + YouthMale + ElderlyMale + DisabledMale;

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

        //serialization for crop diseases and pests
        [DisplayName("የሰብል በሽታዎች / ተባዮች (JSON)")]
        public string? AnimalDiseaseJson { get; set; }

        [NotMapped]
        public List<Guid> SelectedDiseaseIds { get; set; } = new List<Guid>();
        [NotMapped]
        public List<string> DiseaseNames { get; set; } = new List<string>();
        public void SerializeCropDiseases()
        {
            AnimalDiseaseJson = JsonSerializer.Serialize(SelectedDiseaseIds);
        }

        public void DeserializeCropDiseases()
        {
            SelectedDiseaseIds = string.IsNullOrEmpty(AnimalDiseaseJson)
                ? new List<Guid>()
                : JsonSerializer.Deserialize<List<Guid>>(AnimalDiseaseJson) ?? new();
        }

    }
}
