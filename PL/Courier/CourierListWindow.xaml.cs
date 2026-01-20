using PL.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Courier;

/// <summary>
/// courier list window logic.
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
public partial class CourierListWindow : UserControl
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly ObserverMutex _listMutex = new();

    private readonly int AdminID;
    private IEnumerable<BO.CourierInList>? AllCouriers;

    // Event raised to request navigation to the dashboard.
    public event EventHandler? RequestDashboard;
    public event EventHandler? RequestOrderList;

    //Dependency properties

    //courier list
    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }

    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    //search text
    public string SearchText
    {
        get { return (string)GetValue(SearchTextProperty); }
        set { SetValue(SearchTextProperty, value); }
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register("SearchText", typeof(string), typeof(CourierListWindow),
        new PropertyMetadata("", (d, e) => ((CourierListWindow)d).ApplyFilters()));

    //status filter
    public object StatusFilter
    {
        get { return GetValue(StatusFilterProperty); }
        set { SetValue(StatusFilterProperty, value); }
    }
    public static readonly DependencyProperty StatusFilterProperty =
        DependencyProperty.Register("StatusFilter", typeof(object), typeof(CourierListWindow),
        new PropertyMetadata("All", (d, e) => ((CourierListWindow)d).ApplyFilters()));

    //selected courier
    public BO.CourierInList SelectedCourier
    {
        get { return (BO.CourierInList)GetValue(SelectedCourierProperty); }
        set { SetValue(SelectedCourierProperty, value); }
    }
    public static readonly DependencyProperty SelectedCourierProperty =
        DependencyProperty.Register("SelectedCourier", typeof(BO.CourierInList), typeof(CourierListWindow), new PropertyMetadata(null));

    /// <summary>
    /// constructor for CourierListWindow.
    /// </summary>
    /// <param name="adminId"></param>
    public CourierListWindow(int adminId)
    {
        InitializeComponent();
        AdminID = adminId;

        StatusFilter = "All";

        // Wiring up Loaded and Unloaded events
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

            //load all couriers
            AllCouriers = s_bl.Courier.ReadAll(AdminID);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            new CustomMessageBox($"Error loading data: {ex.Message}", "Error", false).ShowDialog();
        }
    }

    /// <summary>
    /// applies the selected filters to the courier list.
    /// </summary>
    private void ApplyFilters()
    {
        if (AllCouriers == null) 
            return;

        var tempAddList = AllCouriers;

        // Filter by delivery type
        if (StatusFilter is BO.DeliveryType selectedType)
            tempAddList = tempAddList.Where(item => item.TypeOfDelivery == selectedType);

        string searchText = SearchText;

        // Filter by search text
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
    /// adds a new courier by opening the courier window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnAddCourier_Click(object sender, RoutedEventArgs e)
    {
        OpenCourierWindow(null);
        SearchText = "";
        StatusFilter = "All";
    }

    /// <summary>
    /// deletes the selected courier after confirmation.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        // Get the courier ID from the button's CommandParameter
        if (sender is Button btn && btn.CommandParameter is int courierIdToDelete)
        {
            var courierToDelete = CourierList.FirstOrDefault(c => c.Id == courierIdToDelete);
            string courierName = courierToDelete?.Name ?? "Selected Courier";

            // Confirm deletion
            if (CustomMessageBox.ShowQuestion($"Are you sure you want to delete {courierName}?", "Delete Courier"))
            {
                try
                {
                    s_bl.Courier.Delete(AdminID, courierIdToDelete);
                    new CustomMessageBox($"{courierName} deleted successfully!", "Deleted", false).ShowDialog();
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"Failed to delete: {ex.Message}", "Error", false).ShowDialog();
                }
            }
        }
    }

    /// <summary>
    /// opens the dashboard view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        RequestDashboard?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// opens the order list view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnList_Click(object sender, RoutedEventArgs e)
    {
        RequestOrderList?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// opens the courier window for adding or updating a courier.
    /// </summary>
    /// <param name="id"></param>
    private void OpenCourierWindow(int? id = null)
    {
        var window = new CourierWindow(id);
        window.Show();
    }

    /// <summary>
    /// opens the courier window on double click.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedCourier != null)
            OpenCourierWindow(SelectedCourier.Id);
    }

    /// <summary>
    /// observer method for courier list changes.
    /// </summary>
    private void CourierListObserver()
    {
        // entry checks and mutex handling
        if (_listMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        // update on UI thread
        Dispatcher.BeginInvoke(async () =>
        {
            LoadData();

            // exit critical section and check for restart request
            if (await _listMutex.UnsetLoadInProgressAndCheckRestartRequested())
                CourierListObserver();
        });
    }

    /// <summary>
    /// opens the courier list and subscribes to updates.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Courier.AddObserver(CourierListObserver);
        s_bl.Order.AddObserver(CourierListObserver);
        LoadData();
    }

    /// <summary>
    /// on unload, unsubscribes from courier updates.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        s_bl.Courier.RemoveObserver(CourierListObserver);
        s_bl.Order.RemoveObserver(CourierListObserver);
    }

    /// <summary>
    /// does nothing, we are already in the courier list. but needed for button consistency.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCouriers_Click(object sender, RoutedEventArgs e)
    {
        // Do nothing, we are already here
    }
}