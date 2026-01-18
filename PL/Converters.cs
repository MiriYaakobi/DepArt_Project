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
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
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
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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
    // colors definition
    private readonly Brush _openColor = (Brush)new BrushConverter().ConvertFrom("#DB7093")!;
    private readonly Brush _inProgressColor = (Brush)new BrushConverter().ConvertFrom("#FFA07A")!;
    private readonly Brush _deliveredColor = (Brush)new BrushConverter().ConvertFrom("#3CB371")!;
    private readonly Brush _errorColor = (Brush)new BrushConverter().ConvertFrom("#ff5555")!;
    private readonly Brush _cancelledColor = (Brush)new BrushConverter().ConvertFrom("#008B8B")!;

    /// <summary>
    /// Converts an OrderStatus enum value to a corresponding color brush.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // status colors
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

        // schedule colors
        if (value is ScheduleStatus schedule)
        {
            switch (schedule)
            {
                case ScheduleStatus.Late: return _errorColor;
                case ScheduleStatus.InRisk: return _openColor; 
                default: return _deliveredColor; 
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
            // checking for statuses that should hide the cancel button
            if (status == OrderStatus.Delivered ||
                status == OrderStatus.Cancelled ||
                status == OrderStatus.Refused)
            {
                return Visibility.Collapsed; // hiding the button
            }
        }
        return Visibility.Visible; // showing the button
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts an Order object to a corresponding visibility for the delete button.
/// </summary>
public class OrderToDeleteVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // checking that the received value is indeed an order
        if (value is BO.Order order)
        {
            // if the order is new (id = 0) - hide the delete button
            if (order.Id == 0)
                return Visibility.Collapsed;

            // checking statuses - show only if it's "Open" or "InProgress"
            // make sure the names (Ordered, InProgress) match exactly with your Enum in BO!
            if (order.StatusOfOrder == BO.OrderStatus.Open ||
                order.StatusOfOrder == BO.OrderStatus.InProgress)
            {
                return Visibility.Visible;
            }
        }

        // the default is to hide the delete button
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

}

/// <summary>
/// Converts an OrderEndStatus enum value to a corresponding brush color.
/// </summary>
public class OrderEndStatusToBrushConverter : IValueConverter
{
    // defining colors
    private readonly Brush _deliveredColor = (Brush)new BrushConverter().ConvertFrom("#DFF0D8")!; // light green (success)
    private readonly Brush _refusedColor = (Brush)new BrushConverter().ConvertFrom("#F2DEDE")!;   // light red (refused/failed)
    private readonly Brush _notFoundColor = (Brush)new BrushConverter().ConvertFrom("#FFF3CD")!;  // light yellow (not found)
    private readonly Brush _cancelledColor = (Brush)new BrushConverter().ConvertFrom("#E2E3E5")!; // gray (cancelled)

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // checking if the value is of type OrderEndStatus
        if (value is OrderEndStatus endStatus)
        {
            switch (endStatus)
            {
                case OrderEndStatus.Delivered:
                    return _deliveredColor;

                case OrderEndStatus.Refused:
                case OrderEndStatus.Failed:
                    return _refusedColor;

                case OrderEndStatus.InviterNotFound:
                    return _notFoundColor;

                case OrderEndStatus.Cancelled:
                    return _cancelledColor;

                default:
                    return Brushes.White;
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)=> throw new NotImplementedException();
}