using System.ComponentModel.DataAnnotations;
using UserAuthService.Attributes;

namespace UserAuthService.Models.RequestModel
{
    public class ChangePasswordRequestModel
    {
        [Required]
        [Hex24String]
        public string UserId { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [Password]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}