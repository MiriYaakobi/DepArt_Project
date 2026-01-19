using BlApi;
using PL.Courier;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

/// <summary>
/// Login window for the application
/// In writing this class, we used AI to understand the connections between this code and
/// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
/// </summary>
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

        // validate all input fields
        ForceValidation(this);

        string inputIdText = UserId;
        string rawPassword = passwordBox.Password;

        // if ID is empty, focus the first textbox (ID)
        if (string.IsNullOrWhiteSpace(inputIdText))
        {
            var textBox = FindFirstTextBox(this);
            textBox?.Focus();
            return;
        }

        // if password is empty, show message and focus password box
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            CustomMessageBox.Show("Please enter your password.", "Validation Error", MessageType.Warning);
            passwordBox.Focus(); // return focus to password box
            return;
        }

        // validate that ID is a number
        if (!int.TryParse(inputIdText, out int idVal))
        {
            CustomMessageBox.Show("ID must be a number.", "Validation Error", MessageType.Warning);
            return;
        }

        try
        {
            BO.UserRole role = s_bl.Courier.Login(idVal, rawPassword);

            // open the appropriate window based on role
            if (role == BO.UserRole.Admin)
            {
                // Check if MainWindow is already open
                if (!ActivateWindowIfExists<MainWindow>())
                {
                    new MainWindow().Show();
                }
            }
            else if (role == BO.UserRole.Courier) // open courier window
            {
                BO.Courier? courier = s_bl.Courier.Read(idVal, idVal);
                if (courier != null) // should always be true here
                {
                    bool found = false;
                    // Check if MainCourierWindow for this courier is already open
                    foreach (Window window in Application.Current.Windows)
                    {
                        // Check if the window is a MainCourierWindow and matches the logged-in courier
                        if (window is MainCourierWindow courierWin && courierWin.CurrentCourier?.Id == courier.Id)
                        {
                            // If minimized, restore it
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
        // Iterate through open windows to find an existing instance
        foreach (Window window in Application.Current.Windows)
        {
            if (window is TWindow)
            {
                // If minimized, restore it
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
        // go through all children in the logical tree
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is DependencyObject node)
            {
                // if it's a TextBox, update its binding source to trigger validation
                if (node is TextBox tb)
                    tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                // recurse into child elements
                ForceValidation(node);
            }
        }
    }

    /// <summary>
    /// Find the first TextBox in the visual tree.
    /// </summary>
    private TextBox? FindFirstTextBox(DependencyObject parent)
    {
        // go through all children in the logical tree
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            // if it's a DependencyObject, check if it's a TextBox or recurse
            if (child is DependencyObject node)
            {
                if (node is TextBox tb) 
                    return tb;
                var found = FindFirstTextBox(node);
                if (found != null) 
                    return found;
            }
        }
        return null;
    }
} 

/// <summary>
/// helper class for commands
/// </summary>
public class RelayCommand : ICommand
{
    // delegates for execute and canExecute logic
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether the command can be executed.
    /// </summary>
    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    /// <summary>
    /// Executes the command.
    /// </summary>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// Occurs when the ability of the command to execute has changed.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}