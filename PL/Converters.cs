using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BO;

namespace PL;

/// <summary>
/// Converts a status value to a corresponding color brush for UI representation.
/// </summary>
/// <remarks>This value converter is typically used in data binding scenarios to visually indicate status, such as
/// active or inactive states, by mapping a boolean value to a <see cref="Brush"/>. For example, an inactive status may
/// be displayed with a specific color, while other states use a default color.</remarks>
public class StatusToColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value indicating status to a corresponding color brush.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive && !isActive)
            return Brushes.LavenderBlush;
        return Brushes.Transparent;
    }

    /// <summary>
    /// Converts a color brush back to a boolean value indicating status.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts an ID value to a corresponding content string for UI representation.
/// </summary>
public class IdToContentConverter : IValueConverter
{
    /// <summary>
    /// Converts an ID value to a corresponding content string for UI representation.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int id && id == 0) 
            return "Add";
        return "Update";
    }

    /// <summary>
    /// Converts a content string back to an ID value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts an ID value to a corresponding read-only state.
/// </summary>
public class IdToIsReadOnlyConverter : IValueConverter
{
    /// <summary>
    /// Converts an ID value to a corresponding read-only state.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int id && id == 0) 
            return false;
        return true;
    }

    /// <summary>
    /// Converts a read-only state back to an ID value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts an ID value to a corresponding visibility state.
/// </summary>
public class IdToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts an ID value to a corresponding visibility state.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int id && id == 0)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a visibility state back to an ID value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts a zero integer value to an empty string.
/// </summary>
public class ZeroToEmptyStringConverter : IValueConverter
{
    /// <summary>
    /// Converts a zero integer value to an empty string.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i && i == 0) 
            return "";
        return value.ToString()!;
    }

    /// <summary>
    /// Converts a zero integer value to an empty string.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value as string)) 
            return 0;
        if (int.TryParse(value as string, out int result)) 
            return result;
        return 0;
    }
}

/// <summary>
/// Converts a boolean value to a button title.
/// </summary>
public class BooleanToButtonTitleConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a button title.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isUpdate && isUpdate) 
            return "Update";
        return "Add";
    }

    /// <summary>
    /// Converts a boolean value back to a button title.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts a null value to a visibility state.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a null value to a visibility state.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isInverted = parameter?.ToString() == "Invert";

        if (value == null)
            return isInverted ? Visibility.Visible : Visibility.Collapsed;

        else
            return isInverted ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Converts a null value back to a visibility state.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Validation rule that checks if a field is not empty.
/// </summary>
public class NotEmptyValidationRule : ValidationRule
{
    /// <summary>
    /// Validates that the field is not empty.
    /// </summary>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult(false, "Field is required.");

        return ValidationResult.ValidResult;
    }
}

/// <summary>
/// Converts a boolean value to a button title.
/// </summary>
public class DeleteVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a button title.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the value is of type CourierInList
        if (value is BO.CourierInList courierList)
        {
            // If the courier has any delivery history or an active order, hide the delete button
            bool hasHistory = (courierList.TotalOnTimeDeliveries + courierList.TotalLateDeliveries) > 0;
            bool hasActiveOrder = courierList.CurrentOrderId is not null && courierList.CurrentOrderId != 0;

            if (hasHistory || hasActiveOrder)
                return Visibility.Collapsed;

            return Visibility.Visible;  
        }

        // Check if the value is of type Courier
        if (value is BO.Courier courier)
        {
            if (courier.Id == 0) 
                return Visibility.Collapsed;

            bool hasHistory = (courier.TotalOnTimeDeliveries + courier.TotalLateDeliveries) > 0;

            bool hasActiveOrder = courier.CurrentOrder != null;

            if (!hasHistory && !hasActiveOrder) 
                return Visibility.Visible;
        }

        // Default to collapsed if conditions are not met
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a boolean value back to a button title.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts an OrderStatus enum value to a corresponding color brush for UI representation.
/// </summary>
public class OrderStatusToBrushConverter : IValueConverter
{
    // מגדיר צבעים מותאמים אישית שמשתלבים עם העיצוב
    private readonly Brush _openColor = (Brush)new BrushConverter().ConvertFrom("#855A9D")!;
    private readonly Brush _inProgressColor = (Brush)new BrushConverter().ConvertFrom("#DBC9EF")!;
    private readonly Brush _deliveredColor = (Brush)new BrushConverter().ConvertFrom("#007B87")!;
    private readonly Brush _errorColor = (Brush)new BrushConverter().ConvertFrom("#13A2A4")!;
    private readonly Brush _cancelledColor = (Brush)new BrushConverter().ConvertFrom("#B9EBE0")!;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Open: return _openColor;
                case OrderStatus.InProgress: return _inProgressColor;
                case OrderStatus.Delivered: return _deliveredColor;
                case OrderStatus.Refused: return _errorColor;
                case OrderStatus.Cancelled: return _cancelledColor;
                default: return Brushes.Black;
            }
        }

        // צבעים ל-Timeliness (עמידה בזמנים)
        if (value is ScheduleStatus schedule)
        {
            switch (schedule)
            {
                case ScheduleStatus.Late: return _errorColor; // אותו אדום רך
                case ScheduleStatus.InRisk: return _openColor; // אותו כתום חמים
                default: return _deliveredColor; // OnTime - ירוק
            }
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts an OrderStatus enum value to a corresponding visibility for the cancel button.
/// </summary>
public class CancelVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            // אם ההזמנה נמסרה, בוטלה או סורבה - אי אפשר לבטל שוב
            if (status == OrderStatus.Delivered ||
                status == OrderStatus.Cancelled ||
                status == OrderStatus.Refused)
            {
                return Visibility.Collapsed; // הסתרת הכפתור
            }
        }
        return Visibility.Visible; // הצגת הכפתור
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class OrderToDeleteVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // בדיקה שהערך שהתקבל הוא אכן הזמנה
        if (value is BO.Order order)
        {
            // 1. אם ה-ID הוא 0, זה מצב הוספה - להסתיר
            if (order.Id == 0)
                return Visibility.Collapsed;

            // 2. בדיקת סטטוסים - להציג רק אם זה "פתוח" או "בתהליך"
            // ודאי שהשמות (Ordered, InProgress) תואמים בדיוק ל-Enum שלך ב-BO!
            if (order.StatusOfOrder == BO.OrderStatus.Open ||
                order.StatusOfOrder == BO.OrderStatus.InProgress)
            {
                return Visibility.Visible;
            }
        }

        // לכל שאר המקרים (סטטוס סגור, נשלח, או אובייקט ריק) - להסתיר
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}