using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;

namespace PL.Courier;

// שינוי קריטי: יורש מ-UserControl ולא מ-Window
public partial class CourierListWindow : UserControl
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int AdminID;

    // אירוע לחזרה לדשבורד (אופציונלי, כדי שהכפתור יעבוד)
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
        CategorySelector.SelectedIndex = 0;
    }

    private void RefreshList()
    {
        try
        {
            if (AdminID == 0) return;
            CourierList = s_bl.Courier.ReadAll(AdminID);
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Error loading data: {ex.Message}", "Error");
        }
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                if (AdminID == 0)
                    return;

                var allCouriers = s_bl.Courier.ReadAll(AdminID);
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

    private void BtnAddCourier_Click(object sender, RoutedEventArgs e)
    {
        OpenCourierWindow(null); // שליחת null = הוספה
        CategorySelector.SelectedIndex = 0; // איפוס הפילטר
    }

    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        // קריאה לאירוע חזרה
        RequestDashboard?.Invoke(this, EventArgs.Empty);
    }

    private void BtnListManagement_Click(object sender, RoutedEventArgs e)
    {
        CategorySelector.SelectedIndex = 0;
    }

    private void OpenCourierWindow(int? id = null)
    {
        // יצירת החלון
        // אם id הוא null - זה הוספה. אם יש מספר - זה עדכון.
        var window = new CourierWindow(id);

        // פתיחת החלון בהמתנה (הקוד יעצור כאן עד שהחלון ייסגר)
        window.ShowDialog();

        // ברגע שהחלון נסגר - מרעננים את הרשימה מהבסיס נתונים
        RefreshList();
    }

    private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is BO.CourierInList selectedCourier)
            OpenCourierWindow(selectedCourier.Id);
    }
}