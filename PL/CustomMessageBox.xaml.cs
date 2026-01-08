using System.Windows;

namespace PL
{
    public enum MessageType { Info, Success, Warning, Error, Question }


    /// <summary>
    /// Represents a custom modal message box window that displays informational messages or questions to the user.
    /// </summary>
    /// <remarks><para> <see cref="CustomMessageBox"/> provides static methods to display simple notification
    /// dialogs or yes/no question dialogs. Use <see cref="Show(string, string)"/> to display an informational message,
    /// or <see cref="ShowQuestion(string, string)"/> to prompt the user for a yes/no response. </para> <para> The
    /// dialog is modal and blocks interaction with other windows until closed. This class is intended for use in WPF
    /// applications. </para></remarks>
    public partial class CustomMessageBox : Window
    {
        // Indicates the result of the dialog; true if "Yes" was clicked, false otherwise.
        public bool Result { get; private set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBox"/> class.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="isQuestion">Indicates whether the message box is a question.</param>
        public CustomMessageBox(string message, string title, bool isQuestion, MessageType type = MessageType.Info)
        {
            InitializeComponent();
            txtMessage.Text = message;
            txtTitle.Text = title;

            // לוגיקת אימוג'ים
            switch (type)
            {
                case MessageType.Success:
                    txtIcon.Text = "✅"; // וי ירוק
                    break;
                case MessageType.Warning:
                    txtIcon.Text = "⚠️"; // משולש אזהרה
                    break;
                case MessageType.Error:
                    txtIcon.Text = "❌"; // איקס אדום
                    break;
                case MessageType.Question:
                    txtIcon.Text = "❓"; // סימן שאלה
                    break;
                case MessageType.Info:
                default:
                    txtIcon.Text = "ℹ️"; // מידע
                    break;
            }

            if (isQuestion)
            {
                // דריסה ידנית למקרה ששכחו לשלוח סוג שאלה
                if (type == MessageType.Info) txtIcon.Text = "❓";

                btnYes.Content = "Yes";
                btnNo.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the click event for the "Yes" button.
        /// </summary>
        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Handles the click event for the "No" button.
        /// </summary>
        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Displays a notification message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        public static void Show(string message, string title = "Notification", MessageType type = MessageType.Info)
        {
            new CustomMessageBox(message, title, false, type).ShowDialog();
        }

        /// <summary>
        /// Displays a question message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        public static bool ShowQuestion(string message, string title = "Question")
        {
            return new CustomMessageBox(message, title, true, MessageType.Question).ShowDialog() == true;
        }
    }
}