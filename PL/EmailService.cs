using System;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace PL
{
    public static class EmailService
    {
        // מדריך: https://support.google.com/accounts/answer/185833
        private const string SenderEmail = "Miri.m.y1984@gmail.com";
        private const string SenderPassword = "YOUR_APP_PASSWORD"; // סיסמת אפליקציה (16 תווים)

        public static void SendNotification(string recipientEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(recipientEmail)) return;

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SenderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(recipientEmail);

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // אנחנו לא רוצים שהתוכנה תקרוס אם המייל נכשל, אז רק נציג שגיאה
                // (במצב אמת היינו רושמים ללוג)
                Console.WriteLine("Email failed: " + ex.Message);
            }
        }
    }
}