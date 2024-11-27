using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserAuthService.Attributes
{
    public class Hex24StringAttribute : ValidationAttribute
    {
        private const string HexPattern = @"^[a-fA-F0-9]{24}$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string hexString && Regex.IsMatch(hexString, HexPattern))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Id must be a 24-character hex string.");
        }
    }
}
