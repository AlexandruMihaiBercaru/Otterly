using System.ComponentModel.DataAnnotations;

namespace Proj.Models.Validation;

public class OneOf(params string[] allowedValues) : ValidationAttribute
{
  private readonly string[] _allowedValues = allowedValues;

  protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
  {
    if (value is null)
    {
      return ValidationResult.Success;
    }

    if (value is string s)
    {
      if (_allowedValues.Contains(s))
      {
        return ValidationResult.Success;
      }
    }

    var allowedValuesString = string.Join(", ", _allowedValues);
    return new ValidationResult(
        $"The value must be one of the following: {allowedValuesString}.");
  }
}