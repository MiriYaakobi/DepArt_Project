using System;
using System.Windows;
using BO;

namespace PL.Courier;

public partial class CourierWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private int _currentAdminId;

    // משתנה שקובע פעם אחת ולתמיד: האם החלון הזה הוא לעדכון?
    public bool IsUpdateMode { get; private set; }

    // רשימה עבור הקומבו-בוקס (במקום להשתמש ב-x:Name)
    public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryType));

    // האובייקט שאליו אנחנו מבצעים Binding
    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    public CourierWindow(int? courierId = null)
    {
        InitializeComponent();

        try { _currentAdminId = s_bl.Admin.GetConfig().AdminId; } catch { _currentAdminId = 1; }

        if (courierId == null)
        {
            // === מצב הוספה ===
            IsUpdateMode = false; // קובעים שזה מצב הוספה
            CurrentCourier = new BO.Courier();
        }
        else
        {
            // === מצב עדכון ===
            IsUpdateMode = true; // קובעים שזה מצב עדכון
            try
            {
                CurrentCourier = s_bl.Courier.Read(_currentAdminId, courierId.Value)!;
            }
            catch
            {
                Close();
            }
        }

        DataContext = this;
    }

    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsUpdateMode)
            {
                // לוגיקה ברורה של עדכון
                s_bl.Courier.Update(_currentAdminId, CurrentCourier);
                CustomMessageBox.Show("Courier updated successfully!", "Success");
            }
            else
            {
                // לוגיקה ברורה של הוספה
                s_bl.Courier.Create(_currentAdminId, CurrentCourier);
                CustomMessageBox.Show("Courier added successfully!", "Success");
            }
            this.Close();
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"Operation failed: {ex.Message}", "Error");
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier.CurrentOrder != null)
        {
            CustomMessageBox.Show("Cannot delete courier while they have an active order.", "Validation Error");
            return;
        }

        if (CustomMessageBox.ShowQuestion("Are you sure you want to delete this courier?", "Delete Confirmation"))
        {
            try
            {
                s_bl.Courier.Delete(_currentAdminId, CurrentCourier.Id);
                CustomMessageBox.Show("Courier deleted successfully.", "Deleted");
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to delete: {ex.Message}", "Error");
            }
        }
    }

    private void BtnViewOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier.CurrentOrder == null) return;

        // יצירת חלון חדש דינאמית
        Window orderWindow = new Window
        {
            Title = "Order Details",
            Width = 400,
            Height = 550,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            Background = (System.Windows.Media.Brush)FindResource("DeepPurple"), // מסגרת סגולה

            // התוכן הוא ההזמנה עצמה
            Content = CurrentCourier.CurrentOrder,

            // העיצוב נלקח מה-DataTemplate שהגדרנו ב-XAML
            ContentTemplate = (DataTemplate)FindResource("OrderDetailsTemplate")
        };

        // מאפשר סגירה בלחיצה על ESC
        orderWindow.PreviewKeyDown += (s, args) => { if (args.Key == System.Windows.Input.Key.Escape) orderWindow.Close(); };

        orderWindow.ShowDialog(); // פתיחה כחלון מודאלי (חוסם את החלון שמתחתיו)
    }
}