using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PL
{
    // --- בדיקה שהשדה לא ריק ---
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // התיקון לאזהרה: הופכים לסטרינג בצורה בטוחה. אם זה null, זה יהפוך למחרוזת ריקה ""
            string str = value?.ToString() ?? "";

            return string.IsNullOrWhiteSpace(str)
                ? new ValidationResult(false, "Field is required.")
                : ValidationResult.ValidResult;
        }
    }

    // --- בדיקה שזה אימייל תקין ---
    public class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value?.ToString() ?? "";

            // אם ריק זה בסדר (אלא אם הוגדר בנפרד NotEmpty)
            if (string.IsNullOrWhiteSpace(str)) return ValidationResult.ValidResult;

            return Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid email format.");
        }
    }

    // --- בדיקה שזה מספר ---
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

    // --- בדיקה לטלפון (מספרים בלבד, 9-10 ספרות) ---
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

    // --- בדיקה לאורך מינימלי (לסיסמאות) ---
    public class MinLengthValidationRule : ValidationRule
    {
        public int MinLength { get; set; } = 4; // ברירת מחדל 4 תווים

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(str)) return ValidationResult.ValidResult;

            return str.Length >= MinLength
                ? ValidationResult.ValidResult
                : new ValidationResult(false, $"Min {MinLength} chars.");
        }
    }
}