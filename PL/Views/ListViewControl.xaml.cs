using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;

namespace PL.Views
{
    public partial class ListViewControl : UserControl
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public event EventHandler? RequestDashboard;
        private int _adminId;

        public IEnumerable<BO.CourierInList> CourierList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
            set { SetValue(CourierListProperty, value); }
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(ListViewControl), new PropertyMetadata(null));

        public ListViewControl()
        {
            InitializeComponent();

            // --- 1. בניית רשימת הסינון (All + Enum) ---
            var filterOptions = new List<object>();
            filterOptions.Add("All"); // הוספת "All" ידנית
            filterOptions.AddRange(Enum.GetValues(typeof(BO.DeliveryType)).Cast<object>());

            CategorySelector.ItemsSource = filterOptions;

            // שימי לב: מחקתי מפה את השורה שבוחרת את האינדקס 0
            // כדי למנוע קריאה לשרת לפני שיש לנו ID
        }

        public void Initialize(int adminId)
        {
            _adminId = adminId;

            // --- 2. עכשיו בטוח לבחור "All" ולטעון נתונים ---
            // זה יפעיל את ה-SelectionChanged שיקרא ל-RefreshList
            CategorySelector.SelectedIndex = 0;
        }

        private void RefreshList()
        {
            try
            {
                // הגנה נוספת: אם משום מה אין ID, לא לפנות לשרת
                if (_adminId == 0) return;

                CourierList = s_bl.Courier.ReadAll(_adminId);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error loading data: {ex.Message}", "Error");
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // אם לא נבחר כלום או "All", מציגים הכל
            if (CategorySelector.SelectedItem == null || CategorySelector.SelectedItem.ToString() == "All")
            {
                RefreshList();
                return;
            }

            try
            {
                if (CategorySelector.SelectedItem is BO.DeliveryType selectedType)
                {
                    // הגנה: וודא שיש ID לפני קריאה
                    if (_adminId == 0) return;

                    var allCouriers = s_bl.Courier.ReadAll(_adminId);
                    CourierList = from item in allCouriers
                                  where item.TypeOfDelivery == selectedType
                                  select item;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Filter Error: {ex.Message}", "Error");
                RefreshList(); // חזרה למצב ברירת מחדל במקרה תקלה
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            RequestDashboard?.Invoke(this, EventArgs.Empty);
        }

        private void BtnListManagement_Click(object sender, RoutedEventArgs e)
        {
            CategorySelector.SelectedIndex = 0; // חזרה ל-"All"
        }

        private void BtnAddCourier_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.Show("Add Courier Window will be implemented in Stage 6!", "Coming Soon");
        }
    }
}