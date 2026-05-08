using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models
{
    public class Locations : CommonAttribute
    {
        public Locations()
        {
            IsActive= true;
        }
        [DisplayName("Location Name")]
        [Required]
        public string? LocationName { get; set; }
        [DisplayName("የቦታው ስም")]
        [Required]
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
        public Guid? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        [DisplayName("እናት ተቋም")]
        public virtual Locations? Parent { get; set; }
        public ICollection<Locations> Children { get; set; }  
    }
}
