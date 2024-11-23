using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserAuthService.Attributes
{
    public class EmailAddressAttribute : ValidationAttribute
    {
        // Define the regex pattern for a basic email validation
        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Check if the value is null or an empty string
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            // Check if the value matches the email pattern
            if (Regex.IsMatch(value.ToString(), EmailPattern))
            {
                return ValidationResult.Success;
            }

            // Return an error message if the email is not valid
            return new ValidationResult("Invalid email address format.");
        }
    }
}
