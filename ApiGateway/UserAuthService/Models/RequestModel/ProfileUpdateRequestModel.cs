using System.ComponentModel.DataAnnotations;
using UserAuthService.Attributes;

namespace UserAuthService.Models.RequestModel
{
    public class ProfileUpdateRequestModel
    {
        [Required]
        [Hex24String]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string WorkingHours { get; set; }
        [Required]
        public string Position { get; set; }
    }
}
