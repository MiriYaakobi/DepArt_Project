using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Courier
{
    public partial class MainCourierWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private int _courierId;
        private string enteredPassword = "";

        public object? MainViewContent
        {
            get { return (object?)GetValue(MainViewContentProperty); }
            set { SetValue(MainViewContentProperty, value); }
        }
        public static readonly DependencyProperty MainViewContentProperty =
            DependencyProperty.Register("MainViewContent", typeof(object), typeof(MainCourierWindow));

        public Visibility DashboardVisibility
        {
            get { return (Visibility)GetValue(DashboardVisibilityProperty); }
            set { SetValue(DashboardVisibilityProperty, value); }
        }
        public static readonly DependencyProperty DashboardVisibilityProperty =
            DependencyProperty.Register("DashboardVisibility", typeof(Visibility), typeof(MainCourierWindow), new PropertyMetadata(Visibility.Visible));

        public Visibility ContentVisibility
        {
            get { return (Visibility)GetValue(ContentVisibilityProperty); }
            set { SetValue(ContentVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ContentVisibilityProperty =
            DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(MainCourierWindow), new PropertyMetadata(Visibility.Collapsed));


        // --- פרטי השליח ---

        public string VisualPassword
        {
            get { return (string)GetValue(VisualPasswordProperty); }
            set { SetValue(VisualPasswordProperty, value); }
        }
        public static readonly DependencyProperty VisualPasswordProperty =
            DependencyProperty.Register("VisualPassword", typeof(string), typeof(MainCourierWindow), new PropertyMetadata("********"));

        public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryType));

        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(MainCourierWindow));


        public MainCourierWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;
            DataContext = this;
            RefreshCourierState();
        }

        private void RefreshCourierState()
        {
            try
            {
                CurrentCourier = s_bl.Courier.Read(_courierId, _courierId)!;
                VisualPassword = "********";
                enteredPassword = "";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error loading data: " + ex.Message, "Error");
                this.Close();
            }
        }

        // --- ניווט ---

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainViewContent = null;
            ContentVisibility = Visibility.Collapsed;
            DashboardVisibility = Visibility.Visible;
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyView = new PL.Courier.CourierHistoryView();
            historyView.CourierId = _courierId;

            historyView.RequestDashboardView += (s, args) =>
            {
                MainViewContent = null;
                ContentVisibility = Visibility.Collapsed;
                DashboardVisibility = Visibility.Visible;
            };

            historyView.RequestPickOrderView += (s, args) =>
            {
                BtnFindOrder_Click(sender, e);
            };

            DashboardVisibility = Visibility.Collapsed;
            MainViewContent = historyView;
            ContentVisibility = Visibility.Visible;
        }

        private void BtnFindOrder_Click(object sender, RoutedEventArgs e)
        {
            // 1. בדיקה אם לשליח כבר יש הזמנה פעילה (אופציונלי, תלוי בדרישות)
            if (CurrentCourier.CurrentOrder != null)
            {
                CustomMessageBox.Show("You already have an active order. Complete it first!", "Alert");
                return;
            }

            // 2. יצירת המסך
            var pickOrderView = new PL.Courier.CourierPickOrderView();
            pickOrderView.CourierId = _courierId;

            // 3. הרשמה לאירועי ניווט (חזרה לדשבורד או להיסטוריה)
            pickOrderView.RequestDashboardView += (s, args) =>
            {
                MainViewContent = null;
                ContentVisibility = Visibility.Collapsed;
                DashboardVisibility = Visibility.Visible;

                // חשוב: לרענן את מצב השליח כי אולי הוא בחר הזמנה!
                RefreshCourierState();
            };

            pickOrderView.RequestHistoryView += (s, args) =>
            {
                // מעבר ישיר מהבחירה להיסטוריה
                BtnHistory_Click(sender, e);
            };

            // 4. הצגת המסך
            DashboardVisibility = Visibility.Collapsed;
            MainViewContent = pickOrderView;
            ContentVisibility = Visibility.Visible;
        }

        // --- שאר הפונקציות ---

        private void Password_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (VisualPassword == "********") { VisualPassword = ""; enteredPassword = ""; }
            enteredPassword += e.Text;
            if (sender is TextBox txt) { txt.Text += "●"; txt.CaretIndex = txt.Text.Length; }
            e.Handled = true;
        }

        private void Password_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt == null) return;
            if (VisualPassword == "********") { VisualPassword = ""; enteredPassword = ""; }

            if (e.Key == Key.Back)
            {
                if (enteredPassword.Length > 0)
                {
                    enteredPassword = enteredPassword.Substring(0, enteredPassword.Length - 1);
                    if (txt.Text.Length > 0) { txt.Text = txt.Text.Substring(0, txt.Text.Length - 1); txt.CaretIndex = txt.Text.Length; }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                enteredPassword += " "; txt.Text += "●"; txt.CaretIndex = txt.Text.Length; e.Handled = true;
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = false; e.Handled = true; }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BO.Courier savedCourier = s_bl.Courier.Read(_courierId, _courierId)!;
                double? companyLimit = s_bl.Admin.GetConfig().DeliveryMaxDistance;
                if (companyLimit.HasValue && CurrentCourier.MaxDistance.HasValue && CurrentCourier.MaxDistance.Value > companyLimit.Value)
                {
                    CustomMessageBox.Show($"Shipping max distance must be less than or equal to the company's shipping distance.", "Error");
                    return;
                }
                if (savedCourier.CurrentOrder != null && savedCourier.TypeOfDelivery != CurrentCourier.TypeOfDelivery)
                {
                    CustomMessageBox.Show("Cannot change vehicle...", "Error");
                    CurrentCourier.TypeOfDelivery = savedCourier.TypeOfDelivery;
                    return;
                }

                if (!string.IsNullOrEmpty(enteredPassword)) CurrentCourier.Password = enteredPassword;
                else
                {
                    BO.Courier originalCourierFromDb = s_bl.Courier.Read(CurrentCourier.Id, CurrentCourier.Id)!;
                    CurrentCourier.Password = originalCourierFromDb.Password;
                }

                s_bl.Courier.Update(_courierId, CurrentCourier);
                CustomMessageBox.Show("Profile updated successfully!", "Success");
                RefreshCourierState();
            }
            catch (Exception ex) { CustomMessageBox.Show($"Update failed: {ex.Message}", "Error"); }
        }

        private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
        {
            // 1. הגנה: אם אין הזמנה פעילה, אין מה לסיים
            if (CurrentCourier.CurrentOrder == null) return;

            // 2. שאלת אישור
            if (CustomMessageBox.ShowQuestion("Mark this order as Delivered?", "Confirm Completion"))
            {
                try
                {
                    // === הנה התשובה לשאלתך ===
                    // המזהה נמצא בתוך האובייקט של ההזמנה הנוכחית
                    // (ייתכן שב-BO קראת לזה Id או OrderId, תבדקי מה משלים לך ה-Intellisense)
                    int deliveryId = CurrentCourier.CurrentOrder.DeliveryId;

                    // 3. קריאה לפונקציה בממשק
                    // הפרמטרים:
                    // 1. requestingUserId -> ה-ID של השליח (כי הוא המשתמש המחובר)
                    // 2. courierId -> ה-ID של השליח שמבצע
                    // 3. deliveryId -> ה-ID ששלפנו הרגע מהאובייקט
                    s_bl.Order.CompleteDelivery(_courierId, _courierId, deliveryId);

                    CustomMessageBox.Show("Order delivery completed!", "Great Job", MessageType.Success);

                    // 4. רענון המסך (ההזמנה תיעלם והסטטוס יתעדכן)
                    RefreshCourierState();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Failed to complete order: {ex.Message}", "Error", MessageType.Error);
                }
            }
        }
    }
}