using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;

namespace PL.Courier;

/// <summary>
/// Represents a user control that displays and manages a list of couriers for an administrator.
/// </summary>
/// <remarks><para> <b>CourierListWindow</b> provides functionality for viewing, filtering, adding, and deleting
/// couriers. The control supports filtering couriers by delivery type and by search text, and allows administrators to
/// manage the courier list interactively. </para> <para> The control raises the <see cref="RequestDashboard"/> event to
/// request navigation back to the dashboard. </para></remarks>
public partial class CourierListWindow : UserControl
{
    // Reference to the business logic layer for courier operations.
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // The ID of the current admin user.
    private readonly int AdminID;

    // The complete list of couriers loaded from the business logic layer.
    private IEnumerable<BO.CourierInList>? AllCouriers;

    /// <summary>
    /// Event raised to request navigation back to the dashboard.
    /// </summary>
    public event EventHandler? RequestDashboard;

    /// <summary>
    /// Gets or sets the list of couriers to display.
    /// </summary>
    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }

    /// <summary>
    /// Identifies the dependency property for the CourierList property.
    /// </summary>
    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Initializes a new instance of the <see cref="CourierListWindow"/> class.
    /// </summary>
    public CourierListWindow(int adminId)
    {
        InitializeComponent();
        AdminID = adminId;

        // Set up filter options for the category selector.
        var filterOptions = new List<object>();
        filterOptions.Add("All");
        filterOptions.AddRange(Enum.GetValues(typeof(BO.DeliveryType)).Cast<object>());

        CategorySelector.ItemsSource = filterOptions;

        LoadData();
        CategorySelector.SelectedIndex = 0;
    }

    /// <summary>
    /// Loads the courier data from the business logic layer.
    /// </summary>
    private void LoadData()
    {
        try
        {
            if (AdminID == 0) return;

            AllCouriers = s_bl.Courier.ReadAll(AdminID);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error");
        }
    }

    /// <summary>
    /// Applies filters to the courier list based on user input.
    /// </summary>
    private void ApplyFilters()
    {
        if (AllCouriers == null) 
            return;

        var tempAddList = AllCouriers;

        // Filter by selected delivery type
        if (CategorySelector.SelectedItem is BO.DeliveryType selectedType)
            tempAddList = tempAddList.Where(item => item.TypeOfDelivery == selectedType);


        // Filter by search text
        string searchText = SearchBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Case-insensitive search in courier names
            tempAddList = tempAddList.Where(item =>
                !string.IsNullOrEmpty(item.Name) &&
                item.Name.ToLower().Contains(searchText.ToLower()));
        }

        CourierList = tempAddList.ToList();
    }

    /// <summary>
    /// Handles changes to the filter controls.
    /// </summary>
    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Opens the courier window for adding a new courier.
    /// </summary>
    private void BtnAddCourier_Click(object sender, RoutedEventArgs e)
    {
        OpenCourierWindow(null);
        SearchBox.Text = "";
        CategorySelector.SelectedIndex = 0;
    }

    /// <summary>
    /// Deletes the selected courier.
    /// </summary>
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        // Confirm deletion
        if (sender is Button btn && btn.DataContext is BO.CourierInList courierToDelete)
        {
            CustomMessageBox customMsg = new CustomMessageBox($"Are you sure you want to delete {courierToDelete.Name}?", "Delete Courier", true);

            // Show confirmation dialog
            if (customMsg.ShowDialog() == true)
            {
                try
                {
                    s_bl.Courier.Delete(AdminID, courierToDelete.Id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"Failed to delete: {ex.Message}", "Error", false).ShowDialog();
                }
            }
        }
    }

    /// <summary>
    /// Opens the dashboard window.
    /// </summary>
    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        RequestDashboard?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Opens the list management window.
    /// </summary>
    private void BtnListManagement_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        CategorySelector.SelectedIndex = 0;
    }

    /// <summary>
    /// Opens the courier window for adding a new courier.
    /// </summary>
    private void OpenCourierWindow(int? id = null)
    {
        var window = new CourierWindow(id);
        window.Closed += (s, args) => LoadData();
        window.Show();
    }

    /// <summary>
    /// Handles double-click events on the courier list.
    /// </summary>
    private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is BO.CourierInList selectedCourier)
            OpenCourierWindow(selectedCourier.Id);
    }
}