using PL.Helpers;
using System;
using System.Windows;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderWindow.xaml
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
public partial class OrderWindow : Window
{
    // bl instance
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly ObserverMutex _orderUpdateMutex = new();

    private int currentAdminId;

    // global enum array for order types
    public Array OrderTypes { get; } = Enum.GetValues(typeof(BO.OrderType));

    // Dependency Properties

    // used to determine if we are in update mode or add mode
    public bool IsUpdateMode
    {
        get { return (bool)GetValue(IsUpdateModeProperty); }
        set { SetValue(IsUpdateModeProperty, value); }
    }

    public static readonly DependencyProperty IsUpdateModeProperty =
        DependencyProperty.Register("IsUpdateMode", typeof(bool), typeof(OrderWindow), new PropertyMetadata(false));


    // indicates if the fields are editable
    public bool IsEditable
    {
        get { return (bool)GetValue(IsEditableProperty); }
        set { SetValue(IsEditableProperty, value); }
    }

    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register("IsEditable", typeof(bool), typeof(OrderWindow), new PropertyMetadata(true));


    // the current order being added/updated
    public BO.Order CurrentOrder
    {
        get { return (BO.Order)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }

    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow),
            new PropertyMetadata(null, OnCurrentOrderChanged));

    // Callback when CurrentOrder changes to recalculate IsEditable
    private static void OnCurrentOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OrderWindow window)
        {
            window.RecalculateIsEditable();
        }
    }

    // Helper function to determine if the screen is editable
    private void RecalculateIsEditable()
    {
        if (!IsUpdateMode)
        {
            // In add mode - always editable
            IsEditable = true;
        }
        else
        {
            // In update mode - editable only if the order is open and exists
            IsEditable = CurrentOrder != null && CurrentOrder.StatusOfOrder == BO.OrderStatus.Open;
        }
    }

    // Constructor
    public OrderWindow(int orderId = 0)
    {
        InitializeComponent();

        try // get current admin ID
        {
            currentAdminId = s_bl.Admin.GetConfig().AdminId;
        }
        catch
        {
            currentAdminId = 123456782;
        }

        // check if we are adding a new order or updating an existing one
        if (orderId == 0)
        {
            IsUpdateMode = false;

            // Creating a new order
            CurrentOrder = new BO.Order
            {
                OrderOpeningTime = s_bl.Admin.GetClock(),
                StatusOfOrder = BO.OrderStatus.Open,
            };
        }
        else
        {
            IsUpdateMode = true;
            try
            {
                // Loading existing order
                CurrentOrder = s_bl.Order.Read(currentAdminId, orderId)!;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Could not load order #{orderId}.\nError: {ex.Message}", "Error");
                this.Close();
                return;
            }
        }

        // Register observer for order updates
        s_bl.Order.AddObserver(OrderUpdateObserver);


        //unregister observer on window closing
        this.Closing += (s, e) =>
        {
            s_bl.Order.RemoveObserver(OrderUpdateObserver);
        };

        // Initial calculation of edit mode
        RecalculateIsEditable();

        DataContext = this;
    }

    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // basic validation
            if (CurrentOrder == null ||
                string.IsNullOrWhiteSpace(CurrentOrder.CustomerName) ||
                string.IsNullOrWhiteSpace(CurrentOrder.Address) ||
                string.IsNullOrWhiteSpace(CurrentOrder.CustomerPhone))
            {
                CustomMessageBox.Show("Please fill required fields (Name, Phone, Address).", "Validation Error");
                return;
            }

            // perform add or update
            if (IsUpdateMode)
            {
                s_bl.Order.Update(currentAdminId, CurrentOrder);
                CustomMessageBox.Show("Order updated successfully!", "Success");
            }
            else // add mode
            {
                s_bl.Order.Create(currentAdminId, CurrentOrder);
                CustomMessageBox.Show("Order added successfully!", "Success");
            }

            this.Close();
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Operation failed: {ex.Message}", "Error");
        }
    }

    // Cancel order button click handler
    private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentOrder == null) 
            return;

        // only open or in-progress orders can be canceled
        if (CurrentOrder.StatusOfOrder != BO.OrderStatus.Open && CurrentOrder.StatusOfOrder != BO.OrderStatus.InProgress)
        {
            CustomMessageBox.Show("Cannot cancel closed order", "Validation Error");
            return;
        }

        // confirm cancellation
        if (CustomMessageBox.ShowQuestion("Cancel this order?", "Confirmation"))
        {
            try // attempt to cancel the order
            {
                s_bl.Order.Cancel(currentAdminId, CurrentOrder.Id);
                CustomMessageBox.Show("Order canceled successfully.", "Success");
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to cancel: {ex.Message}", "Error");
            }
        }
    }

    // Cancel button click handler
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// observer method for order updates.
    /// </summary>
    private void OrderUpdateObserver()
    {
        // relevant only in update mode with a current order
        if (!IsUpdateMode || CurrentOrder == null)
            return;

        //  entry checks and mutex handling
        if (_orderUpdateMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        // update on UI thread
        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                var updatedOrder = s_bl.Order.Read(currentAdminId, CurrentOrder.Id);

                if (updatedOrder != null)
                {
                    CurrentOrder = updatedOrder;
                }
            }
            catch
            {
                this.Close();
            }

            // exit checks and mutex handling
            if (await _orderUpdateMutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderUpdateObserver();
        });
    }
}