using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models
{
    public class Locations
    {
        public Locations()
        {
            IsActive= true;
        }
        [Key]
        public Guid Id { get; set; }

        [DisplayName("Location Name")]
        [Required]
        public string? LocationName { get; set; }
        // Optional: imported lists sometimes only have the English name. Display code should fall back to LocationName.
        [DisplayName("የቦታው ስም")]
        public string? LocationAmharicName { get; set; }
        [DisplayName("ኮድ")]
        [Required]
        public string? LocationCode { get; set; }
        [DisplayName("ስልክ")]
        [Required]
        public string? PhoneNumber { get; set; }
        [DisplayName("ደረጃ")]
        [Required]
        public LocationLevel? Level { get; set; }
        [DisplayName("ሔደር")]
        public string? CardHeaderTitle { get; set; }
        public bool IsActive { get; set; } = true;

        // WGS84 point (SRID 4326). The database also keeps a computed geography column (GeoPoint)
        // built from these two values for spatial queries.
        [DisplayName("ኬክሮስ (Latitude)")]
        [Range(-90, 90)]
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }
        [DisplayName("ኬንትሮስ (Longitude)")]
        [Range(-180, 180)]
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        /// <summary>Name for display: the Amharic name, or the English name when no Amharic name is recorded.
        /// Not stored; in database queries use <c>LocationAmharicName ?? LocationName</c> instead.</summary>
        [NotMapped]
        public string DisplayName => !string.IsNullOrWhiteSpace(LocationAmharicName) ? LocationAmharicName! : LocationName ?? "";

        public Guid? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        [DisplayName("እናት ተቋም")]
        public virtual Locations? Parent { get; set; }
        public ICollection<Locations> Children { get; set; }  
    }
}
