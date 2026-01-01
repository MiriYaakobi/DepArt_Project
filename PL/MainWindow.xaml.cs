using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
// using PL.Courier; // ודאי שהשורה הזו קיימת או שה-Namespace מלא

namespace PL
{
    public partial class MainWindow : Window
    {
        private BlApi.IBl s_bl = BlApi.Factory.Get();
        private int _adminId;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                BO.Config config = s_bl.Admin.GetConfig();
                _adminId = config.AdminId;

                txtAdminId.Text = config.AdminId.ToString();
                txtAddress.Text = config.CompenyAddress;
                txtMaxDist.Text = config.DeliveryMaxDistance?.ToString() ?? "0";

                txtCarSpeed.Text = config.AverageVehicleSpeedKmH.ToString();
                txtMotoSpeed.Text = config.AverageMotorcycleSpeedKmH.ToString();
                txtBikeSpeed.Text = config.AverageBicycleSpeedKmH.ToString();
                txtFootSpeed.Text = config.AverageByFootSpeedKmH.ToString();

                txtRange.Text = config.MaxDeliveryRange.ToString();
                txtRiskRange.Text = config.RiskRange.ToString();
                txtInactivity.Text = config.InactivityTimeRange.ToString();

                DateTime now = s_bl.Admin.GetClock();
                txtClockTime.Text = now.ToString("HH:mm:ss");
                txtClockDate.Text = now.ToString("dd/MM/yyyy");

                RefreshGraph();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error loading data: {ex.Message}", "Error");
            }
        }

        private void RefreshGraph()
        {
            try
            {
                int[] quantities = s_bl.Order.GetOrderSummaryQuantities(_adminId);

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
            if (maxValue == 0) maxValue = 1;

            double maxHeight = 150;
            double newHeight = ((double)value / maxValue) * maxHeight;

            if (value > 0 && newHeight < 20) newHeight = 20;
            if (value == 0) newHeight = 5;

            bar.Height = newHeight;

            double minOpacity = 0.3;
            double maxOpacity = 1.0;
            double ratio = (double)value / maxValue;
            double newOpacity = minOpacity + (ratio * (maxOpacity - minOpacity));
            if (value == 0) newOpacity = 0.2;

            bar.Opacity = newOpacity;
        }

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BO.Config config = s_bl.Admin.GetConfig();

                if (double.TryParse(txtMaxDist.Text, out double dist)) config.DeliveryMaxDistance = dist;
                config.CompenyAddress = txtAddress.Text;

                if (double.TryParse(txtCarSpeed.Text, out double car)) config.AverageVehicleSpeedKmH = car;
                if (double.TryParse(txtMotoSpeed.Text, out double moto)) config.AverageMotorcycleSpeedKmH = moto;
                if (double.TryParse(txtBikeSpeed.Text, out double bike)) config.AverageBicycleSpeedKmH = bike;
                if (double.TryParse(txtFootSpeed.Text, out double foot)) config.AverageByFootSpeedKmH = foot;

                if (TimeSpan.TryParse(txtRange.Text, out TimeSpan range)) config.MaxDeliveryRange = range;
                if (TimeSpan.TryParse(txtRiskRange.Text, out TimeSpan risk)) config.RiskRange = risk;
                if (TimeSpan.TryParse(txtInactivity.Text, out TimeSpan inact)) config.InactivityTimeRange = inact;

                s_bl.Admin.SetConfig(config);
                CustomMessageBox.Show("Configuration saved successfully!", "Success");
                LoadData();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to save config: {ex.Message}", "Validation Error");
            }
        }

        private void BtnInit_Click(object sender, RoutedEventArgs e)
        {
            bool confirm = CustomMessageBox.ShowQuestion("Initialize DB? Current data will be lost.", "Initialize");
            if (confirm)
            {
                try
                {
                    s_bl.Admin.InitializeDB();
                    CustomMessageBox.Show("Database initialized.", "Done");
                    LoadData();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            bool confirm = CustomMessageBox.ShowQuestion("RESET the DB? ALL data will be deleted.", "Reset");
            if (confirm)
            {
                try
                {
                    s_bl.Admin.ResetDB();
                    CustomMessageBox.Show("Database reset.", "Done");
                    LoadData();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void ShowDashboard()
        {
            // הסתרת הפקד של הרשימה והצגת הדשבורד מחדש
            MainContentControl.Visibility = Visibility.Collapsed;
            MainContentControl.Content = null; // ניקוי הזיכרון
            DashboardGrid.Visibility = Visibility.Visible;
            LoadData();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        // --- התיקון הגדול כאן ---
        private void BtnCouriers_Click(object sender, RoutedEventArgs e)
        {
            // 1. יצירת החלון (UserControl) עם שליחת ה-ID
            var courierList = new PL.Courier.CourierListWindow(_adminId);

            // 2. הרשמה לאירוע חזרה (שימוש בשם הפונקציה הקיים אצלך: ShowDashboard)
            courierList.RequestDashboard += (s, args) => ShowDashboard();

            // 3. הכנסה לתוך הפקד (שימוש בשם הקיים אצלך: MainContentControl)
            MainContentControl.Content = courierList;

            // 4. החלפת תצוגה
            MainContentControl.Visibility = Visibility.Visible;
            DashboardGrid.Visibility = Visibility.Collapsed;
        }

        private void BtnList_Click(object sender, RoutedEventArgs e)
        {
            // Future implementation
        }

        private void BtnAddMinute_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Minutes);
            LoadData();
        }

        private void BtnAddHour_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Hours);
            LoadData();
        }

        private void BtnAddDay_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Days);
            LoadData();
        }

        private void BtnAddMonth_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Months);
            LoadData();
        }

        private void BtnAddYear_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Years);
            LoadData();
        }
    }
}