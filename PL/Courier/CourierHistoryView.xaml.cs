using PL.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier;

/// <summary>
/// history view for a courier, displaying closed deliveries and allowing filtering by status.
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
public partial class CourierHistoryView : UserControl
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly ObserverMutex _listMutex = new();

    // Dependency Properties

    //courier current data
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierHistoryView));

    //courier id
    public int CourierId
    {
        get { return (int)GetValue(CourierIdProperty); }
        set { SetValue(CourierIdProperty, value); }
    }

    public static readonly DependencyProperty CourierIdProperty =
        DependencyProperty.Register("CourierId", typeof(int), typeof(CourierHistoryView), new PropertyMetadata(0, OnCourierIdChanged));

    // Observable Collection for Deliveries
    public ObservableCollection<BO.ClosedDeliveryInList> DeliveriesList { get; set; } = new();

    // Status filter options
    public IEnumerable<object> StatusOptions { get; } =
        new List<object> { "All" }
        .Concat(Enum.GetValues(typeof(BO.OrderEndStatus)).Cast<object>())
        .ToList();

    // Current selected status filter
    public object SelectedStatusFilter
    {
        get { return GetValue(SelectedStatusFilterProperty); }
        set { SetValue(SelectedStatusFilterProperty, value); }
    }

    public static readonly DependencyProperty SelectedStatusFilterProperty =
        DependencyProperty.Register("SelectedStatusFilter", typeof(object), typeof(CourierHistoryView), new PropertyMetadata("All"));

    // Events to request navigation
    public event EventHandler? RequestDashboardView;
    public event EventHandler? RequestPickOrderView;

    /// <summary>
    /// constructor for CourierHistoryView.
    /// </summary>
    public CourierHistoryView()
    {
        InitializeComponent();
        SelectedStatusFilter = "All";

        //wiring up Loaded and Unloaded events
        this.Loaded += UserControl_Loaded;
        this.Unloaded += UserControl_Unloaded;
    }

    /// <summary>
    /// observer method for order changes.
    /// </summary>
    private void OrderObserver()
    {
        // entry checks and mutex handling
        if (_listMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        // update on UI thread
        Dispatcher.BeginInvoke(async () =>
        {
            RefreshList();

            // exit mutex and check if a restart is needed
            if (await _listMutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderObserver();
        });
    }

    /// <summary>
    /// called when the UserControl is loaded; registers the order observer and refreshes the list.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.AddObserver(OrderObserver);
        RefreshList();
    }

    /// <summary>
    /// called when the UserControl is unloaded; removes the order observer.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.RemoveObserver(OrderObserver);
    }

    /// <summary>
    /// callback for when the CourierId property changes; refreshes the list.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnCourierIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CourierHistoryView view)
        {
            view.RefreshList();
        }
    }

    /// <summary>
    /// refreshes the list of closed deliveries for the courier,
    /// applying any selected status filter.
    /// </summary>
    public void RefreshList()
    {
        if (CourierId == 0)
            return;

        try
        {
            // Fetch closed deliveries for the courier
            IEnumerable<BO.ClosedDeliveryInList> list = s_bl.Order.GetClosedDeliveriesForCourier(CourierId, CourierId);

            // Apply status filter if selected
            if (SelectedStatusFilter is BO.OrderEndStatus statusEnum)
                list = list.Where(d => d.OrderClosedStatus == statusEnum);

            DeliveriesList.Clear();

            // Populate the observable collection
            foreach (var item in list)
                DeliveriesList.Add(item);
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Error fetching deliveries: {ex.Message}", "System Error", MessageType.Error);
        }
    }

    /// <summary>
    /// selection changed event handler for the status filter; refreshes the list.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshList();
    }

    /// <summary>
    /// dashboard button click event handler; requests navigation to the dashboard view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        RequestDashboardView?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// pick order button click event handler; requests navigation to the pick order view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
    {
        RequestPickOrderView?.Invoke(this, EventArgs.Empty);
    }
}