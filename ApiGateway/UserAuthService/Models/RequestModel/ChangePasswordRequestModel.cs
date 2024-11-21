using System.ComponentModel.DataAnnotations;

namespace UserAuthService.Models.RequestModel
{
    public class ChangePasswordRequestModel
    {
        public string UserId { get; set; }
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        //[Required]
        //[Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        //public string ConfirmNewPassword { get; set; }
    }
}