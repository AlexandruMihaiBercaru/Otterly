using System.ComponentModel.DataAnnotations;

namespace Proj.Models.Validation;

public class SameOrAfterAttribute : ValidationAttribute
{
    private readonly string _otherName;
    private readonly string? _errorMessage;

    public SameOrAfterAttribute(string than, string? errorMessage = default)
    {
        _otherName = than;
        _errorMessage = errorMessage;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherProperty = validationContext.ObjectType.GetProperty(_otherName);
        if (otherProperty is null)
        {
            return new ValidationResult($"The property {_otherName} does not exist.");
        }

        var otherValue =
            otherProperty.GetValue(validationContext.ObjectInstance) as DateTimeOffset?;
        var thisValue = value as DateTimeOffset?;

        if (otherValue == null || thisValue == null)
        {
            return ValidationResult.Success;
        }

        if (thisValue < otherValue)
        {
            return new ValidationResult(_errorMessage ?? "The value is less than the other value.");
        }

        return ValidationResult.Success;
    }
}