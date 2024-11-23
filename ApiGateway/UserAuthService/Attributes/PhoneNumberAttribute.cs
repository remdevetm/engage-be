using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserAuthService.Attributes
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        private static readonly Regex E164Regex = new Regex(@"^\+[1-9]\d{1,14}$");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var phoneNumber = value as string;
            if (phoneNumber != null && E164Regex.IsMatch(phoneNumber))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid phone number format. Expected format: [+][country code][subscriber number including area code]. Example: +12345678900");
        }
    }
}
