using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Models
{
    public class PasswordResetKey
    {
        public Guid Id { get; set; }
        //public string UserId { get; set; }
        //public ApplicationUser? User { get; set; }
        public string PasswordResetValue { get; set; }
        public string? KeyNote { get; set; }
    }
    /// <summary>The signed-in user changing their own password.</summary>
    public class ChangePasswordViewModel
    {
        public string? UserId { get; set; }

        [Required(ErrorMessage = "የአሁኑን የይለፍ ቃል ያስገቡ።")]
        [DataType(DataType.Password)]
        [Display(Name = "የአሁኑ የይለፍ ቃል")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "አዲሱን የይለፍ ቃል ያስገቡ።")]
        [StringLength(100, ErrorMessage = "የይለፍ ቃሉ ቢያንስ {2} ፊደላት መሆን አለበት።", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "አዲስ የይለፍ ቃል")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "አዲሱን የይለፍ ቃል በድጋሚ ያስገቡ።")]
        [DataType(DataType.Password)]
        [Display(Name = "አዲሱን የይለፍ ቃል ያረጋግጡ")]
        [Compare("NewPassword", ErrorMessage = "አዲሱ የይለፍ ቃልና ማረጋገጫው አይመሳሰሉም።")]
        public string ConfirmPassword { get; set; } = "";
    }
}
