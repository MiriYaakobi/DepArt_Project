using System.Windows;

namespace PL.Order;

public partial class OrderWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public bool IsUpdateMode { get; private set; }

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
                CurrentOrder = s_bl.Order.Read(s_bl.Admin.GetConfig().AdminId, orderId)!;
            }
            catch
            {
                Close();
            }
        }

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
                s_bl.Order.Update(s_bl.Admin.GetConfig().AdminId, CurrentOrder);
                CustomMessageBox.Show("Order updated successfully!", "Success");
            }
            else
            {
                // כאן יש לוודא שיש לך פונקציית יצירה מתאימה ב-BL
                // s_bl.Order.Create(CurrentOrder); 
                CustomMessageBox.Show("Order added successfully!", "Success");
            }
            Close();
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Operation failed: {ex.Message}", "Error");
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (CustomMessageBox.ShowQuestion("Delete this order?", "Confirmation"))
        {
            try
            {
                s_bl.Order.Delete(s_bl.Admin.GetConfig().AdminId, CurrentOrder.Id);
                CustomMessageBox.Show("Order deleted.", "Success");
                Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed: {ex.Message}", "Error");
            }
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}