using System.ComponentModel.DataAnnotations;
using UserAuthService.Attributes;
using EmailAddressAttribute = UserAuthService.Attributes.EmailAddressAttribute;

namespace UserAuthService.Models.RequestModel
{
    public class SendEmailOtpRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
