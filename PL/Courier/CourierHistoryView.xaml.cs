using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{
    public partial class CourierHistoryView : UserControl
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // משתנה עבור פרטי השליח בסרגל הצד
        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierHistoryView));

        // מזהה השליח
        public int CourierId
        {
            get { return (int)GetValue(CourierIdProperty); }
            set { SetValue(CourierIdProperty, value); }
        }
        public static readonly DependencyProperty CourierIdProperty =
            DependencyProperty.Register("CourierId", typeof(int), typeof(CourierHistoryView), new PropertyMetadata(0, OnCourierIdChanged));

        // רשימת המשלוחים לתצוגה
        public ObservableCollection<BO.ClosedDeliveryInList> DeliveriesList { get; set; } = new();

        // רשימת הסינון - כוללת את "All" + ה-Enum
        // שיניתי ל-IEnumerable<object> כדי שיוכל להכיל גם סטרינג וגם Enum
        public IEnumerable<object> StatusOptions { get; } =
            new List<object> { "All" }
            .Concat(Enum.GetValues(typeof(BO.OrderEndStatus)).Cast<object>())
            .ToList();

        // הבחירה הנוכחית בסינון
        public object SelectedStatusFilter
        {
            get { return GetValue(SelectedStatusFilterProperty); }
            set { SetValue(SelectedStatusFilterProperty, value); }
        }
        public static readonly DependencyProperty SelectedStatusFilterProperty =
            DependencyProperty.Register("SelectedStatusFilter", typeof(object), typeof(CourierHistoryView), new PropertyMetadata("All")); // ברירת מחדל "All"

        // אירועי ניווט
        public event EventHandler? RequestDashboardView;
        public event EventHandler? RequestPickOrderView;

        public CourierHistoryView()
        {
            InitializeComponent();
            DataContext = this;
            SelectedStatusFilter = "All"; // שמים ברירת מחדל
        }

        private static void OnCourierIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CourierHistoryView view)
            {
                view.RefreshList();
            }
        }

        public void RefreshList()
        {
            if (CourierId == 0) return;

            try
            {
                // טעינת פרטי השליח לסרגל
                CurrentCourier = s_bl.Courier.Read(CourierId, CourierId)!;

                // שליפת כל המשלוחים
                IEnumerable<BO.ClosedDeliveryInList> list = s_bl.Order.GetClosedDeliveriesForCourier(CourierId, CourierId);

                // --- לוגיקת הסינון החדשה ---
                // אם זה לא "All" וגם לא null -> תסנן
                if (SelectedStatusFilter is BO.OrderEndStatus statusEnum)
                {
                    list = list.Where(d => d.OrderClosedStatus == statusEnum);
                }
                // אם זה "All" (כסטרינג) או null -> אל תעשה כלום (תציג הכל)

                // עדכון הרשימה
                DeliveriesList.Clear();
                foreach (var item in list)
                {
                    DeliveriesList.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching deliveries: {ex.Message}");
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshList();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SelectedStatusFilter = "All"; // חזרה למצב הכל
            RefreshList();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            RequestDashboardView?.Invoke(this, EventArgs.Empty);
        }

        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            RequestPickOrderView?.Invoke(this, EventArgs.Empty);
        }
    }
}