using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BO;

namespace PL;

public class StatusToColorConverter : IValueConverter
{
    // המרה מהנתונים (האם פעיל?) לצבע (Brush)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // אנחנו מצפים לקבל בוליאני (true/false)
        if (value is bool isActive)
        {
            if (!isActive) // אם לא פעיל
            {
                // מחזירים צבע אדמדם/אפרפר בהיר לסימון
                return Brushes.LavenderBlush;
            }
        }

        // אם פעיל - צבע רגיל (שקוף/לבן)
        return Brushes.Transparent;
    }

    // המרה הפוכה (לא רלוונטי לצבעים, לכן זורקים שגיאה)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

// בודק אם אובייקט הוא NULL.
// אם האובייקט קיים (לא NULL) -> מחזיר Visible.
// אם האובייקט ריק (NULL) -> מחזיר Collapsed.
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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

public class DeleteVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CourierInList courier)
        {
            // שליח עם משלוחים בעבר - אי אפשר למחוק
            bool hasHistory = (courier.TotalOnTimeDeliveries + courier.TotalLateDeliveries) > 0;

            // שליח פעיל כרגע - אי אפשר למחוק
            // (במקום לבדוק CurrentOrder שאין לך, נבדוק אם יש לו משלוח פעיל לפי הנתונים שיש)
            // נניח שאין גישה ל-CurrentOrder, נסתמך כרגע רק על ההיסטוריה כי זה בטוח
            // או שנבדוק IsActive אם זה אומר שהוא זמין או לא

            if (!hasHistory)
            {
                return Visibility.Visible; // אפשר למחוק
            }
        }
        return Visibility.Collapsed; // אי אפשר למחוק
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}