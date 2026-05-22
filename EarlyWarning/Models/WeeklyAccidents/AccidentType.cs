using EarlyWarning.Models.AnimalPrice;
using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models.WeeklyAccidents
{
    public class AccidentType
    {
        public Guid AccidentTypeId{ get; set; }
        [Required(ErrorMessage = "የአደጋ ስም ያስፈልጋል")]
        [Display(Name = "የአደጋ ስም")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "የአደጋ ስም ከ2 እስከ 100 ቁምፊዎች መሆን አለበት")]
        public string AccidentName{ get; set; } = string.Empty;

        [Display(Name = "ንቁ ነው?")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<WeeklyAccidents>? WeeklyAccidents { get; set; }
    }
}
