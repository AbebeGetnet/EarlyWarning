using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models
{
    public class CropPestAndDesease
    {
        public Guid Id { get; set; }
        [Display(Name = "የተባይ/በሽታ ስም")]
        [Required(ErrorMessage = "የተባይ/በሽታ ስም ያስፈልጋል።")]
        public string Name { get; set; }
        [Display(Name = "አይነት")]
        public CPDType CPDType { get; set; }
    }
}
