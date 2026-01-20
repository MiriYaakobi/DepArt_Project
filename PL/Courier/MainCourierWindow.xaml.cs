using PL.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Courier;

/// <summary>
/// main window for courier operations
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
public partial class MainCourierWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly ObserverMutex _clockMutex = new();

    private int courierId;
    private string enteredPassword = "";

    public Array OrderEndStatuses { get; } = Enum.GetValues(typeof(BO.OrderEndStatus));
    public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryType));


    //Dependency Properties

    //order completion status
    public BO.OrderEndStatus SelectedEndStatus
    {
        get { return (BO.OrderEndStatus)GetValue(SelectedEndStatusProperty); }
        set { SetValue(SelectedEndStatusProperty, value); }
    }

    public static readonly DependencyProperty SelectedEndStatusProperty =
        DependencyProperty.Register("SelectedEndStatus", typeof(BO.OrderEndStatus), typeof(MainCourierWindow), new PropertyMetadata(BO.OrderEndStatus.Delivered));

    //main view content
    public object? MainViewContent
    {
        get { return (object?)GetValue(MainViewContentProperty); }
        set { SetValue(MainViewContentProperty, value); }
    }

    public static readonly DependencyProperty MainViewContentProperty =
        DependencyProperty.Register("MainViewContent", typeof(object), typeof(MainCourierWindow));

    //dashboard visibility
    public Visibility DashboardVisibility
    {
        get { return (Visibility)GetValue(DashboardVisibilityProperty); }
        set { SetValue(DashboardVisibilityProperty, value); }
    }

    public static readonly DependencyProperty DashboardVisibilityProperty =
        DependencyProperty.Register("DashboardVisibility", typeof(Visibility), typeof(MainCourierWindow), new PropertyMetadata(Visibility.Visible));

    //content visibility
    public Visibility ContentVisibility
    {
        get { return (Visibility)GetValue(ContentVisibilityProperty); }
        set { SetValue(ContentVisibilityProperty, value); }
    }

    public static readonly DependencyProperty ContentVisibilityProperty =
        DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(MainCourierWindow), new PropertyMetadata(Visibility.Collapsed));

    //visual password
    public string VisualPassword
    {
        get { return (string)GetValue(VisualPasswordProperty); }
        set { SetValue(VisualPasswordProperty, value); }
    }

    public static readonly DependencyProperty VisualPasswordProperty =
        DependencyProperty.Register("VisualPassword", typeof(string), typeof(MainCourierWindow), new PropertyMetadata("********"));

    //current courier
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(MainCourierWindow));


    /// <summary>
    /// registered observer for courier and order changes
    /// </summary>
    private void CourierObserver()
    {
        //entry checks and mutex handling
        if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        // update UI thread
        Dispatcher.BeginInvoke(async () =>
        {
            RefreshCourierState();

            // exit observer and check for restart
            if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
            {
                CourierObserver();
            }
        });
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="courierId"></param>
    public MainCourierWindow(int courierId)
    {
        InitializeComponent();

        this.courierId = courierId;

        //register observers
        s_bl.Courier.AddObserver(CourierObserver);
        s_bl.Order.AddObserver(CourierObserver);

        this.Closing += (s, e) => {
            s_bl.Courier.RemoveObserver(CourierObserver);
            s_bl.Order.RemoveObserver(CourierObserver);
        };

        RefreshCourierState();
    }

    /// <summary>
    /// refresh courier state from BL
    /// </summary>
    private void RefreshCourierState()
    {
        try
        {
            CurrentCourier = s_bl.Courier.Read(courierId, courierId)!;

            //reset password fields
            VisualPassword = "********";
            enteredPassword = "";
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show("Error loading data: " + ex.Message, "Error");
            this.Close();
        }
    }

    /// <summary>
    /// dashboard button click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        MainViewContent = null;
        ContentVisibility = Visibility.Collapsed;
        DashboardVisibility = Visibility.Visible;
    }

    /// <summary>
    /// history button click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
        var historyView = new PL.Courier.CourierHistoryView();
        historyView.CourierId = courierId;

        //register navigation events
        historyView.RequestDashboardView += (s, args) =>
        {
            MainViewContent = null;
            ContentVisibility = Visibility.Collapsed;
            DashboardVisibility = Visibility.Visible;
        };

        // direct navigation from history to pick order
        historyView.RequestPickOrderView += (s, args) =>
        {
            BtnFindOrder_Click(sender, e);
        };

        DashboardVisibility = Visibility.Collapsed;
        MainViewContent = historyView;
        ContentVisibility = Visibility.Visible;
    }

    /// <summary>
    /// find order button click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnFindOrder_Click(object sender, RoutedEventArgs e)
    {
        //check if courier already has an active order
        if (CurrentCourier.CurrentOrder != null)
        {
            CustomMessageBox.Show("You already have an active order. Complete it first!", "Alert");
            return;
        }

        // create pick order view
        var pickOrderView = new PL.Courier.CourierPickOrderView();
        pickOrderView.CourierId = courierId;
        pickOrderView.CurrentCourier = this.CurrentCourier;

        // register navigation events
        pickOrderView.RequestDashboardView += (s, args) =>
        {
            MainViewContent = null;
            ContentVisibility = Visibility.Collapsed;
            DashboardVisibility = Visibility.Visible;
            RefreshCourierState();
        };

        //  direct navigation from pick order to history
        pickOrderView.RequestHistoryView += (s, args) =>
        {
            BtnHistory_Click(sender, e);
        };

        // show pick order view
        DashboardVisibility = Visibility.Collapsed;
        MainViewContent = pickOrderView;
        ContentVisibility = Visibility.Visible;
    }

    /// <summary>
    /// password textbox text input - mask input with '*'
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Password_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (VisualPassword == "********") //first input - clear existing
        {
            VisualPassword = "";
            enteredPassword = "";
        }

        enteredPassword += e.Text;

        if (sender is TextBox txt)      //append '*' to visual password
        {
            txt.Text += "*";
            txt.CaretIndex = txt.Text.Length;
        }

        e.Handled = true;
    }

    /// <summary>
    /// password textbox key down - handle backspace
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Password_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        TextBox? txt = sender as TextBox;

        if (txt == null)
            return;

        if (VisualPassword == "********") //first input - clear existing
        {
            VisualPassword = "";
            enteredPassword = "";
        }

        if (e.Key == Key.Back)          //handle backspace
        {
            if (enteredPassword.Length > 0)
            {
                enteredPassword = enteredPassword.Substring(0, enteredPassword.Length - 1);

                if (txt.Text.Length > 0)
                {
                    txt.Text = txt.Text.Substring(0, txt.Text.Length - 1);
                    txt.CaretIndex = txt.Text.Length;
                }
            }
            e.Handled = true;
        }

        else if (e.Key == Key.Space)  //handle space
        {
            enteredPassword += " "; txt.Text += "*"; txt.CaretIndex = txt.Text.Length; e.Handled = true;
        }
    }

    /// <summary>
    /// can execute command binding - disable certain commands
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = false;
        e.Handled = true;
    }

    /// <summary>
    /// update button click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            double? companyLimit = s_bl.Admin.GetConfig().DeliveryMaxDistance;

            // validate max distance against company limit
            if (companyLimit.HasValue &&
                CurrentCourier.MaxDistance.HasValue &&
                CurrentCourier.MaxDistance.Value > companyLimit.Value)
            {
                CustomMessageBox.Show($"Shipping max distance must be less than or equal to the company's shipping distance.", "Error");
                return;
            }

            // determine password to store
            if (!string.IsNullOrEmpty(enteredPassword))
                CurrentCourier.Password = enteredPassword;

            // if password not changed, retain original
            else
            {
                BO.Courier originalCourierFromDb = s_bl.Courier.Read(CurrentCourier.Id, CurrentCourier.Id)!;
                CurrentCourier.Password = originalCourierFromDb.Password;
            }

            s_bl.Courier.Update(courierId, CurrentCourier);

            CustomMessageBox.Show("Profile updated successfully!", "Success");

            RefreshCourierState();
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Update failed: {ex.Message}", "Error");
        }
    }

    /// <summary>
    /// finish order button click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier.CurrentOrder == null)
            return;

        // premmition to complete order
        if (CustomMessageBox.ShowQuestion("Mark this order as Delivered?", "Confirm Completion"))
        {
            try
            {
                int deliveryId = CurrentCourier.CurrentOrder.DeliveryId;
                BO.OrderEndStatus selectedStatus = this.SelectedEndStatus;

                s_bl.Order.CompleteDelivery(courierId, courierId, deliveryId, selectedStatus);

                CustomMessageBox.Show("Order delivery completed!", "Great Job!", MessageType.Success);

                RefreshCourierState();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to complete order: {ex.Message}", "Error", MessageType.Error);
            }
        }
    }
}