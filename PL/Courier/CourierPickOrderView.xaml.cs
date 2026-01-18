using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier;

/// <summary>
/// pick order view for courier
/// </summary>
public partial class CourierPickOrderView : UserControl
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public event EventHandler? RequestDashboardView;
    public event EventHandler? RequestHistoryView;

    //Dependency Properties

    // courier current data
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierPickOrderView));

    //courier id
    public int CourierId
    {
        get { return (int)GetValue(CourierIdProperty); }
        set { SetValue(CourierIdProperty, value); }
    }
    public static readonly DependencyProperty CourierIdProperty =
        DependencyProperty.Register("CourierId", typeof(int), typeof(CourierPickOrderView),
            new PropertyMetadata(0, OnCourierIdChanged));

    //orders list
    public ObservableCollection<BO.OpenOrderInList> OrdersList
    {
        get { return (ObservableCollection<BO.OpenOrderInList>)GetValue(OrdersListProperty); }
        set { SetValue(OrdersListProperty, value); }
    }
    public static readonly DependencyProperty OrdersListProperty =
        DependencyProperty.Register("OrdersList", typeof(ObservableCollection<BO.OpenOrderInList>), typeof(CourierPickOrderView));

    // sort options list
    public IEnumerable<object> SortOptions { get; } = new List<object>
    {
        "All",
        BO.OpenOrderFieldSort.AirDistance,
        BO.OpenOrderFieldSort.ExpectedDeliveryTime,
        BO.OpenOrderFieldSort.TimeLinessStatus,
        BO.OpenOrderFieldSort.Id
    };

    //current selected sort option
    public object SelectedSortOption
    {
        get { return GetValue(SelectedSortOptionProperty); }
        set { SetValue(SelectedSortOptionProperty, value); }
    }

    public static readonly DependencyProperty SelectedSortOptionProperty =
        DependencyProperty.Register("SelectedSortOption", typeof(object), typeof(CourierPickOrderView),
            new PropertyMetadata("All", OnSortChanged));


    /// <summary>
    /// constructor for CourierPickOrderView.
    /// </summary>
    public CourierPickOrderView()
    {
        InitializeComponent();

        this.Loaded += UserControl_Loaded;
        this.Unloaded += UserControl_Unloaded;
    }

    //monitor order list changes
    private void OrderListObserver() => RefreshList();

    //loaded and unloaded events

    /// <summary>
    /// loaded event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.AddObserver(OrderListObserver);
        RefreshList();
    }

    /// <summary>
    /// unloaded event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.RemoveObserver(OrderListObserver);
    }

    //callbacks

    /// <summary>
    /// called when courier id changes
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnCourierIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CourierPickOrderView view)
            view.RefreshList();
    }

    /// <summary>
    /// called when sort option changes
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CourierPickOrderView view)
        {
            view.RefreshList();
        }
    }

    /// <summary>
    /// method to refresh the orders list based on current courier and sort option
    /// </summary>
    public void RefreshList()
    {
        if (CourierId == 0)
            return;

        try
        {
            if (CurrentCourier == null)
                CurrentCourier = s_bl.Courier.Read(CourierId, CourierId)!;

            //reading all open orders
            IEnumerable<BO.OpenOrderInList> list = s_bl.Order.ReadAllOpenOrders(CourierId, CourierId);

            // filtre by max distance
            double maxDist = CurrentCourier.MaxDistance ?? double.MaxValue;
            list = list.Where(o => o.AirDistance <= maxDist);

            // sorting by selected option
            if (SelectedSortOption is BO.OpenOrderFieldSort sortEnum)
            {
                list = sortEnum switch
                {
                    BO.OpenOrderFieldSort.AirDistance => list.OrderBy(o => o.AirDistance),
                    BO.OpenOrderFieldSort.ExpectedDeliveryTime => list.OrderBy(o => o.RemainingTime),
                    BO.OpenOrderFieldSort.TimeLinessStatus => list.OrderBy(o => o.TimeLinessStatus),
                    BO.OpenOrderFieldSort.Id => list.OrderBy(o => o.OrderId),
                    _ => list.OrderBy(o => o.OrderId)
                };
            }

            // default sort by order id
            else
                list = list.OrderBy(o => o.OrderId);

            // updating the observable collection
            OrdersList = new ObservableCollection<BO.OpenOrderInList>(list);
        }
        catch (Exception ex)
        {
            new CustomMessageBox($"Error: {ex.Message}", "Error", false).ShowDialog();
        }
    }

    //button click handlers

    /// <summary>
    /// pick order button click handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnPick_Click(object sender, RoutedEventArgs e)
    {
        // getting the order to pick from button data context
        if (sender is Button btn && btn.CommandParameter is int orderIdToPick)
        {
            // find the order in the list 
            var orderToPick = OrdersList.FirstOrDefault(o => o.OrderId == orderIdToPick);

            if (orderToPick == null)
                return;

            try
            {
                // calling BL to choose the order
                s_bl.Order.ChooseOrder(CourierId, CourierId, orderToPick.OrderId);
                OrdersList.Remove(orderToPick);

                new CustomMessageBox($"Order #{orderToPick.OrderId} picked successfully!\nAn email with the full details has been sent to you.", "Success", false).ShowDialog();

                //auto navigate to dashboard after picking an order
                RequestDashboardView?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Failed to pick order: {ex.Message}", "Error", false).ShowDialog();
            }
        }
    }

    /// <summary>
    /// click handler to navigate to dashboard view
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        RequestDashboardView?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// click handler to navigate to history view
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
        RequestHistoryView?.Invoke(this, EventArgs.Empty);
    }
}