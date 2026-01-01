using System;
using System.Globalization;
using System.Windows; // חובה בשביל Visibility
using System.Windows.Controls;
using System.Windows.Data;

namespace PL.Converters;

// ממיר 1: טקסט לכפתור
public class IdToContentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int id && id == 0) return "Add";
        return "Update";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ממיר 2: נעילת שדות
public class IdToIsReadOnlyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int id && id == 0) return false;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ממיר 3: הסתרת שדות (החדש)
public class IdToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // אם ה-ID הוא 0 (הוספה) -> מציג
        if (value is int id && id == 0)
            return Visibility.Visible;

        // אחרת (עדכון) -> מסתיר
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ממיר שמציג מחרוזת ריקה אם המספר הוא 0 (מונע את הופעת ה-0 בהתחלה)
public class ZeroToEmptyStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i && i == 0) return "";
        return value.ToString()!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value as string)) return 0;
        if (int.TryParse(value as string, out int result)) return result;
        return 0;
    }
}

public class BooleanToButtonTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isUpdate && isUpdate) return "Update";
        return "Add";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ממיר חדש: בודק אם אובייקט הוא NULL.
// אם האובייקט קיים (לא NULL) -> מחזיר Visible.
// אם האובייקט ריק (NULL) -> מחזיר Collapsed.
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotEmptyValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult(false, "Field is required.");
        }
        return ValidationResult.ValidResult;
    }
}