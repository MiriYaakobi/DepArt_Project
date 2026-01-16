using BlApi; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL.Order
{
    public partial class OrderListWindow : UserControl
    {
        // גישה לשכבת ה-BL
        private BlApi.IBl s_bl = BlApi.Factory.Get();

        // משתנה לשמירת ה-ID של המנהל המחובר
        private int AdminID;

        public event EventHandler? RequestDashboard;
        public event EventHandler? RequestCouriers;

        // תכונת תלות לרשימת ההזמנות
        public IEnumerable<BO.OrderInList> OrderList
        {
            get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }

        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow));

        // תכונה לסינון (סטטוס)
        public object StatusFilter { get; set; } = "All";
        private IEnumerable<BO.OrderInList>? AllOrders;

        // בנאי המקבל את מזהה המנהל (ברירת מחדל 1 אם לא נשלח)
        public OrderListWindow(int adminId = 1)
        {
            InitializeComponent();
            AdminID = adminId;
            StatusFilter = "All";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            // כאן תוסיפי את ה-Observer כשהוא יהיה מוכן ב-BL
            s_bl.Order.AddObserver(OrderListObserver);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.RemoveObserver(OrderListObserver);
        }

        private void OrderListObserver()
        {
            Dispatcher.Invoke(() => LoadData());
        }

        private void LoadData()
        {
            try
            {
                // תיקון השגיאה: העברת AdminID לפונקציה ReadAll
                AllOrders = s_bl.Order.ReadAll(AdminID);
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            if (AllOrders == null) return;

            var tempAddList = AllOrders;

            // סינון סטטוס
            if (StatusFilter is BO.OrderStatus selectedStatus)
            {
                tempAddList = tempAddList.Where(item => item.StatusOfOrder == selectedStatus);
            }

            // סינון חיפוש (לפי ID או OrderID)
            string searchText = SearchBox.Text;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                tempAddList = tempAddList.Where(item =>
                    (item.Id?.ToString().Contains(searchText) ?? false) ||
                    (item.OrderId.ToString().Contains(searchText))
                );
            }

            OrderList = tempAddList.ToList();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            new OrderWindow().Show();
        }

        private void OpenOrderWindow(int id = 0)
        {
            var window = new OrderWindow(id);
            window.Closed += (s, args) => LoadData();
            window.Show();
        }

        private void OrderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is BO.OrderInList selectedOrder)
                OpenOrderWindow(selectedOrder.OrderId);
        }

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
                    try
                    {
                        // --- קריאה ל-BL ---
                        // ה-BL כבר דואג לשלוח את המייל בתוך הפונקציה הזו!
                        s_bl.Order.Cancel(AdminID, idToCancel);

                        // הצגת הודעת הצלחה למשתמש
                        CustomMessageBox.Show("Order cancelled successfully (Courier notified via email).", "Success");

                        // רענון הרשימה
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show($"System Error: {ex.Message}", "Error");
                    }
                }
            }
        }
        // 1. חזרה לדשבורד
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            RequestDashboard?.Invoke(this, EventArgs.Empty);
        }

        // 2. מעבר לרשימת שליחים (השם חייב להיות זהה למה שכתוב ב-XAML)
        private void BtnCouriers_Click(object sender, RoutedEventArgs e)
        {
            RequestCouriers?.Invoke(this, EventArgs.Empty);
        }

        // 3. רשימת הזמנות (אנחנו כבר כאן - לא עושה כלום)
        private void BtnList_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing, we are already here
        }
    }
}