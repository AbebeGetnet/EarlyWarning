using System.ComponentModel.DataAnnotations;
using EarlyWarning.Enums;

namespace EarlyWarning.Models
{
    public class AnimalDisease
    {
        public Guid Id { get; set; }
        [Display(Name = "የበሽታ ስም")]
        [Required(ErrorMessage = "የበሽታ ስም ያስፈልጋል።")]
        public string Name { get; set; }
    }
}
