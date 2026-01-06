using System.Windows;

namespace PL
{
    public partial class CustomMessageBox : Window
    {
        public bool Result { get; private set; } = false;

        public CustomMessageBox(string message, string title, bool isQuestion)
        {
            InitializeComponent();
            txtMessage.Text = message;
            txtTitle.Text = title;

            if (isQuestion)
            {
                btnYes.Content = "Yes";
                btnNo.Visibility = Visibility.Visible;
            }
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // פונקציה להודעה רגילה
        public static void Show(string message, string title = "Notification")
        {
            CustomMessageBox msg = new CustomMessageBox(message, title, false);
            msg.ShowDialog();
        }

        // פונקציה חדשה לשאלה (מחזירה true אם לחצו כן)
        public static bool ShowQuestion(string message, string title = "Question")
        {
            CustomMessageBox msg = new CustomMessageBox(message, title, true);
            msg.ShowDialog();
            return msg.Result;
        }
    }
}