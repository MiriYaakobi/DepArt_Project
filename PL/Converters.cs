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
        if (value is bool isActive)
        {
            if (!isActive)
                return Brushes.LavenderBlush;
        }

        return Brushes.Transparent;
    }

    /// <summary>
    /// Converts a color brush back to a boolean value indicating status.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
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
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Converts a null value back to a visibility state.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}