using System.Windows;

namespace PL.Courier;

/// <summary>
/// courier add/update window
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
public partial class CourierWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public bool IsUpdateMode { get; private set; }
    private int currentAdminId;

    public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryType));

    //Dependency Property

    //current courier
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    /// <summary>
    /// constructor for CourierWindow
    /// </summary>
    /// <param name="courierId"></param>
    public CourierWindow(int? courierId = null)
    {
        InitializeComponent();
        try
        {
            currentAdminId = s_bl.Admin.GetConfig().AdminId;
        }
        catch
        {
            // Default admin ID if config retrieval fails
            currentAdminId = 123456782;
        }

        // Determine mode based on presence of courierId
        if (courierId == null)
        {
            IsUpdateMode = false;
            CurrentCourier = new BO.Courier();
        }

        // Update mode
        else
        {
            IsUpdateMode = true;

            // Load the existing courier details
            try
            {
                CurrentCourier = s_bl.Courier.Read(currentAdminId, courierId.Value)!;

                //save the password to not be null
                CurrentCourier.Password = "********";
            }
            catch
            {
                Close();
            }
        }
    }

    /// <summary>
    /// adds or updates a courier based on the current mode.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Basic validation
            if (string.IsNullOrEmpty(CurrentCourier.Name) || string.IsNullOrEmpty(CurrentCourier.Phone))
            {
                CustomMessageBox.Show("Please fill in all required fields.", "Validation Error");
                return;
            }

            // Update existing courier
            if (IsUpdateMode)
            {
                s_bl.Courier.Update(currentAdminId, CurrentCourier);
                CustomMessageBox.Show("Courier updated successfully!", "Success");
            }

            //Add new courier
            else
            {
                // Ensure password is provided for new courier
                if (string.IsNullOrEmpty(CurrentCourier.Password))
                {
                    CustomMessageBox.Show("Password is required for new courier.", "Validation Error");
                    return;
                }

                s_bl.Courier.Create(currentAdminId, CurrentCourier);
                CustomMessageBox.Show("Courier added successfully!", "Success");
            }
            this.Close();
        }
        catch (Exception ex)
        {
            // Hide password in case of error during update
            if (IsUpdateMode) 
                CurrentCourier.Password = "********";

            CustomMessageBox.Show($"Operation failed: {ex.Message}", "Error");
        }
    }

    /// <summary>
    /// deletes the current courier after confirmation.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        // Prevent deletion if the courier has an active order
        if (CurrentCourier.CurrentOrder != null)
        {
            CustomMessageBox.Show("Cannot delete courier while they have an active order.", "Validation Error");
            return;
        }

        string msg = $"Are you sure you want to delete {CurrentCourier.Name}?";

        // Confirm deletion
        if (CustomMessageBox.ShowQuestion(msg, "Delete Confirmation"))
        {
            try
            {
                s_bl.Courier.Delete(currentAdminId, CurrentCourier.Id);
                CustomMessageBox.Show($"{CurrentCourier.Name} was deleted successfully.", "Deleted");
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to delete: {ex.Message}", "Error");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnViewOrder_Click(object sender, RoutedEventArgs e)
    {
        // Ensure there is a current order to view
        if (CurrentCourier.CurrentOrder == null) 
            return;

        // Create and display the order details window
        Window orderWindow = new Window
        {
            Title = "Order Details",
            Width = 400,
            Height = 550,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            Background = (System.Windows.Media.Brush)FindResource("DeepPurple"),
            Content = CurrentCourier.CurrentOrder,
            ContentTemplate = (DataTemplate)FindResource("OrderDetailsTemplate")
        };

        // Close the window on Escape key press
        orderWindow.PreviewKeyDown += (s, args) => { if (args.Key == System.Windows.Input.Key.Escape) orderWindow.Close(); };

        orderWindow.ShowDialog(); 
    }

    /// <summary>
    /// cancel button click handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}