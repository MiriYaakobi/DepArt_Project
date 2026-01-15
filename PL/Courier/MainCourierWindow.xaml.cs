using System;
using System.Windows;
using System.Windows.Controls;
// ודאי שיש לך את ה-using ל-BO ול-BlApi

namespace PL.Courier
{
    public partial class CourierInterfaceWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private int _courierId;

        // Dependency Property for Binding
        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierInterfaceWindow));

        public CourierInterfaceWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;

            // טעינת רשימת כלי הרכב לקומבו-בוקס
            CmbVehicle.ItemsSource = Enum.GetValues(typeof(BO.DeliveryType));

            RefreshCourierState();
        }

        // פונקציה לטעינה מחדש של נתוני השליח מה-BL
        private void RefreshCourierState()
        {
            try
            {
                CurrentCourier = s_bl.Courier.Read(_courierId);

                // עדכון הנתונים ויזואלית (חשוב לקומבו בוקס של סיום הזמנה)
                // במצב אמת, היינו יוצרים Binding, אבל כאן אני ניגש ישירות לאלמנט בתוך ה-Template
                // הערה: גישה לאלמנטים בתוך DataTemplate דורשת טריק קטן, או פשוט לקשר ב-XAML.
                // לצורך הפשטות, נניח שהלוגיקה של סיום הזמנה מתבצעת כשהכפתור נלחץ.
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error loading courier data: " + ex.Message, "Error");
                this.Close(); // אם אי אפשר לטעון, סוגרים את החלון
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. לוגיקה: בדיקת מרחק מירבי
                // נניח שחוקי החברה הם שהמרחק לא יעלה על המרחק הגלובלי המוגדר
                double companyMaxDistance = s_bl.Admin.GetConfig().MaxDeliveryRange;

                // הערה: בדוגמה הקודמת Range היה TimeSpan, כאן הנחתי מרחק בק"מ.
                // אם אין לך משתנה כזה בקונפיג, אפשר להשתמש במספר קבוע, למשל 500 ק"מ.
                if (CurrentCourier.MaxDistance > 500)
                {
                    CustomMessageBox.Show("Max distance cannot exceed company limit (500 km).", "Validation Error");
                    return;
                }

                // 2. לוגיקה: עדכון סוג שילוח באמצע משלוח
                // אנחנו צריכים לדעת מה היה הרכב הקודם (לפני השינוי ב-UI).
                // מכיוון שה-Binding הוא TwoWay, הנתון ב-CurrentCourier כבר השתנה.
                // הפתרון הנכון: לשלוף את השליח המקורי מהדאטה-בייס להשוואה.
                BO.Courier originalCourier = s_bl.Courier.Read(_courierId);

                if (CurrentCourier.CurrentOrder != null &&
                    CurrentCourier.TypeOfDelivery != originalCourier.TypeOfDelivery)
                {
                    CustomMessageBox.Show("Cannot change vehicle type while delivering an order!", "Operation Denied");

                    // החזרת המצב לקדמותו
                    CurrentCourier.TypeOfDelivery = originalCourier.TypeOfDelivery;
                    return;
                }

                // ביצוע העדכון
                s_bl.Courier.Update(CurrentCourier);
                CustomMessageBox.Show("Details updated successfully!", "Success");
                RefreshCourierState();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Update failed: {ex.Message}", "Error");
            }
        }

        private void BtnFindOrder_Click(object sender, RoutedEventArgs e)
        {
            // פתיחת מסך בחירת הזמנה
            // new SelectOrderWindow(_courierId).ShowDialog();

            // לאחר שהחלון נסגר, מרעננים את המסך הנוכחי כדי לראות אם נבחרה הזמנה
            RefreshCourierState();
        }

        private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
        {
            // כדי לגשת לקומבו-בוקס שנמצא בתוך ה-DataTemplate, צריך למצוא אותו בעץ הויזואלי
            // או פשוט יותר: לקשר את ה-SelectedItem שלו למאפיין ב-CodeBehind.
            // כאן אשתמש בגישה ישירה דרך ה-Sender כי זה קל יותר להבנה:

            if (sender is Button btn && btn.Parent is StackPanel panel)
            {
                // חיפוש הקומבו-בוקס בתוך הפאנל
                ComboBox? cmb = null;
                foreach (var child in panel.Children)
                {
                    if (child is ComboBox c) { cmb = c; break; }
                }

                // בדיקה שנבחר סטטוס
                // כאן אני יוצר רשימה ידנית כי אין לי את ה-Enum המדויק שלך לסטטוס סיום
                // במציאות תקשרי את זה ל-ItemsSource ב-XAML
                if (cmb != null && cmb.SelectedItem == null)
                {
                    // מילוי ה-Combobox אם הוא ריק (תיקון זמני ללוגיקה)
                    cmb.ItemsSource = new[] { BO.OrderEndStatus.Delivered, BO.OrderEndStatus.Refused };
                    CustomMessageBox.Show("Please select a completion status.", "Validation");
                    return;
                }

                if (cmb != null && cmb.SelectedItem != null)
                {
                    try
                    {
                        BO.OrderEndStatus status = (BO.OrderEndStatus)cmb.SelectedItem;

                        // קריאה ל-BL לסיום טיפול
                        s_bl.Order.CompleteDelivery(CurrentCourier.CurrentOrder.Id, status);

                        CustomMessageBox.Show("Order completed successfully!", "Great Job");
                        RefreshCourierState(); // המסך יתעדכן וההזמנה תיעלם
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show($"Failed to complete order: {ex.Message}", "Error");
                    }
                }
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            // פתיחת חלון היסטוריה
            // new CourierHistoryWindow(_courierId).Show();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}