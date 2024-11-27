using System.ComponentModel.DataAnnotations;
using UserAuthService.Attributes;
using EmailAddressAttribute = UserAuthService.Attributes.EmailAddressAttribute;

namespace UserAuthService.Models.RequestModel
{
    public class AgentRegisterRequestModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string WorkingHours { get; set; }
        [Required]
        public string Position { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
