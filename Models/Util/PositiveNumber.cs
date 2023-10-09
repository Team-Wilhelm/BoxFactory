using System.ComponentModel.DataAnnotations;

namespace Models.Util;

public class PositiveNumber : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult("Value cannot be null");
        }
        
        // Check if value is a float or an int
        if (value.GetType() != typeof(float) && value.GetType() != typeof(int))
        {
            return new ValidationResult("Value must be a float or an int");
        }

        if (value is float f && f <= 0)
        {
            return new ValidationResult("Value must be positive");
        }
        
        if (value is int i && i <= 0)
        {
            return new ValidationResult("Value must be positive");
        }
       
        return ValidationResult.Success;
    }
}