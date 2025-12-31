using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;
using PL.Courier; // לוודא שזה קיים כדי להכיר את CourierWindow

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

            var filterOptions = new List<object>();
            filterOptions.Add("All");
            filterOptions.AddRange(Enum.GetValues(typeof(BO.DeliveryType)).Cast<object>());

            CategorySelector.ItemsSource = filterOptions;
        }

        public void Initialize(int adminId)
        {
            _adminId = adminId;
            CategorySelector.SelectedIndex = 0;
        }

        private void RefreshList()
        {
            try
            {
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
            if (CategorySelector.SelectedItem == null || CategorySelector.SelectedItem.ToString() == "All")
            {
                RefreshList();
                return;
            }

            try
            {
                if (CategorySelector.SelectedItem is BO.DeliveryType selectedType)
                {
                    if (_adminId == 0) return;
                    var allCouriers = s_bl.Courier.ReadAll(_adminId);
                    CourierList = from item in allCouriers
                                  where item.TypeOfDelivery == selectedType
                                  select item;
                }
            }
            catch
            {
                RefreshList();
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            RequestDashboard?.Invoke(this, EventArgs.Empty);
        }

        private void BtnListManagement_Click(object sender, RoutedEventArgs e)
        {
            CategorySelector.SelectedIndex = 0;
        }

        // --- הפונקציה הנכונה (וודאי שאין עוד אחת בשם הזה בקובץ) ---
        private void BtnAddCourier_Click(object sender, RoutedEventArgs e)
        {
            new CourierWindow().ShowDialog();
            RefreshList();
        }
    }
}