using BlApi;
using PL.Courier;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls; // עבור PasswordBox
using System.Windows.Input;

namespace PL
{
    public partial class LoginWindow : Window
    {
        private IBl s_bl = BlApi.Factory.Get();

        // --- 1. Dependency Property עבור תעודת זהות (Binding) ---
        // זה מחליף את הגישה הישירה txtId.Text
        public string UserId
        {
            get { return (string)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register("UserId", typeof(string), typeof(LoginWindow), new PropertyMetadata(""));

        // --- 2. פקודות (Commands) לכפתורים ---
        // זה מחליף את הפונקציות _Click
        public ICommand LoginCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            // הגדרת ה-DataContext לעצמו כדי שה-XAML יכיר את המשתנים
            this.DataContext = this;

            // אתחול הפקודות
            LoginCommand = new RelayCommand(ExecuteLogin);
            ExitCommand = new RelayCommand(ExecuteExit);

            // אתחול DB אם צריך (כמו בקוד הקודם)
            try
            {
                if (s_bl.Admin.GetConfig().AdminId == 0) s_bl.Admin.InitializeDB();
            }
            catch { }
        }

        // --- לוגיקה לכפתור התחברות ---
        private void ExecuteLogin(object? parameter)
        {
            // אנחנו מקבלים את תיבת הסיסמה כפרמטר כי אסור לגשת אליה בשם
            if (parameter is not PasswordBox passwordBox) return;

            string inputIdText = UserId; // לוקחים מה-Dependency Property
            string rawPassword = passwordBox.Password; // לוקחים מהפרמטר

            // בדיקות תקינות
            if (string.IsNullOrWhiteSpace(inputIdText) || string.IsNullOrWhiteSpace(rawPassword))
            {
                CustomMessageBox.Show("Please enter ID and Password.", "Validation Error", MessageType.Warning);
                return;
            }

            if (!int.TryParse(inputIdText, out int idVal))
            {
                CustomMessageBox.Show("ID must be a number.", "Validation Error", MessageType.Warning);
                return;
            }

            // נסיון התחברות
            try
            {
                BO.UserRole role = s_bl.Courier.Login(idVal, rawPassword);

                if (role == BO.UserRole.Admin)
                {
                    new MainWindow().Show();
                    // מחקנו את Close() - החלון יישאר פתוח!
                }
                else if (role == BO.UserRole.Courier)
                {
                    BO.Courier? courier = s_bl.Courier.Read(idVal, idVal);
                    if (courier != null)
                    {
                        new MainCourierWindow(courier.Id).Show();
                    }
                }

                // --- ניקוי השדות למשתמש הבא ---
                UserId = "";         // מנקה את ה-TextBox דרך ה-Binding
                passwordBox.Clear(); // מנקה את הסיסמה
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, "Login Failed", MessageType.Error);
            }
        }

        // --- לוגיקה לכפתור יציאה ---
        private void ExecuteExit(object? parameter)
        {
            if (CustomMessageBox.ShowQuestion("Are you sure you want to exit?", "Exit System"))
            {
                Application.Current.Shutdown();
            }
        }

        // --- אירועי UI (מותרים לפי ההוראות לבדיקת קלט בלבד) ---
        private void TxtId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }

    // --- מחלקת עזר פשוטה ל-Commands (חובה ל-MVVM) ---
    public class RelayCommand : ICommand
    {
        // שינוי: הוספנו '?' כדי להגיד שמותר לקבל null
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}