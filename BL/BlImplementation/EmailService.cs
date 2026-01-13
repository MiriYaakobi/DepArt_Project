using System;
using System.Net;
using System.Net.Mail;

namespace BlImplementation // שינינו מ-PL ל-BlImplementation
{
    internal static class EmailService
    {
        private const string SenderEmail = "Miri.m.y1984@gmail.com";
        private const string SenderPassword = "oxgyvpxxhpyqefpg";

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
                throw new Exception($"GMAIL ERROR: {ex.Message}");
            }
        }
    }
}