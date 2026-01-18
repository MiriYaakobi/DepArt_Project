using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PL; // חובה עבור CustomMessageBox

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
    /// Event raised to request navigation back to the dashboard and order list.
    /// </summary>
    public event EventHandler? RequestDashboard;
    public event EventHandler? RequestOrderList;

    /// <summary>
    /// Gets or sets the selected status filter for the courier list.
    /// </summary>
    public object StatusFilter { get; set; } = "All";

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

        StatusFilter = "All";

        // הוספתי את ה-Loading וה-Unloading לפה כדי לוודא שזה רשום
        this.Loaded += UserControl_Loaded;
        this.Unloaded += UserControl_Unloaded;

        LoadData();
    }

    /// <summary>
    /// Loads the courier data from the business logic layer.
    /// </summary>
    private void LoadData()
    {
        try
        {
            if (AdminID == 0) return;

            // טעינה מחדש מה-BL
            AllCouriers = s_bl.Courier.ReadAll(AdminID);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            // שימוש ב-CustomMessageBox במקום MessageBox רגיל
            new CustomMessageBox($"Error loading data: {ex.Message}", "Error", false).ShowDialog();
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

        // Filter by delivery type
        if (StatusFilter is BO.DeliveryType selectedType)
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
        StatusFilter = "All";
    }

    /// <summary>
    /// Deletes the selected courier.
    /// </summary>
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        // Confirm deletion
        if (sender is Button btn && btn.DataContext is BO.CourierInList courierToDelete)
        {
            // שימוש ב-CustomMessageBox לשאלה
            if (CustomMessageBox.ShowQuestion($"Are you sure you want to delete {courierToDelete.Name}?", "Delete Courier"))
            {
                try
                {
                    s_bl.Courier.Delete(AdminID, courierToDelete.Id);
                    // לא צריך לקרוא ל-LoadData ידנית כי ה-Observer יעשה את זה!
                    // אבל אם רוצים תגובה מיידית:
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
    private void BtnList_Click(object sender, RoutedEventArgs e)
    {
        RequestOrderList?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Opens the courier window for adding a new courier.
    /// </summary>
    private void OpenCourierWindow(int? id = null)
    {
        var window = new CourierWindow(id);
        // window.Closed += (s, args) => LoadData(); // אין צורך, ה-Observer יטפל בעדכון!
        window.Show();
    }

    /// <summary>
    /// Handles double-click events on the courier list.
    /// </summary>
    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is BO.CourierInList selectedCourier)
            OpenCourierWindow(selectedCourier.Id);
    }

    /// <summary>
    /// Observer method for courier list changes.
    /// </summary>
    private void CourierListObserver()
    {
        Dispatcher.Invoke(() =>
        {
            // תיקון חשוב: טעינה מחדש מה-DB ולא רק סינון
            LoadData();
        });
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Courier.AddObserver(CourierListObserver);
        LoadData(); // טעינה ראשונית כשהמסך עולה
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        s_bl.Courier.RemoveObserver(CourierListObserver);
    }

    private void BtnCouriers_Click(object sender, RoutedEventArgs e)
    {
        // Do nothing, we are already here
    }
}