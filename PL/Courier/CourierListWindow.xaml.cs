using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;

namespace PL.Courier;

public partial class CourierListWindow : UserControl
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int AdminID;

    // תיקון אזהרה 1: הוספנו '?' כדי לאפשר ערך ריק (null) בהתחלה
    private IEnumerable<BO.CourierInList>? AllCouriers;

    public event EventHandler? RequestDashboard;

    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }

    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    public CourierListWindow(int adminId)
    {
        InitializeComponent();
        AdminID = adminId;

        var filterOptions = new List<object>();
        filterOptions.Add("All");
        filterOptions.AddRange(Enum.GetValues(typeof(BO.DeliveryType)).Cast<object>());

        CategorySelector.ItemsSource = filterOptions;

        LoadData();
        CategorySelector.SelectedIndex = 0;
    }

    private void LoadData()
    {
        try
        {
            if (AdminID == 0) return;

            // טעינת הנתונים לזיכרון
            AllCouriers = s_bl.Courier.ReadAll(AdminID);

            // הפעלת הסינון
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error");
        }
    }

    private void ApplyFilters()
    {
        // אם הרשימה הראשית ריקה, אין מה לסנן
        if (AllCouriers == null) return;

        var tempAddList = AllCouriers;

        // 1. סינון לפי קטגוריה
        if (CategorySelector.SelectedItem is BO.DeliveryType selectedType)
        {
            tempAddList = tempAddList.Where(item => item.TypeOfDelivery == selectedType);
        }

        // 2. סינון לפי טקסט חיפוש
        string searchText = SearchBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // תיקון אזהרה 2: בודקים ש-item.Name לא ריק לפני שעושים עליו חיפוש
            tempAddList = tempAddList.Where(item =>
                !string.IsNullOrEmpty(item.Name) &&
                item.Name.ToLower().Contains(searchText.ToLower()));
        }

        // עדכון התצוגה (המרה לרשימה בסוף התהליך)
        CourierList = tempAddList.ToList();
    }

    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void BtnAddCourier_Click(object sender, RoutedEventArgs e)
    {
        OpenCourierWindow(null);
        SearchBox.Text = "";
        CategorySelector.SelectedIndex = 0;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is BO.CourierInList courierToDelete)
        {
            if (MessageBox.Show($"Are you sure you want to delete {courierToDelete.Name}?",
                                "Delete Courier",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Courier.Delete(courierToDelete.Id, AdminID);
                    LoadData(); // טעינה מחדש מהמסד כדי לרענן גם את ה-Cache
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete: {ex.Message}", "Error");
                }
            }
        }
    }

    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        RequestDashboard?.Invoke(this, EventArgs.Empty);
    }

    private void BtnListManagement_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        CategorySelector.SelectedIndex = 0;
    }

    private void OpenCourierWindow(int? id = null)
    {
        var window = new CourierWindow(id);
        // כשהחלון הזה ייסגר מתישהו בעתיד - תבצע רענון לרשימה
        window.Closed += (s, args) => LoadData();
        window.Show();
    }

    private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is BO.CourierInList selectedCourier)
            OpenCourierWindow(selectedCourier.Id);
    }
}