using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// Represents a validation rule that checks whether a value is not null, empty, or consists only of white-space
/// characters.
/// </summary>
/// <remarks>This rule is typically used in data binding scenarios to ensure that required fields are provided by
/// the user. It treats <see langword="null"/> values and values that convert to empty or white-space strings as
/// invalid.</remarks>
public class NotEmptyValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // Convert the input value to a string, treating null as an empty string
        string str = value?.ToString() ?? "";

        return string.IsNullOrWhiteSpace(str)
            ? new ValidationResult(false, "Field is required.")
            : ValidationResult.ValidResult;
    }
}

/// <summary>
/// Provides a validation rule that checks whether a value conforms to a standard email address format.
/// </summary>
/// <remarks>This rule considers empty or whitespace-only values as valid. To require a non-empty email address,
/// use an additional <c>NotEmpty</c> validation rule. The validation checks for a basic email pattern and does not
/// guarantee that the email address exists or is deliverable.</remarks>
public class EmailValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string str = value?.ToString() ?? "";

        // Allow empty values to pass validation
        if (string.IsNullOrWhiteSpace(str)) return ValidationResult.ValidResult;

        return Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Invalid email format.");
    }
}

/// <summary>
/// Provides a validation rule that checks whether a value can be parsed as a number.
/// </summary>
public class NumberValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string str = value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(str)) return ValidationResult.ValidResult;

        return double.TryParse(str, out _)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Must be a number.");
    }
}


public class PhoneValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string str = value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(str)) return new ValidationResult(false, "Phone is required.");

        return (str.All(char.IsDigit) && str.Length >= 9 && str.Length <= 10)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Phone must be 9-10 digits.");
    }
}

/// <summary>
/// Represents a validation rule that checks whether a value meets a minimum length requirement.
/// </summary>
public class MinLengthValidationRule : ValidationRule
{
    public int MinLength { get; set; } = 4; // Default is 4 characters

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string str = value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(str)) return ValidationResult.ValidResult;

        return str.Length >= MinLength
            ? ValidationResult.ValidResult
            : new ValidationResult(false, $"Min {MinLength} chars.");
    }
}