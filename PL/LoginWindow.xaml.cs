using BlApi;
using PL.Courier;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

public partial class LoginWindow : Window
{
    private IBl s_bl = BlApi.Factory.Get();

    // dependency property for UserId to enable binding
    public string UserId
    {
        get { return (string)GetValue(UserIdProperty); }
        set { SetValue(UserIdProperty, value); }
    }

    public static readonly DependencyProperty UserIdProperty =
        DependencyProperty.Register("UserId", typeof(string), typeof(LoginWindow), new PropertyMetadata(""));

    // commands for buttons
    public ICommand LoginCommand { get; private set; }
    public ICommand ExitCommand { get; private set; }

    // Constructor
    public LoginWindow()
    {
        InitializeComponent();
        this.DataContext = this;

        // register commands
        LoginCommand = new RelayCommand(ExecuteLogin);
        ExitCommand = new RelayCommand(ExecuteExit);

        // check if admin is initialized, if not initialize the database
        try
        {
            if (s_bl.Admin.GetConfig().AdminId == 0)
                s_bl.Admin.InitializeDB();
        }
        catch { }
    }

    /// <summary>
    /// logic for login button
    /// </summary>
    private void ExecuteLogin(object? parameter)
    {
        if (parameter is not PasswordBox passwordBox)
            return;

        // 1. הופך את השדות לאדומים אם הם לא תקינים
        ForceValidation(this);

        string inputIdText = UserId;
        string rawPassword = passwordBox.Password;

        // 2. בדיקה מפוצלת:

        // אם התעודת זהות ריקה - אנחנו עוצרים כאן.
        // לא מקפיצים הודעה, כי התיבה כבר אדומה והמשתמש רואה "Field is required".
        if (string.IsNullOrWhiteSpace(inputIdText))
        {
            var textBox = FindFirstTextBox(this);
            textBox?.Focus();
            return;
        }

        // אם הסיסמה ריקה - כאן כן נקפיץ הודעה, כי לסיסמה אין מסגרת אדומה
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            CustomMessageBox.Show("Please enter your password.", "Validation Error", MessageType.Warning);
            passwordBox.Focus(); // נחזיר את המשתמש לכתוב סיסמה
            return;
        }

        // --- מכאן הכל נשאר רגיל ---

        // המרה למספר (למרות שהולידציה בודקת, זה ליתר ביטחון)
        if (!int.TryParse(inputIdText, out int idVal))
        {
            // כאן אפשר גם לעצור בשקט אם רוצים, אבל הודעה זה בסדר כי זה מקרה נדיר
            CustomMessageBox.Show("ID must be a number.", "Validation Error", MessageType.Warning);
            return;
        }

        try
        {
            BO.UserRole role = s_bl.Courier.Login(idVal, rawPassword);

            // ... המשך הקוד המקורי שלך (Admin/Courier) ...
            if (role == BO.UserRole.Admin)
            {
                // וכו'... (להעתיק את שאר הלוגיקה מהקוד הקודם)
                if (!ActivateWindowIfExists<MainWindow>())
                {
                    new MainWindow().Show();
                }
            }
            else if (role == BO.UserRole.Courier)
            {
                // ...
                BO.Courier? courier = s_bl.Courier.Read(idVal, idVal);
                if (courier != null)
                {
                    // לוגיקת פתיחת חלון שליח...
                    bool found = false;
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is MainCourierWindow courierWin && courierWin.CurrentCourier?.Id == courier.Id)
                        {
                            if (window.WindowState == WindowState.Minimized)
                                window.WindowState = WindowState.Normal;

                            window.Activate();
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        new MainCourierWindow(courier.Id).Show();
                    }
                }
            }

            // Cleanup
            UserId = "";
            passwordBox.Clear();
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show(ex.Message, "Login Failed", MessageType.Error);
        }
    }

    /// <summary>
    /// Helper method to activate an existing window or return false if not found.
    /// </summary>
    private bool ActivateWindowIfExists<TWindow>() where TWindow : Window
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is TWindow)
            {
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;

                window.Activate(); // Bring to front
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// exit button logic
    /// </summary>
    private void ExecuteExit(object? parameter)
    {
        if (CustomMessageBox.ShowQuestion("Are you sure you want to exit?", "Exit System"))
            Application.Current.Shutdown();
    }

    /// <summary>
    /// text input validation for ID textbox - only numbers allowed
    /// </summary>
    private void TxtId_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
    }

    /// <summary>
    /// logic to enable window dragging from anywhere in the window
    /// </summary>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    /// <summary>
    /// Recursive method to force validation on all input fields in the window
    /// </summary>
    private void ForceValidation(DependencyObject parent)
    {
        // עוברים על כל הילדים של האלמנט הנוכחי
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is DependencyObject node)
            {
                // אם מצאנו TextBox - נפעיל את הולידציה שלו
                if (node is TextBox tb)
                {
                    tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                }

                // ממשיכים לחפש עמוק יותר (רקורסיה)
                ForceValidation(node);
            }
        }
    }

    private TextBox? FindFirstTextBox(DependencyObject parent)
    {
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is DependencyObject node)
            {
                if (node is TextBox tb) return tb;
                var found = FindFirstTextBox(node);
                if (found != null) return found;
            }
        }
        return null;
    }
} // --- סוף המחלקה LoginWindow ---

// --- תחילת המחלקה RelayCommand (מחוץ ל-LoginWindow!) ---
/// <summary>
/// helper class for commands
/// </summary>
public class RelayCommand : ICommand
{
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