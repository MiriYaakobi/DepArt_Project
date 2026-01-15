using System.Windows;

namespace PL.Order;

public partial class OrderWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public bool IsUpdateMode { get; private set; }

    private int currentAdminId;

    // Dependency Properties
    public BO.Order CurrentOrder
    {
        get { return (BO.Order)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }

    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

    public Array OrderTypes { get; } = Enum.GetValues(typeof(BO.OrderType));

    public OrderWindow(int orderId = 0)
    {
        InitializeComponent();

        try
        {
            currentAdminId = s_bl.Admin.GetConfig().AdminId;
        }
        catch
        {
            currentAdminId = 123456782;
        }

        if (orderId == 0) // Add mode
        {
            IsUpdateMode = false;
            CurrentOrder = new BO.Order
            {
                OrderOpeningTime = s_bl.Admin.GetClock(),
                StatusOfOrder = BO.OrderStatus.Open,
            };
        }
        else // Update mode
        {
            IsUpdateMode = true;
            try
            {
                CurrentOrder = s_bl.Order.Read(currentAdminId, orderId)!;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Could not load order #{orderId}.\nError: {ex.Message}", "Error");
                CurrentOrder = new BO.Order();
                IsUpdateMode = false;
            }
        }
        CurrentOrder = s_bl.Order.Read(currentAdminId, orderId)!;
        DataContext = this;
    }

    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // ולידציה
            if (string.IsNullOrWhiteSpace(CurrentOrder.CustomerName) ||
                string.IsNullOrWhiteSpace(CurrentOrder.Address) ||
                string.IsNullOrWhiteSpace(CurrentOrder.CustomerPhone))
            {
                CustomMessageBox.Show("Please fill required fields (Name, Phone, Address).", "Validation Error");
                return;
            }

            if (IsUpdateMode)
            {
                s_bl.Order.Update(currentAdminId, CurrentOrder);
                CustomMessageBox.Show("Order updated successfully!", "Success");
            }
            else
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

    /// <summary>
    /// returns true if the order fields are editable based on the mode and order status.
    /// </summary>
    public bool IsEditable
    {
        get
        {
            //if it's add mode - always editable
            if (!IsUpdateMode)
                return true;

            //can edit only if order is open
            return CurrentOrder != null && CurrentOrder.StatusOfOrder == BO.OrderStatus.Open;
        }
    }

    private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentOrder.StatusOfOrder != BO.OrderStatus.Open && CurrentOrder.StatusOfOrder != BO.OrderStatus.InProgress)
        {
            CustomMessageBox.Show("Cannot cancel close order", "Validation Error");
            return;
        }

        if (CustomMessageBox.ShowQuestion("Cancel this order?", "Confirmation"))
        {
            try
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

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}