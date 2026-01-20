using PL.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderListWindow.xaml
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
public partial class OrderListWindow : UserControl
{
    // bl instance
    private BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly ObserverMutex _orderListMutex = new();

    // admin ID
    private int AdminID;

    // events for navigation
    public event EventHandler? RequestDashboard;
    public event EventHandler? RequestCouriers;

    // Dependency Properties

    //order list property to bind to the DataGrid
    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow));

    // search text property for filtering
    public string SearchText
    {
        get { return (string)GetValue(SearchTextProperty); }
        set { SetValue(SearchTextProperty, value); }
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register("SearchText", typeof(string), typeof(OrderListWindow), new PropertyMetadata(string.Empty)); // ערך התחלתי ריק

    // selected order property for tracking the selected order in the DataGrid
    public object? SelectedOrder
    {
        get { return (object?)GetValue(SelectedOrderProperty); }
        set { SetValue(SelectedOrderProperty, value); }
    }

    public static readonly DependencyProperty SelectedOrderProperty =
        DependencyProperty.Register("SelectedOrder", typeof(object), typeof(OrderListWindow));

    // status filter property for filtering orders by status
    public object StatusFilter
    {
        get { return (object)GetValue(StatusFilterProperty); }
        set { SetValue(StatusFilterProperty, value); }
    }

    public static readonly DependencyProperty StatusFilterProperty =
        DependencyProperty.Register("StatusFilter", typeof(object), typeof(OrderListWindow), new PropertyMetadata("All"));

    // all orders fetched from BL
    private IEnumerable<BO.OrderInList>? AllOrders;

    // constructor
    public OrderListWindow(int adminId = 1)
    {
        InitializeComponent();
        AdminID = adminId;
        StatusFilter = "All";
    }

    // event handlers
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        LoadData();
        s_bl.Order.AddObserver(OrderListObserver);
        s_bl.Courier.AddObserver(OrderListObserver);
    }

    // un-register observer on unload to prevent memory leaks
    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.RemoveObserver(OrderListObserver);
        s_bl.Courier.RemoveObserver(OrderListObserver);
    }

    /// <summary>
    /// observer method for order list changes.
    /// </summary>
    private void OrderListObserver()
    {
        // entry checks and mutex handling
        if (_orderListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        // update on UI thread
        Dispatcher.BeginInvoke(async () =>
        { 
            LoadData();

            // exit critical section and check for restart request
            if (await _orderListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderListObserver();
        });
    }

    // load data from BL
    private void LoadData()
    {
        try
        {
            AllOrders = s_bl.Order.ReadAll(AdminID);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}");
        }
    }

    // apply filters to the order list
    private void ApplyFilters()
    {
        if (AllOrders == null) 
            return;

        var tempAddList = AllOrders;

        // filtering by status
        if (StatusFilter is BO.OrderStatus selectedStatus)
            tempAddList = tempAddList.Where(item => item.StatusOfOrder == selectedStatus);

        // filtering by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            tempAddList = tempAddList.Where(item =>
                (item.Id?.ToString().Contains(SearchText) ?? false) ||
                (item.OrderId.ToString().Contains(SearchText))
            );
        }

        OrderList = tempAddList.ToList();
    }

    // event handler for filter changes
    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    // event handler for search text changes
    private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
    {
        new OrderWindow().Show();
    }

    // open order window for viewing/editing
    private void OpenOrderWindow(int id = 0)
    {
        var window = new OrderWindow(id);
        window.Show();
    }

    // double-click event to open selected order
    private void OrderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedOrder is BO.OrderInList selectedOrder)
        {
            OpenOrderWindow(selectedOrder.OrderId);
            SelectedOrder = null;
        }
    }

    // cancel order button click event
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        //safty check to ensure sender is a button and its DataContext is an OrderInList
        if (sender is FrameworkElement element && element.DataContext is BO.OrderInList orderToCancel)
        {
            //if the order is already delivered, cancelled, or refused, we cannot cancel it
            if (orderToCancel.StatusOfOrder == BO.OrderStatus.Delivered ||
                orderToCancel.StatusOfOrder == BO.OrderStatus.Cancelled ||
                orderToCancel.StatusOfOrder == BO.OrderStatus.Refused)
            {
                CustomMessageBox.Show("Cannot cancel a closed order.", "Error");
                return;
            }

            int idToCancel = orderToCancel.OrderId;

            if (CustomMessageBox.ShowQuestion($"Are you sure you want to CANCEL Order #{idToCancel}?", "Confirm Cancellation"))
            {
                try // attempt to cancel the order via BL
                {
                    s_bl.Order.Cancel(AdminID, idToCancel);
                    CustomMessageBox.Show("Order cancelled successfully (Courier notified via email).", "Success");
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"System Error: {ex.Message}", "Error");
                }
            }
        }
    }

    // route navigation event handlers
    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        RequestDashboard?.Invoke(this, EventArgs.Empty);
    }

    // couriers list navigation
    private void BtnCouriers_Click(object sender, RoutedEventArgs e)
    {
        RequestCouriers?.Invoke(this, EventArgs.Empty);
    }

    // orders list navigation (no action needed)
    private void BtnList_Click(object sender, RoutedEventArgs e)
    {
        // Do nothing, we are already here
    }
}