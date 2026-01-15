using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for CourierHistoryView.xaml
    /// </summary>
    public partial class CourierHistoryView : UserControl
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // =================================================================
        // הוספה חדשה (1): משתנה עבור פרטי השליח בסרגל הצד
        // =================================================================
        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierHistoryView));
        // =================================================================


        // מזהה השליח הנוכחי
        public int CourierId
        {
            get { return (int)GetValue(CourierIdProperty); }
            set { SetValue(CourierIdProperty, value); }
        }

        public static readonly DependencyProperty CourierIdProperty =
            DependencyProperty.Register("CourierId", typeof(int), typeof(CourierHistoryView), new PropertyMetadata(0, OnCourierIdChanged));

        // רשימת המשלוחים לתצוגה
        public ObservableCollection<BO.ClosedDeliveryInList> DeliveriesList { get; set; } = new();

        // אפשרויות לסינון ב-ComboBox
        public IEnumerable<BO.OrderEndStatus> StatusOptions { get; } =
            Enum.GetValues(typeof(BO.OrderEndStatus)).Cast<BO.OrderEndStatus>();

        // הפרופרטי שנבחר בסינון
        public BO.OrderEndStatus? SelectedStatusFilter
        {
            get { return (BO.OrderEndStatus?)GetValue(SelectedStatusFilterProperty); }
            set { SetValue(SelectedStatusFilterProperty, value); }
        }

        public static readonly DependencyProperty SelectedStatusFilterProperty =
            DependencyProperty.Register("SelectedStatusFilter", typeof(BO.OrderEndStatus?), typeof(CourierHistoryView));

        // הגדרת אירועים (Events) כדי שהחלון הראשי ידע מתי להחליף מסך
        public event EventHandler? RequestDashboardView;
        public event EventHandler? RequestPickOrderView;

        public CourierHistoryView()
        {
            InitializeComponent();
            DataContext = this;
        }

        // כאשר ה-ID משתנה (למשל בכניסה הראשונה), נטען את הנתונים
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
                // =================================================================
                // הוספה חדשה (2): טעינת פרטי השליח עבור הסרגל
                // =================================================================
                CurrentCourier = s_bl.Courier.Read(CourierId, CourierId)!;
                // =================================================================

                // קריאה לפונקציה ב-BL כפי שהוגדר בממשק
                IEnumerable<BO.ClosedDeliveryInList> list = s_bl.Order.GetClosedDeliveriesForCourier(CourierId, CourierId);

                // סינון לוגי בצד התצוגה (אם נבחר פילטר)
                if (SelectedStatusFilter.HasValue)
                {
                    list = list.Where(d => d.OrderClosedStatus == SelectedStatusFilter.Value);
                }

                // עדכון הרשימה הנצפית
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

        // --- אירועי ממשק משתמש ---

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshList();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SelectedStatusFilter = null;
            RefreshList();
        }

        // --- ניווט ---

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