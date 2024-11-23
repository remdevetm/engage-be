using System.ComponentModel.DataAnnotations;

namespace UserAuthService.Models.RequestModel
{
    public class ProfileUpdateRequestModel
    {
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
