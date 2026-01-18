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

        string inputIdText = UserId;
        string rawPassword = passwordBox.Password;

        // Validation
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

        try
        {
            BO.UserRole role = s_bl.Courier.Login(idVal, rawPassword);

            // === Admin Login Logic ===
            if (role == BO.UserRole.Admin)
            {
                // בדיקה אם חלון מנהל כבר פתוח
                if (!ActivateWindowIfExists<MainWindow>())
                {
                    new MainWindow().Show();
                }
            }
            // === Courier Login Logic ===
            else if (role == BO.UserRole.Courier)
            {
                BO.Courier? courier = s_bl.Courier.Read(idVal, idVal);
                if (courier != null)
                {
                    // בדיקה אם חלון שליח כבר פתוח (ובודקים שזה אותו שליח!)
                    bool found = false;
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is MainCourierWindow courierWin && courierWin.CurrentCourier?.Id == courier.Id)
                        {
                            if (window.WindowState == WindowState.Minimized)
                                window.WindowState = WindowState.Normal;

                            window.Activate(); // הבאת החלון לקדמה
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