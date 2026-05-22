using EarlyWarning.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EarlyWarning.Models
{
    public class CropPestAndDeseaseReport : CommonAttribute
    {
        [DisplayName("የሰብል ተባይ እና በሽታ አጋጥሟል?")]
        public bool HasPestAndDeseasOccured { get; set; }

        [DisplayName("አባወራ")]
        public int MaleHouseHold { get; set; }

        [DisplayName("እማወራ ")]
        public int FemaleHouseHold { get; set; }
        
        [DisplayName("ወንድ")]
        public int MaleFamily { get; set; }

        [DisplayName("ሴት")]
        public int FemaleFamily { get; set; }

        [DisplayName("ወንድ")]
        public int ChildhMale { get; set; }

        [DisplayName("ሴት")]
        public int ChildFemale { get; set; }
      

        [DisplayName("ወንድ")]
        public int YouthMale { get; set; }

        [DisplayName("ሴት")]
        public int YouthFemale { get; set; }
      

        [DisplayName("ወንድ")]
        public int ElderlyMale { get; set; }

        [DisplayName("ሴት")]
        public int ElderlyFemale { get; set; }      

        [DisplayName("ወንድ")]
        public int DisabledMale { get; set; }
        [DisplayName("ሴት")]
        public int DisabledFemale { get; set; }
        [DisplayName("ድምር")]
        [NotMapped]
        public int TotalHouseHold => MaleHouseHold + FemaleHouseHold;
        [DisplayName("ድምር")]
        [NotMapped]
        public int TotalFamily => MaleFamily + FemaleFamily;
        [DisplayName("ድምር")]
        [NotMapped]
        public int TotalChild => ChildhMale + ChildFemale;
        [DisplayName("ድምር")]
        [NotMapped]
        public int TotalYouth => YouthMale + YouthFemale;
        [DisplayName("ድምር")]
        [NotMapped]
        public int TotalElderly => ElderlyMale + ElderlyFemale;
        [DisplayName("ድምር")]
        [NotMapped]
        public int TotalDisabled => DisabledMale + DisabledFemale;
        [NotMapped]
        [DisplayName("ወንድ")]
        public int TotalMale => MaleHouseHold + MaleFamily + ChildFemale + YouthMale + ElderlyMale + DisabledMale;

        [NotMapped]
        [DisplayName("ሴት")]

        public int TotalFemale => FemaleHouseHold + FemaleFamily + ChildFemale + YouthFemale + ElderlyFemale + DisabledFemale;
        [NotMapped]
        [DisplayName("ጠቅላላ ድምር")]

        public int TotalAffected => TotalFemale + TotalMale;

        [DisplayName("በሰብል ተባይ የተጎዳ መሬት (በሄክታር)")]
        public float? AffectedLandInHectar { get; set; }

        [DisplayName("በሰብል ተባይ የተጎዳ ሰብል አይነት")]
        public string? TypeOfCropAffected { get; set; }

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
        public string? CropDiseasesJson { get; set; }

        [NotMapped]
        public List<Guid> SelectedDiseaseIds { get; set; } = new List<Guid>();
        [NotMapped]
        public List<string> DiseaseNames { get; set; } = new List<string>();
        public void SerializeCropDiseases()
        {
            CropDiseasesJson = JsonSerializer.Serialize(SelectedDiseaseIds);
        }

        public void DeserializeCropDiseases()
        {
            SelectedDiseaseIds = string.IsNullOrEmpty(CropDiseasesJson)
                ? new List<Guid>()
                : JsonSerializer.Deserialize<List<Guid>>(CropDiseasesJson) ?? new();
        }
    }
}
