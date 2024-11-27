using System.ComponentModel.DataAnnotations;
using UserAuthService.Attributes;
using EmailAddressAttribute = UserAuthService.Attributes.EmailAddressAttribute;

namespace UserAuthService.Models.RequestModel
{
    public class LoginRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Password]
        public string Password { get; set; }
    }
}
