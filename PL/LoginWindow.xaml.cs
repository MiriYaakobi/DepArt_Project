using BlApi;
using PL.Courier;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

public partial class LoginWindow : Window
{
    private IBl s_bl = BlApi.Factory.Get();

    //dependency property for UserId to enable binding
    public string UserId
    {
        get { return (string)GetValue(UserIdProperty); }
        set { SetValue(UserIdProperty, value); }
    }

    public static readonly DependencyProperty UserIdProperty =
        DependencyProperty.Register("UserId", typeof(string), typeof(LoginWindow), new PropertyMetadata(""));

    //commands for buttons
    public ICommand LoginCommand { get; private set; }
    public ICommand ExitCommand { get; private set; }

    // Constructor
    public LoginWindow()
    {
        InitializeComponent();
        this.DataContext = this;

        //register commands
        LoginCommand = new RelayCommand(ExecuteLogin);
        ExitCommand = new RelayCommand(ExecuteExit);

        //check if admin is initialized, if not initialize the database
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
    /// <param name="parameter"></param>
    private void ExecuteLogin(object? parameter)
    {
        if (parameter is not PasswordBox passwordBox)
            return;

        string inputIdText = UserId;
        string rawPassword = passwordBox.Password;

        //validation
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

        //connection chance
        try
        {
            BO.UserRole role = s_bl.Courier.Login(idVal, rawPassword);

            //open the relevant window based on role
            if (role == BO.UserRole.Admin)
                new MainWindow().Show();

            //open courier window
            else if (role == BO.UserRole.Courier)
            {
                BO.Courier? courier = s_bl.Courier.Read(idVal, idVal);

                if (courier != null)
                    new MainCourierWindow(courier.Id).Show();
            }

            //clean up after login
            UserId = "";
            passwordBox.Clear();
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show(ex.Message, "Login Failed", MessageType.Error);
        }
    }

    /// <summary>
    /// exit button logic
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteExit(object? parameter)
    {
        if (CustomMessageBox.ShowQuestion("Are you sure you want to exit?", "Exit System"))
            Application.Current.Shutdown();
    }

    /// <summary>
    /// text input validation for ID textbox - only numbers allowed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxtId_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
    }

    /// <summary>
    /// logic to enable window dragging from anywhere in the window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }
}

/// <summary>
/// helper class for commands
/// </summary>
public class RelayCommand : ICommand
{
    //fields
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// relay command constructor
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="canExecute"></param>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// event to re-evaluate command execution status
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}