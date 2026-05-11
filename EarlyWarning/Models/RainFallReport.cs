using EarlyWarning.Enums;
using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EarlyWarning.Models
{
    public class RainfallReport : CommonAttribute
    {

        [Required]
        [DisplayName("ወረዳ")]
        public Guid WoredaId { get; set; }

        [ForeignKey("WoredaId")]
        public virtual Locations Woreda { get; set; } = null!;

        // የሽፋን ብዛት (በእያንዳንዱ ምድብ ያሉ ቀበሌዎች ብዛት)

        [DisplayName("ሙሉ ሽፋን ያገኙ ቀበሌዎች")]
        public int FullCoverageKebeles { get; set; } = 0;
        public string? FullCoveredKebelieList { get; set; }

        [DisplayName("ከፊል ሽፋን ያገኙ ቀበሌዎች")]
        public int PartialCoverageKebeles { get; set; } = 0;

        [DisplayName("ዝናብ ያልዘነበባቸው ቀበሌዎች")]
        public int NoRainKebeles { get; set; } = 0;
        public string? NoRainKebelieList { get; set; }

        [DisplayName("መረጃ ያልተገኘባቸው ቀበሌዎች")]
        public int NoDataKebeles { get; set; } = 0;

        // የዝናብ መጠን ምድቦች

        [DisplayName("ዝቅተኛ መጠን ያላቸው ቀበሌዎች")]
        public int LowAmountKebeles { get; set; } = 0;
        public string? LowRainKebeliesList { get; set; } 

        [DisplayName("ዝቅተኛ - መካከለኛ ዝናብ ያገኙ ቀበሌዎች")]
        public int LowMediumAmountKebeles { get; set; } = 0;

        [DisplayName("መካከለኛ ዝናብ ያገኙ ቀበሌዎች")]
        public int MediumAmountKebeles { get; set; } = 0;

        [DisplayName("መካከለኛ - ከፍተኛ ዝናብ ያገኙ ቀበሌዎች")]
        public int MediumHighAmountKebeles { get; set; } = 0;

        [DisplayName("ከፍተኛ ዝናብ ያገኙ ቀበሌዎች")]
        public int HighAmountKebeles { get; set; } = 0;
        public string? HighRainKebelieList { get; set; } 

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

        //json formated kebele list for each rain coverage and rain amount category
        [DisplayName("በዝናብ ብዛት የተጎዱ ቀበሌዎች ዝርዝር")]
        public string? AffectedKebelesJson { get; set; }

        [NotMapped]
        [DisplayName("በከፍተኛ ዝናብ የተጎዱ ቀበሌዎች")]
        public List<Guid> SelectedKebeleIds { get; set; } = new();

        // Helper method to serialize/deserialize JSON
        public void SerializeKebeles()
        {
            AffectedKebelesJson = JsonSerializer.Serialize(SelectedKebeleIds);
        }

        public void DeserializeKebeles()
        {
            if (!string.IsNullOrEmpty(AffectedKebelesJson))
                SelectedKebeleIds = JsonSerializer.Deserialize<List<Guid>>(AffectedKebelesJson) ?? new();
            else
                SelectedKebeleIds = new();
        }

        // For drought
        [DisplayName("በድርቅ የተጎዱ ቀበሌዎች ዝርዝር")]
        public string? DroughtAffectedKebelesJson { get; set; }

        [NotMapped]
        public List<Guid> SelectedDroughtKebeleIds { get; set; } = new();

        public void SerializeDroughtKebeles()
        {
            DroughtAffectedKebelesJson = JsonSerializer.Serialize(SelectedDroughtKebeleIds);
        }

        public void DeserializeDroughtKebeles()
        {
            SelectedDroughtKebeleIds = string.IsNullOrEmpty(DroughtAffectedKebelesJson)
                ? new List<Guid>()
                : JsonSerializer.Deserialize<List<Guid>>(DroughtAffectedKebelesJson) ?? new();
        }
    }
}
