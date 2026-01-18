using System.Windows;

namespace PL
{
    public enum MessageType { Info, Success, Warning, Error, Question }

    /// <summary>
    /// Custom message box for displaying messages to the user.
    /// In writing this class, we used AI to understand the connections between this code and
    /// the XAML code and to rewrite the code we wrote so that it was accurate and minimal.
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        //setup Dependency Properties for data binding
        public bool Result { get; private set; } = false;

        /// <summary>
        /// Gets or sets the message text displayed in the message box.
        /// </summary>

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string), typeof(CustomMessageBox));

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("TitleText", typeof(string), typeof(CustomMessageBox));

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register("IconText", typeof(string), typeof(CustomMessageBox));

        public string YesButtonContent
        {
            get { return (string)GetValue(YesButtonContentProperty); }
            set { SetValue(YesButtonContentProperty, value); }
        }

        public static readonly DependencyProperty YesButtonContentProperty =
            DependencyProperty.Register("YesButtonContent", typeof(string), typeof(CustomMessageBox), new PropertyMetadata("OK"));

        public Visibility NoButtonVisibility
        {
            get { return (Visibility)GetValue(NoButtonVisibilityProperty); }
            set { SetValue(NoButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NoButtonVisibilityProperty =
            DependencyProperty.Register("NoButtonVisibility", typeof(Visibility), typeof(CustomMessageBox), new PropertyMetadata(Visibility.Collapsed));


        /// <summary>
        /// constructor for CustomMessageBox
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="isQuestion"></param>
        /// <param name="type"></param>
        public CustomMessageBox(string message, string title, bool isQuestion, MessageType type = MessageType.Info)
        {
            InitializeComponent();
            DataContext = this;

            MessageText = message;
            TitleText = title;

            // Set icon based on message type
            switch (type)
            {
                case MessageType.Success: IconText = "✅"; break;
                case MessageType.Warning: IconText = "⚠️"; break;
                case MessageType.Error: IconText = "❌"; break;
                case MessageType.Question: IconText = "❓"; break;
                case MessageType.Info:
                default: IconText = "ℹ️"; break;
            }

            if (isQuestion)
            {
                if (type == MessageType.Info)
                    IconText = "❓";

                YesButtonContent = "Yes";
                NoButtonVisibility = Visibility.Visible;
            }
            else
            {
                YesButtonContent = "OK";
                NoButtonVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// button Click Handlers - YES
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// button Click Handlers - NO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// static method to show a message box
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        public static void Show(string message, string title = "Notification", MessageType type = MessageType.Info)
        {
            new CustomMessageBox(message, title, false, type).ShowDialog();
        }

        /// <summary>
        /// static method to show a question message box
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool ShowQuestion(string message, string title = "Question")
        {
            return new CustomMessageBox(message, title, true, MessageType.Question).ShowDialog() == true;
        }
    }
}