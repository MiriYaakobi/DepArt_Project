using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{
    public partial class CourierPickOrderView : UserControl
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierPickOrderView));

        public int CourierId
        {
            get { return (int)GetValue(CourierIdProperty); }
            set { SetValue(CourierIdProperty, value); }
        }
        public static readonly DependencyProperty CourierIdProperty =
            DependencyProperty.Register("CourierId", typeof(int), typeof(CourierPickOrderView), new PropertyMetadata(0, OnCourierIdChanged));

        public ObservableCollection<BO.OpenOrderInList> OrdersList { get; set; } = new();

        // תיקון: לוקחים את האינם כמו שהוא, ומוסיפים לו "All" בהתחלה.
        // ככה זה דינמי ונכון הנדסית.
        public IEnumerable<object> SortOptions { get; } =
            new List<object> { "All" }
            .Concat(Enum.GetValues(typeof(BO.OpenOrderFieldSort)).Cast<object>())
            .ToList();

        public object SelectedSortOption
        {
            get { return GetValue(SelectedSortOptionProperty); }
            set { SetValue(SelectedSortOptionProperty, value); }
        }
        public static readonly DependencyProperty SelectedSortOptionProperty =
            DependencyProperty.Register("SelectedSortOption", typeof(object), typeof(CourierPickOrderView), new PropertyMetadata("All"));

        public event EventHandler? RequestDashboardView;
        public event EventHandler? RequestHistoryView;

        public CourierPickOrderView()
        {
            InitializeComponent();
            DataContext = this;

            // האזנה לשינויים
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
        }

        private void OrderListObserver()
        {
            Dispatcher.Invoke(() => RefreshList());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.AddObserver(OrderListObserver);
            RefreshList();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.RemoveObserver(OrderListObserver);
        }

        private static void OnCourierIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CourierPickOrderView view)
            {
                view.RefreshList();
            }
        }

        public void RefreshList()
        {
            if (CourierId == 0) return;

            try
            {
                CurrentCourier = s_bl.Courier.Read(CourierId, CourierId)!;
                double maxDistance = CurrentCourier.MaxDistance ?? double.MaxValue;

                IEnumerable<BO.OpenOrderInList> list = s_bl.Order.ReadAllOpenOrders(CourierId, CourierId);

                // סינון מרחק
                list = list.Where(o => o.AirDistance <= maxDistance);

                // מיון
                if (SelectedSortOption is BO.OpenOrderFieldSort sortEnum)
                {
                    // אם נבחר ערך מהאינם - ממיינים לפיו
                    switch (sortEnum)
                    {
                        case BO.OpenOrderFieldSort.AirDistance:
                            list = list.OrderBy(o => o.AirDistance);
                            break;
                        case BO.OpenOrderFieldSort.ExpectedDeliveryTime:
                            list = list.OrderBy(o => o.RemainingTime);
                            break;
                        // אם יש עוד ערכים באינם, הם ייכנסו לפה
                        default:
                            list = list.OrderBy(o => o.OrderId);
                            break;
                    }
                }
                else
                {
                    // אם נבחר "All" (או כלום) - ממיינים לפי ID כברירת מחדל
                    list = list.OrderBy(o => o.OrderId);
                }

                OrdersList.Clear();
                foreach (var item in list)
                {
                    OrdersList.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching open orders: {ex.Message}");
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshList();
        }

        private void BtnPick_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BO.OpenOrderInList orderToPick)
            {
                try
                {
                    s_bl.Order.ChooseOrder(CourierId, CourierId, orderToPick.OrderId);
                    new CustomMessageBox($"Order #{orderToPick.OrderId} picked successfully!", "Success", false).ShowDialog();
                    RefreshList();
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"Failed to pick order: {ex.Message}", "Error", false).ShowDialog();
                }
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            RequestDashboardView?.Invoke(this, EventArgs.Empty);
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            RequestHistoryView?.Invoke(this, EventArgs.Empty);
        }
    }
}