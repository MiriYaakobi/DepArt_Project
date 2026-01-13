using System.Windows;

namespace PL.Courier;

/// <summary>
/// Represents a window for adding, updating, viewing, or deleting courier information within the application.
/// </summary>
/// <remarks><para> The <see cref="CourierWindow"/> provides a user interface for managing courier records,
/// supporting both creation of new couriers and modification or deletion of existing ones. The window adapts its
/// behavior based on whether a courier ID is provided at construction time: </para> <list type="bullet">   <item>    
/// <description>If <c>courierId</c> is <see langword="null"/>, the window operates in add mode, allowing entry of a new
/// courier's details.</description>   </item>   <item>     <description>If <c>courierId</c> is specified, the window
/// loads the corresponding courier for editing or deletion.</description>   </item> </list> <para> The window exposes
/// properties for data binding, including the current courier being edited and available delivery types. It also
/// provides feedback to the user for validation errors and operation results. </para></remarks>
public partial class CourierWindow : Window
{
    // Reference to the business logic layer for courier operations.
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // Indicates whether the window is in update mode (true) or add mode (false).
    public bool IsUpdateMode { get; private set; }

    // Array of available delivery types for selection in the UI.
    public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryType));

    /// The courier currently being added or edited.
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    /// <summary>
    /// Identifies the dependency property for the CurrentCourier property.
    /// </summary>
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    /// <summary>
    /// Initializes a new instance of the <see cref="CourierWindow"/> class.
    /// </summary>
    public CourierWindow(int? courierId = null)
    {
        InitializeComponent();

        // Determine mode based on presence of courierId
        if (courierId == null)
        {
            IsUpdateMode = false;
            CurrentCourier = new BO.Courier();
        }

        else //update mode
        {
            IsUpdateMode = true;

            // Load the existing courier details
            try
            {
                CurrentCourier = s_bl.Courier.Read(s_bl.Admin.GetConfig().AdminId, courierId.Value)!;

                //save the password to not be null
                CurrentCourier.Password = "********";
            }
            catch
            {
                Close();
            }
        }

        DataContext = this;
    }

    /// <summary>
    /// Handles the click event for the Add/Update button.
    /// </summary>
    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        // Validate required fields
        try
        {
            // Basic validation
            if (string.IsNullOrEmpty(CurrentCourier.Name) || string.IsNullOrEmpty(CurrentCourier.Phone))
            {
                CustomMessageBox.Show("Please fill in all required fields.", "Validation Error");
                return;
            }

            // Additional validation can be added here as needed
            if (IsUpdateMode)
            {
                // If password is not changed, retain the original password
                if (string.IsNullOrEmpty(CurrentCourier.Password))
                {
                    BO.Courier originalCourierFromDb = s_bl.Courier.Read(s_bl.Admin.GetConfig().AdminId, CurrentCourier.Id)!;

                    CurrentCourier.Password = originalCourierFromDb!.Password;
                }

                // Update existing courier
                s_bl.Courier.Update(s_bl.Admin.GetConfig().AdminId, CurrentCourier);
                CustomMessageBox.Show("Courier updated successfully!", "Success");
            }
            else // Add new courier
            {
                // Ensure password is provided for new courier
                if (string.IsNullOrEmpty(CurrentCourier.Password))
                {
                    CustomMessageBox.Show("Password is required for new courier.", "Validation Error");
                    return;
                }

                // Create new courier
                s_bl.Courier.Create(s_bl.Admin.GetConfig().AdminId, CurrentCourier);
                CustomMessageBox.Show("Courier added successfully!", "Success");
            }
            this.Close();
        }
        catch (Exception ex) // Catch any exceptions from BL layer
        {
            if (IsUpdateMode) 
                CurrentCourier.Password = "********";
            CustomMessageBox.Show($"Operation failed: {ex.Message}", "Error");
        }
    }

    /// <summary>
    /// Handles the click event for the Delete button.
    /// </summary>
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        // Prevent deletion if the courier has an active order
        if (CurrentCourier.CurrentOrder != null)
        {
            CustomMessageBox.Show("Cannot delete courier while they have an active order.", "Validation Error");
            return;
        }

        // Confirm deletion
        if (CustomMessageBox.ShowQuestion("Are you sure you want to delete this courier?", "Delete Confirmation"))
        {
            // Proceed with deletion
            try
            {
                s_bl.Courier.Delete(s_bl.Admin.GetConfig().AdminId, CurrentCourier.Id);
                CustomMessageBox.Show("Courier deleted successfully.", "Deleted");
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to delete: {ex.Message}", "Error");
            }
        }
    }

    /// <summary>
    /// Handles the click event for the View Order button.
    /// </summary>
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
    /// Handles the click event for the Cancel button.
    /// </summary>
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}