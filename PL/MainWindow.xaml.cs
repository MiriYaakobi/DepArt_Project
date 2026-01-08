using System.Windows;
using System.Windows.Controls;

namespace PL;

public partial class MainWindow : Window
{
    private BlApi.IBl s_bl = BlApi.Factory.Get();
    private int AdminID;

    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));

    public BO.Config Configuration
    {
        get { return (BO.Config)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow), new PropertyMetadata(null));

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            CurrentTime = s_bl.Admin.GetClock();
            Configuration = s_bl.Admin.GetConfig();

            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);

            if (Configuration != null)
            {
                AdminID = Configuration.AdminId;
                txtPassword.Text = "********";
                RefreshGraph();
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Error loading data: {ex.Message}", "Error");
        }
    }

    private void clockObserver()
    {
        Dispatcher.Invoke(() =>
        {
            CurrentTime = s_bl.Admin.GetClock();
        });
    }

    private void configObserver()
    {
        Dispatcher.Invoke(() =>
        {
            Configuration = s_bl.Admin.GetConfig();
        });
    }

    private void RefreshGraph()
    {
        try
        {
            int[] quantities = s_bl.Order.GetOrderSummaryQuantities(AdminID);

            UpdateSingleBar(barOpen, valOpen, quantities[(int)BO.OrderStatus.Open], quantities);
            UpdateSingleBar(barInProgress, valInProgress, quantities[(int)BO.OrderStatus.InProgress], quantities);
            UpdateSingleBar(barDelivered, valDelivered, quantities[(int)BO.OrderStatus.Delivered], quantities);
            UpdateSingleBar(barRefused, valRefused, quantities[(int)BO.OrderStatus.Refused], quantities);
            UpdateSingleBar(barCancelled, valCancelled, quantities[(int)BO.OrderStatus.Cancelled], quantities);
        }
        catch (Exception) { }
    }

    private void UpdateSingleBar(Border bar, TextBlock textVal, int value, int[] allValues)
    {
        textVal.Text = value.ToString();
        int maxValue = allValues.Max();

        if (maxValue == 0)
            maxValue = 1;

        double maxHeight = 150;
        double newHeight = ((double)value / maxValue) * maxHeight;

        if (value > 0 && newHeight < 20)
            newHeight = 20;

        if (value == 0)
            newHeight = 5;

        bar.Height = newHeight;
        double minOpacity = 0.3;
        double maxOpacity = 1.0;
        double ratio = (double)value / maxValue;
        double newOpacity = minOpacity + (ratio * (maxOpacity - minOpacity));

        if (value == 0)
            newOpacity = 0.2;

        bar.Opacity = newOpacity;
    }

    private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Configuration == null)
                return;

            if (txtPassword.Text != "********")
                Configuration.AdminPassword = txtPassword.Text;

            s_bl.Admin.SetConfig(Configuration);

            CustomMessageBox.Show("Configuration saved successfully!", "Success");
            txtPassword.Text = "********";
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Failed to save config: {ex.Message}", "Validation Error");
        }
    }

    private async void BtnInit_Click(object sender, RoutedEventArgs e)
    {
        if (!CustomMessageBox.ShowQuestion("Initialize Database with dummy data?", "Initialize"))
            return;

        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            await System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(250);
                s_bl.Admin.InitializeDB();
                System.Threading.Thread.Sleep(250);
            });

            Configuration = s_bl.Admin.GetConfig();
            RefreshGraph();
            LoadingOverlay.Visibility = Visibility.Collapsed;
            CustomMessageBox.Show("Database initialized successfully.", "Done");
        }
        catch (Exception ex)
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            CustomMessageBox.Show("Error: " + ex.Message, "Error");
        }
    }

    private async void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        if (!CustomMessageBox.ShowQuestion("RESET the DB? ALL data will be deleted.", "Reset"))
            return;

        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            await System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(250);
                s_bl.Admin.ResetDB();
                System.Threading.Thread.Sleep(250);
            });
            Configuration = s_bl.Admin.GetConfig();
            RefreshGraph();
            LoadingOverlay.Visibility = Visibility.Collapsed;
            CustomMessageBox.Show("Database reset successfully.", "Done");
        }
        catch (Exception ex)
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            CustomMessageBox.Show("Error: " + ex.Message, "Error");
        }
    }

    private void ShowDashboard()
    {
        MainContentControl.Visibility = Visibility.Collapsed;
        MainContentControl.Content = null;
        DashboardGrid.Visibility = Visibility.Visible;
        RefreshGraph();
    }

    private void BtnDashboard_Click(object sender, RoutedEventArgs e) => ShowDashboard();

    private void BtnCouriers_Click(object sender, RoutedEventArgs e)
    {
        var courierList = new PL.Courier.CourierListWindow(AdminID);
        courierList.RequestDashboard += (s, args) => ShowDashboard();
        MainContentControl.Content = courierList;
        MainContentControl.Visibility = Visibility.Visible;
        DashboardGrid.Visibility = Visibility.Collapsed;
    }
    private void Window_Closed(object sender, EventArgs e)
    {
        s_bl.Admin.RemoveClockObserver(clockObserver);
        s_bl.Admin.RemoveConfigObserver(configObserver);
    }

    private void BtnList_Click(object sender, RoutedEventArgs e) { }
    private void BtnAddMinute_Click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(BO.TimeUnit.Minutes);
    private void BtnAddHour_Click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(BO.TimeUnit.Hours);
    private void BtnAddDay_Click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(BO.TimeUnit.Days);
    private void BtnAddMonth_Click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(BO.TimeUnit.Months);
    private void BtnAddYear_Click(object sender, RoutedEventArgs e) => s_bl.Admin.ForwardClock(BO.TimeUnit.Years);
}