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
    public class ChangePasswordViewModel
    {
        public string? UserId { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Your password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
