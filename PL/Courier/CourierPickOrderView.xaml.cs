using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PL; // חובה בשביל CustomMessageBox

namespace PL.Courier
{
    public partial class CourierPickOrderView : UserControl
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // --- אירועים (Events) ---
        public event EventHandler? RequestDashboardView;
        public event EventHandler? RequestHistoryView;

        // --- Dependency Properties ---

        // 1. פרטי השליח
        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierPickOrderView));

        // 2. מזהה שליח (כולל טריגר לשינוי)
        public int CourierId
        {
            get { return (int)GetValue(CourierIdProperty); }
            set { SetValue(CourierIdProperty, value); }
        }
        public static readonly DependencyProperty CourierIdProperty =
            DependencyProperty.Register("CourierId", typeof(int), typeof(CourierPickOrderView),
                new PropertyMetadata(0, OnCourierIdChanged));

        // 3. רשימת ההזמנות
        public ObservableCollection<BO.OpenOrderInList> OrdersList
        {
            get { return (ObservableCollection<BO.OpenOrderInList>)GetValue(OrdersListProperty); }
            set { SetValue(OrdersListProperty, value); }
        }
        public static readonly DependencyProperty OrdersListProperty =
            DependencyProperty.Register("OrdersList", typeof(ObservableCollection<BO.OpenOrderInList>), typeof(CourierPickOrderView));

        // 4. אפשרויות המיון (רשימה מותאמת אישית)
        public IEnumerable<object> SortOptions { get; } = new List<object>
        {
            "All",                                      // ברירת מחדל
            BO.OpenOrderFieldSort.AirDistance,          // מרחק אווירי
            BO.OpenOrderFieldSort.ExpectedDeliveryTime, // דחיפות
            BO.OpenOrderFieldSort.TimeLinessStatus,     // סטטוס איחור
            BO.OpenOrderFieldSort.Id                    // סדר רץ
        };

        // 5. המיון שנבחר (כולל טריגר לשינוי!)
        public object SelectedSortOption
        {
            get { return GetValue(SelectedSortOptionProperty); }
            set { SetValue(SelectedSortOptionProperty, value); }
        }
        public static readonly DependencyProperty SelectedSortOptionProperty =
            DependencyProperty.Register("SelectedSortOption", typeof(object), typeof(CourierPickOrderView),
                new PropertyMetadata("All", OnSortChanged));


        // --- בנאי (Constructor) ---
        public CourierPickOrderView()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
        }

        // --- ניהול אירועי טעינה ---
        private void OrderListObserver() => RefreshList();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.AddObserver(OrderListObserver);
            RefreshList();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.RemoveObserver(OrderListObserver);
        }

        // --- פונקציות טריגר (Callbacks) ---

        // נקראת כאשר ה-CourierId משתנה
        private static void OnCourierIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CourierPickOrderView view)
            {
                view.RefreshList();
            }
        }

        // נקראת כאשר אפשרות המיון משתנה (OnSortChanged)
        private static void OnSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CourierPickOrderView view)
            {
                view.RefreshList();
            }
        }

        // --- הלוגיקה הראשית: רענון ומיון הרשימה ---
        public void RefreshList()
        {
            if (CourierId == 0) return;

            try
            {
                // קריאת פרטי שליח
                var courierData = s_bl.Courier.Read(CourierId, CourierId)!;
                CurrentCourier = courierData;

                // קריאת רשימת הזמנות (מהירה, ללא חישובי מסלול כבדים)
                IEnumerable<BO.OpenOrderInList> list = s_bl.Order.ReadAllOpenOrders(CourierId, CourierId);

                // סינון לפי מרחק מקסימלי
                double maxDist = courierData.MaxDistance ?? double.MaxValue;
                list = list.Where(o => o.AirDistance <= maxDist);

                // מיון לפי הבחירה
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
                else
                {
                    // ברירת מחדל ("All")
                    list = list.OrderBy(o => o.OrderId);
                }

                // עדכון המסך
                OrdersList = new ObservableCollection<BO.OpenOrderInList>(list);
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Error: {ex.Message}", "Error", false).ShowDialog();
            }
        }

        // --- כפתורים ---
        private void BtnPick_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BO.OpenOrderInList orderToPick)
            {
                try
                {
                    // 1. ביצוע הפעולה מול ה-BL
                    s_bl.Order.ChooseOrder(CourierId, CourierId, orderToPick.OrderId);

                    // 2. הסרה מהרשימה (כדי שזה ירגיש מהיר)
                    OrdersList.Remove(orderToPick);

                    // 3. הודעת הצלחה מעוצבת
                    new CustomMessageBox($"Order #{orderToPick.OrderId} picked successfully!", "Success", false).ShowDialog();

                    // 4. מעבר אוטומטי למסך הראשי (מונע לקיחת הזמנה נוספת)
                    RequestDashboardView?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    // הודעת שגיאה מעוצבת
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