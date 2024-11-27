using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserAuthService.Attributes
{
    public class OtpAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otp = value as string;

            if (string.IsNullOrWhiteSpace(otp))
            {
                return new ValidationResult("OTP cannot be null or empty.");
            }

            // Ensure OTP is exactly 5 digits
            if (!Regex.IsMatch(otp, @"^\d{5}$"))
            {
                return new ValidationResult("OTP must be exactly 5 numeric digits.");
            }

            return ValidationResult.Success;
        }
    }
}
