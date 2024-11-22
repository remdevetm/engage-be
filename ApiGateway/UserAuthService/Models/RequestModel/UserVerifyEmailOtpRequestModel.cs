using System.ComponentModel.DataAnnotations;

namespace UserAuthService.Models.RequestModel
{
    public class UserVerifyEmailOtpRequestModel
    {

        [Required]
        public string Otp { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
