using PL.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// Represents the main window of the application, providing the primary user interface for managing administrative
/// tasks, including configuration, database operations, and data visualization.
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly ObserverMutex _observerMutex = new();

    // simulator properties

    // simulator interval property
    private int _simulatorInterval = 1;
    public int SimulatorInterval
    {
        get => _simulatorInterval;
        set { _simulatorInterval = value; OnPropertyChanged(); }
    }

    // simulator running state
    private bool _isSimulatorRunning = false;
    public bool IsSimulatorRunning
    {
        get => _isSimulatorRunning;
        set
        {
            _isSimulatorRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulatorButtonContent)); //update button text
            OnPropertyChanged(nameof(IsSimulatorNotRunning));  //update button enabled state
        }
    }

    //helper property to bind button enabled state
    public bool IsSimulatorNotRunning => !IsSimulatorRunning;

    // text for simulator button
    public string SimulatorButtonContent => IsSimulatorRunning ? "Stop" : "Start";

    //property changed implementation
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));

    //visibility and content properties

    //dashboard and content visibility
    private Visibility _dashboardVisibility = Visibility.Visible;
    public Visibility DashboardVisibility
    {
        get => _dashboardVisibility;
        set { _dashboardVisibility = value; OnPropertyChanged(); }
    }

    //content visibility
    private Visibility _contentVisibility = Visibility.Collapsed;
    public Visibility ContentVisibility
    {
        get => _contentVisibility;
        set { _contentVisibility = value; OnPropertyChanged(); }
    }

    //loading overlay visibility
    private Visibility _loadingVisibility = Visibility.Collapsed;
    public Visibility LoadingVisibility
    {
        get => _loadingVisibility;
        set { _loadingVisibility = value; OnPropertyChanged(); }
    }

    //main content control
    private object? _mainContent = null;
    public object? MainContent
    {
        get => _mainContent;
        set { _mainContent = value; OnPropertyChanged(); }
    }

    //graph bar properties
    private double _openHeight;
    public double OpenHeight
    {
        get => _openHeight;
        set { _openHeight = value; OnPropertyChanged(); }
    }

    //bar value
    private string _openVal = "0";
    public string OpenVal
    {
        get => _openVal;
        set { _openVal = value; OnPropertyChanged(); }
    }

    //following properties are similar for other order statuses

    //InProgress
    private double _inProgressHeight;
    public double InProgressHeight
    {
        get => _inProgressHeight;
        set { _inProgressHeight = value; OnPropertyChanged(); }
    }

    //bar value
    private string _inProgressVal = "0";
    public string InProgressVal
    {
        get => _inProgressVal;
        set { _inProgressVal = value; OnPropertyChanged(); }
    }

    //Delivered
    private double _deliveredHeight;
    public double DeliveredHeight
    {
        get => _deliveredHeight;
        set { _deliveredHeight = value; OnPropertyChanged(); }
    }

    //bar value
    private string _deliveredVal = "0";
    public string DeliveredVal
    {
        get => _deliveredVal;
        set { _deliveredVal = value; OnPropertyChanged(); }
    }

    //Refused
    private double _refusedHeight;
    public double RefusedHeight
    {
        get => _refusedHeight;
        set { _refusedHeight = value; OnPropertyChanged(); }
    }

    //bar value
    private string _refusedVal = "0";
    public string RefusedVal
    {
        get => _refusedVal;
        set { _refusedVal = value; OnPropertyChanged(); }
    }

    //Cancelled
    private double _cancelledHeight;
    public double CancelledHeight
    {
        get => _cancelledHeight;
        set { _cancelledHeight = value; OnPropertyChanged(); }
    }

    //bar value
    private string _cancelledVal = "0";
    public string CancelledVal
    {
        get => _cancelledVal;
        set { _cancelledVal = value; OnPropertyChanged(); }
    }

    //admin password input binding
    private string _adminPasswordInput = "********";
    public string AdminPasswordInput
    {
        get => _adminPasswordInput;
        set { _adminPasswordInput = value; OnPropertyChanged(); }
    }

    //Dependency properties

    //current time property
    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));

    //configuration property
    public BO.Config? Configuration
    {
        get { return (BO.Config?)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow), new PropertyMetadata());

    /// <summary>
    /// constructor for main window
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// load event handler for the window. Initializes data,
    /// subscribes observers, and sets up the initial UI state.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //initialize data
            CurrentTime = s_bl.Admin.GetClock();
            Configuration = s_bl.Admin.GetConfig();

            //subscribe observers
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);

            s_bl.Order.AddObserver(OrderObserver);

            //initial UI setup
            if (Configuration != null)
            {
                AdminPasswordInput = "********";
                RefreshGraph();
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Error loading data: {ex.Message}", "Error");
        }
    }

    /// <summary>
    /// observer method for order changes.
    /// </summary>
    private void OrderObserver()
    {
        //entry checks and mutex handling
        if (_observerMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            RefreshGraph();

            // exit and check for restart
            if (await _observerMutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderObserver();
        });
    }

    /// <summary>
    /// Updates the current time by retrieving the clock value from the administration service.
    /// </summary>
    /// <remarks>This method is invoked on the UI thread using the dispatcher to ensure thread safety  when
    /// updating the <see cref="CurrentTime"/> property.</remarks>
    private void clockObserver()
    {
        //entry checks and mutex handling
        if (_observerMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            CurrentTime = s_bl.Admin.GetClock();

            //exit and check for restart
            if (await _observerMutex.UnsetLoadInProgressAndCheckRestartRequested())
                clockObserver();
        });
    }
    /// <summary>
    /// Configures the observer by retrieving the current configuration from the admin service.
    /// </summary>
    /// <remarks>This method must be called on the UI thread as it uses the dispatcher to update the
    /// configuration.</remarks>
    private void configObserver()
    {
        //entry checks and mutex handling
        if (_observerMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            Configuration = s_bl.Admin.GetConfig();

            // exit and check for restart
            if (await _observerMutex.UnsetLoadInProgressAndCheckRestartRequested())
                configObserver();
        });
    }

    /// <summary>
    /// Refreshes the graphical representation of order statuses by updating the values of the corresponding bars.
    /// </summary>
    /// <remarks>This method retrieves the summary of order quantities for each status using the
    /// administrator's ID and updates the graphical bars to reflect the current state of orders. The method handles the
    /// statuses Open, In Progress, Delivered, Refused, and Cancelled.</remarks>
    private void RefreshGraph()
    {
        try
        {
            if (Configuration == null)
                return;

            int[] quantities = s_bl.Order.GetOrderSummaryQuantities(Configuration.AdminId);
            int maxVal = quantities.Max();

            if (maxVal == 0)
                maxVal = 1;

            UpdateBarData(quantities[(int)BO.OrderStatus.Open], maxVal, v => OpenVal = v, h => OpenHeight = h);
            UpdateBarData(quantities[(int)BO.OrderStatus.InProgress], maxVal, v => InProgressVal = v, h => InProgressHeight = h);
            UpdateBarData(quantities[(int)BO.OrderStatus.Delivered], maxVal, v => DeliveredVal = v, h => DeliveredHeight = h);
            UpdateBarData(quantities[(int)BO.OrderStatus.Refused], maxVal, v => RefusedVal = v, h => RefusedHeight = h);
            UpdateBarData(quantities[(int)BO.OrderStatus.Cancelled], maxVal, v => CancelledVal = v, h => CancelledHeight = h);
        }
        catch (Exception) { }
    }

    /// <summary>
    /// Updates the bar data for the specified order status.
    /// </summary>
    /// <param name="value">The current value of the order status.</param>
    /// <param name="maxValue">The maximum value among all order statuses.</param>
    /// <param name="SetVal">Action to set the textual representation of the value.</param>
    /// <param name="SetHeight">Action to set the height of the bar.</param>
    private void UpdateBarData(int value, int maxValue, Action<string> SetVal, Action<double> SetHeight)
    {
        SetVal(value.ToString());

        double maxHeight = 110;
        double newHeight = ((double)value / maxValue) * maxHeight;

        if (value > 0 && newHeight < 20)
            newHeight = 20;

        if (value == 0)
            newHeight = 5;

        SetHeight(newHeight);
    }

    /// <summary>
    /// handles the click event of the "Save Configuration" button.
    /// Validates and saves the current configuration.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Configuration == null)
                return;

            // Validate configuration
            if (AdminPasswordInput != "********")
                Configuration.AdminPassword = AdminPasswordInput;

            s_bl.Admin.SetConfig(Configuration);

            CustomMessageBox.Show("Configuration saved successfully!", "Success");

            //initialize password field
            AdminPasswordInput = "********";
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Failed to save config: {ex.Message}", "Validation Error");
        }
    }

    /// <summary>
    /// starts or stops the simulator based on its current
    /// state when the corresponding button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnSimulator_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!IsSimulatorRunning)
            {
                // starting
                s_bl.Admin.StartSimulator(SimulatorInterval);
                IsSimulatorRunning = true;
            }
            else
            {
                // stopping
                s_bl.Admin.StopSimulator();
                IsSimulatorRunning = false;
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show(ex.Message, "Simulator Error", MessageType.Error);
        }
    }

    /// <summary>
    /// Handles the click event of the "Initialize Database" button. Prompts the user for confirmation before
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BtnInit_Click(object sender, RoutedEventArgs e)
    {
        //check if a background process is already running
        if (LoadingVisibility == Visibility.Visible)
            return;

        // Confirm initialization
        if (!CustomMessageBox.ShowQuestion("Initialize Database with dummy data?", "Initialize"))
            return;

        //convert sender to button
        var btn = sender as Button;

        try
        {
            //lock the current button
            if (btn != null) btn.IsEnabled = false;

            LoadingVisibility = Visibility.Visible;

            //temp disconnection of observers to avoid multiple updates during init
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Admin.RemoveClockObserver(clockObserver);

            s_bl.Order.RemoveObserver(OrderObserver);

            // Perform database initialization asynchronously
            await System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(250);
                s_bl.Admin.InitializeDB();
                System.Threading.Thread.Sleep(250);
            });

            // Reattach observers
            s_bl.Admin.AddConfigObserver(configObserver);
            s_bl.Admin.AddClockObserver(clockObserver);

            s_bl.Order.AddObserver(OrderObserver);

            clockObserver(); // Update time immediately after init

            // Refresh configuration and graph
            Configuration = s_bl.Admin.GetConfig();

            RefreshGraph();

            LoadingVisibility = Visibility.Collapsed;

            CustomMessageBox.Show("Database initialized successfully.", "Done");
        }
        catch (Exception ex)
        {
            // Hide loading overlay and show error message
            LoadingVisibility = Visibility.Collapsed;
            CustomMessageBox.Show("Error: " + ex.Message, "Error");
        }
        finally
        {
            //added block to ensure UI is always released
            LoadingVisibility = Visibility.Collapsed;

            if (btn != null)
                btn.IsEnabled = true;
        }
    }

    /// <summary>
    /// handles the click event of the "Reset Database" button.
    /// Prompts the user for confirmation before
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        //check if a background process is already running
        if (LoadingVisibility == Visibility.Visible)
            return;

        // Confirm reset
        if (!CustomMessageBox.ShowQuestion("RESET the DB? ALL data will be deleted.", "Reset"))
            return;

        // convert sender to button
        var btn = sender as Button;

        try
        {
            //lock the current button
            if (btn != null)
                btn.IsEnabled = false;

            LoadingVisibility = Visibility.Visible;

            // temp disconnection of observers to avoid multiple updates during reset
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Admin.RemoveClockObserver(clockObserver);

            s_bl.Order.RemoveObserver(OrderObserver);

            // Perform database reset asynchronously
            await System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(250);
                s_bl.Admin.ResetDB();
                System.Threading.Thread.Sleep(250);
            });

            //reattach observers
            s_bl.Admin.AddConfigObserver(configObserver);
            s_bl.Admin.AddClockObserver(clockObserver);

            s_bl.Order.AddObserver(OrderObserver);

            clockObserver(); // Update time immediately after init

            // Refresh configuration and graph
            Configuration = s_bl.Admin.GetConfig();

            RefreshGraph();

            LoadingVisibility = Visibility.Collapsed;

            CustomMessageBox.Show("Database reset successfully.", "Done");
        }
        catch (Exception ex)
        {
            // Hide loading overlay and show error message
            LoadingVisibility = Visibility.Collapsed;
            CustomMessageBox.Show("Error: " + ex.Message, "Error");
        }
        finally
        {
            //added block to ensure UI is always released
            LoadingVisibility = Visibility.Collapsed;
            if (btn != null) btn.IsEnabled = true;
        }
    }

    /// <summary>
    /// Displays the dashboard by hiding the main content and making the dashboard visible.
    /// </summary>
    /// <remarks>This method collapses the visibility of the main content control, clears its content,  and
    /// ensures the dashboard grid is visible. It also refreshes the graph displayed on the dashboard.</remarks>
    private void ShowDashboard()
    {
        DashboardVisibility = Visibility.Visible;
        ContentVisibility = Visibility.Collapsed;
        RefreshGraph();
    }

    /// <summary>
    /// displays the dashboard when the corresponding button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDashboard_Click(object sender, RoutedEventArgs e) => ShowDashboard();

    /// <summary>
    /// changes the main content to display the courier list when the corresponding button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCouriers_Click(object sender, RoutedEventArgs e)
    {
        if (Configuration == null)
            return;

        var courierList = new PL.Courier.CourierListWindow(Configuration.AdminId);

        courierList.RequestDashboard += (s, args) => ShowDashboard();
        courierList.RequestOrderList += (s, args) => BtnList_Click(this, new RoutedEventArgs());

        MainContent = courierList;

        DashboardVisibility = Visibility.Collapsed;
        ContentVisibility = Visibility.Visible;
    }

    /// <summary>
    /// cleans up observers when the window is closed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Closed(object sender, EventArgs e)
    {
        //stop simulator if running
        if (IsSimulatorRunning)
            s_bl.Admin.StopSimulator();

        s_bl.Admin.RemoveClockObserver(clockObserver);
        s_bl.Admin.RemoveConfigObserver(configObserver);

        // cleanup order observer
        s_bl.Order.RemoveObserver(OrderObserver);
    }

    //placeholder - to be implemented in the future
    /// <summary>
    /// Opens the Order List Management screen.
    /// </summary>
    private void BtnList_Click(object sender, RoutedEventArgs e)
    {
        if (Configuration == null)
            return;

        var orderList = new PL.Order.OrderListWindow(Configuration.AdminId);

        orderList.RequestDashboard += (s, args) => ShowDashboard();
        orderList.RequestCouriers += (s, args) => BtnCouriers_Click(this, new RoutedEventArgs());

        MainContent = orderList;

        DashboardVisibility = Visibility.Collapsed;
        ContentVisibility = Visibility.Visible;
    }

    //clock forwarding buttons handlers

    private void BtnAddMinute_Click(object sender, RoutedEventArgs e)
        => s_bl.Admin.ForwardClock(BO.TimeUnit.Minutes);
    private void BtnAddHour_Click(object sender, RoutedEventArgs e)
        => s_bl.Admin.ForwardClock(BO.TimeUnit.Hours);
    private void BtnAddDay_Click(object sender, RoutedEventArgs e)
        => s_bl.Admin.ForwardClock(BO.TimeUnit.Days);
    private void BtnAddMonth_Click(object sender, RoutedEventArgs e)
        => s_bl.Admin.ForwardClock(BO.TimeUnit.Months);
    private void BtnAddYear_Click(object sender, RoutedEventArgs e)
        => s_bl.Admin.ForwardClock(BO.TimeUnit.Years);
}