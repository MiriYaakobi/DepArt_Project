using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Courier
{
    public partial class MainCourierWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private int _courierId;
        private string _enteredPassword = ""; // הסיסמה האמיתית (נסתרת)

        // 1. יצירת Dependency Property לטקסט שמופיע בתיבה (הכוכביות)
        public string VisualPassword
        {
            get { return (string)GetValue(VisualPasswordProperty); }
            set { SetValue(VisualPasswordProperty, value); }
        }

        public static readonly DependencyProperty VisualPasswordProperty =
            DependencyProperty.Register("VisualPassword", typeof(string), typeof(MainCourierWindow), new PropertyMetadata("********"));

        // מאפיין שמחזיק את כל סוגי המשלוח (אופנוע, רכב וכו')
        public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryType));

        // Dependency Property לפרטי השליח
        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(MainCourierWindow));

        public MainCourierWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;
            DataContext = this; // חובה!
            RefreshCourierState();
        }

        private void RefreshCourierState()
        {
            try
            {
                CurrentCourier = s_bl.Courier.Read(_courierId, _courierId)!;

                // 2. איפוס התצוגה לכוכביות והסיסמה האמיתית לריקה
                VisualPassword = "********";
                _enteredPassword = "";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error loading data: " + ex.Message, "Error");
                this.Close();
            }
        }

        // --- לוגיקת הכוכביות הידנית (עובדת עם sender וללא שמות) ---

        private void Password_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // אם זו ההקלדה הראשונה והתיבה מכילה את ברירת המחדל, נאפס אותה
            if (VisualPassword == "********")
            {
                VisualPassword = "";
                _enteredPassword = "";
            }

            // הוספת התו לסיסמה האמיתית
            _enteredPassword += e.Text;

            // עדכון התצוגה (הוספת כוכבית)
            // שימי לב: בגלל שיש Binding, עדכון המאפיין יעדכן את המסך
            // אבל כאן אנחנו מעדכנים ישירות את הטקסט כדי לשלוט בסמן
            if (sender is TextBox txt)
            {
                txt.Text += "●";
                txt.CaretIndex = txt.Text.Length; // הזזת הסמן לסוף
            }

            e.Handled = true; // מונע מהאות האמיתית להופיע
        }

        private void Password_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt == null) return;

            // אם התיבה מכילה את ברירת המחדל, כל לחיצה מוחקת אותה
            if (VisualPassword == "********")
            {
                VisualPassword = "";
                _enteredPassword = "";
                // לא עושים return כי אולי זה היה Backspace שצריך לבצע
            }

            if (e.Key == Key.Back)
            {
                if (_enteredPassword.Length > 0)
                {
                    _enteredPassword = _enteredPassword.Substring(0, _enteredPassword.Length - 1);

                    if (txt.Text.Length > 0)
                    {
                        txt.Text = txt.Text.Substring(0, txt.Text.Length - 1);
                        txt.CaretIndex = txt.Text.Length;
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                _enteredPassword += " ";
                txt.Text += "●";
                txt.CaretIndex = txt.Text.Length;
                e.Handled = true;
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BO.Courier savedCourier = s_bl.Courier.Read(_courierId, _courierId)!;

                double? companyLimit = s_bl.Admin.GetConfig().DeliveryMaxDistance;
                if (companyLimit.HasValue && CurrentCourier.MaxDistance.HasValue &&
                    CurrentCourier.MaxDistance.Value > companyLimit.Value)
                {
                    CustomMessageBox.Show($"Shipping max distance must be less than or equal to the company's shipping distance.", "Error");
                    return;
                }

                if (savedCourier.CurrentOrder != null &&
                    savedCourier.TypeOfDelivery != CurrentCourier.TypeOfDelivery)
                {
                    CustomMessageBox.Show("Cannot change vehicle...", "Error");
                    CurrentCourier.TypeOfDelivery = savedCourier.TypeOfDelivery;
                    return;
                }

                if (!string.IsNullOrEmpty(_enteredPassword))
                {
                    CurrentCourier.Password = _enteredPassword;
                }
                else
                {
                    BO.Courier originalCourierFromDb =
                        s_bl.Courier.Read(CurrentCourier.Id, CurrentCourier.Id)!;

                    CurrentCourier.Password = originalCourierFromDb.Password;
                }

                s_bl.Courier.Update(_courierId, CurrentCourier);
                CustomMessageBox.Show("Profile updated successfully!", "Success");

                RefreshCourierState();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Update failed: {ex.Message}", "Error");
            }
        }

        // ... שאר הפונקציות (FindOrder, FinishOrder, Logout) ללא שינוי ...
        private void BtnFindOrder_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnFinishOrder_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnHistory_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}