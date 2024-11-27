using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;

namespace UserAuthService.Attributes
{
    public class PasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Password cannot be null or empty.");
            }

            if (password.Length < 8 || password.Length > 16)
            {
                return new ValidationResult("Password must be between 8 and 16 characters long.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return new ValidationResult("Password must contain at least one digit.");
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?':{ }|<>]"))
            {
                return new ValidationResult("Password must contain at least one special character.");
            }

            return ValidationResult.Success;
        }
    }
}

