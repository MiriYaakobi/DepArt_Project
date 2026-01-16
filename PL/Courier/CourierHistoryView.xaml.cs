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

        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierHistoryView));

        public int CourierId
        {
            get { return (int)GetValue(CourierIdProperty); }
            set { SetValue(CourierIdProperty, value); }
        }
        public static readonly DependencyProperty CourierIdProperty =
            DependencyProperty.Register("CourierId", typeof(int), typeof(CourierHistoryView), new PropertyMetadata(0, OnCourierIdChanged));

        public ObservableCollection<BO.ClosedDeliveryInList> DeliveriesList { get; set; } = new();

        public IEnumerable<object> StatusOptions { get; } =
            new List<object> { "All" }
            .Concat(Enum.GetValues(typeof(BO.OrderEndStatus)).Cast<object>())
            .ToList();

        public object SelectedStatusFilter
        {
            get { return GetValue(SelectedStatusFilterProperty); }
            set { SetValue(SelectedStatusFilterProperty, value); }
        }
        public static readonly DependencyProperty SelectedStatusFilterProperty =
            DependencyProperty.Register("SelectedStatusFilter", typeof(object), typeof(CourierHistoryView), new PropertyMetadata("All"));

        public event EventHandler? RequestDashboardView;
        public event EventHandler? RequestPickOrderView;

        public CourierHistoryView()
        {
            InitializeComponent();
            DataContext = this;
            SelectedStatusFilter = "All";

            // === הוספת האזנה לעדכונים אוטומטיים ===
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
        }

        // פונקציית האזנה - תופעל כשמשהו משתנה במערכת
        private void OrderObserver()
        {
            Dispatcher.Invoke(() => RefreshList());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.AddObserver(OrderObserver); // הרשמה
            RefreshList();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.RemoveObserver(OrderObserver); // ביטול הרשמה
        }
        // ==========================================

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
                CurrentCourier = s_bl.Courier.Read(CourierId, CourierId)!;

                IEnumerable<BO.ClosedDeliveryInList> list = s_bl.Order.GetClosedDeliveriesForCourier(CourierId, CourierId);

                if (SelectedStatusFilter is BO.OrderEndStatus statusEnum)
                {
                    list = list.Where(d => d.OrderClosedStatus == statusEnum);
                }

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